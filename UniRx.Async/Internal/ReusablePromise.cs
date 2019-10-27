using System.Threading;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Runtime.ExceptionServices;

namespace UniRx.Async.Internal
{
    // public for some types uses it.

    public abstract class ReusablePromise : IAwaiter
    {
        private object continuation; // Action or Queue<Action>
        private ExceptionDispatchInfo exception;

        public UniTask Task => new UniTask(this);

        // can override for control 'start/reset' timing.
        public virtual bool IsCompleted => Status.IsCompleted();

        public AwaiterStatus Status { get; private set; }

        void IAwaiter.GetResult()
        {
            GetResult();
        }

        public void OnCompleted(Action action)
        {
            UnsafeOnCompleted(action);
        }

        public void UnsafeOnCompleted(Action action)
        {
            if (continuation == null)
            {
                continuation = action;
            }
            else
            {
                if (continuation is Action act)
                {
                    var q = new MinimumQueue<Action>(4);
                    q.Enqueue(act);
                    q.Enqueue(action);
                    continuation = q;
                }
                else
                {
                    ((MinimumQueue<Action>) continuation).Enqueue(action);
                }
            }
        }

        public virtual void GetResult()
        {
            switch (Status)
            {
                case AwaiterStatus.Succeeded:
                    return;
                case AwaiterStatus.Faulted:
                    exception.Throw();
                    break;
                case AwaiterStatus.Canceled:
                    throw new OperationCanceledException();
            }

            throw new InvalidOperationException("Invalid Status:" + Status);
        }

        public void ResetStatus(bool forceReset)
        {
            if (forceReset)
                Status = AwaiterStatus.Pending;
            else if (Status == AwaiterStatus.Succeeded) Status = AwaiterStatus.Pending;
        }

        public virtual bool TrySetCanceled()
        {
            if (Status == AwaiterStatus.Pending)
            {
                Status = AwaiterStatus.Canceled;
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public virtual bool TrySetException(Exception ex)
        {
            if (Status == AwaiterStatus.Pending)
            {
                Status = AwaiterStatus.Faulted;
                exception = ExceptionDispatchInfo.Capture(ex);
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public virtual bool TrySetResult()
        {
            if (Status == AwaiterStatus.Pending)
            {
                Status = AwaiterStatus.Succeeded;
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        private void TryInvokeContinuation()
        {
            if (continuation == null) return;

            if (continuation is Action act)
            {
                continuation = null;
                act();
            }
            else
            {
                // reuse Queue(don't null clear)
                var q = (MinimumQueue<Action>) continuation;
                var size = q.Count;
                for (var i = 0; i < size; i++) q.Dequeue().Invoke();
            }
        }
    }

    public abstract class ReusablePromise<T> : IAwaiter<T>
    {
        private object continuation; // Action or Queue<Action>
        private ExceptionDispatchInfo exception;

        public UniTask<T> Task => new UniTask<T>(this);

        protected T RawResult { get; private set; }

        // can override for control 'start/reset' timing.
        public virtual bool IsCompleted => Status.IsCompleted();

        public virtual T GetResult()
        {
            switch (Status)
            {
                case AwaiterStatus.Succeeded:
                    return RawResult;
                case AwaiterStatus.Faulted:
                    exception.Throw();
                    break;
                case AwaiterStatus.Canceled:
                    throw new OperationCanceledException();
            }

            throw new InvalidOperationException("Invalid Status:" + Status);
        }

        public AwaiterStatus Status { get; private set; }

        void IAwaiter.GetResult()
        {
            GetResult();
        }

        public void OnCompleted(Action action)
        {
            UnsafeOnCompleted(action);
        }

        public void UnsafeOnCompleted(Action action)
        {
            if (continuation == null)
            {
                continuation = action;
            }
            else
            {
                if (continuation is Action act)
                {
                    var q = new MinimumQueue<Action>(4);
                    q.Enqueue(act);
                    q.Enqueue(action);
                    continuation = q;
                }
                else
                {
                    ((MinimumQueue<Action>) continuation).Enqueue(action);
                }
            }
        }

        protected void ForceSetResult(T result)
        {
            RawResult = result;
        }

        public void ResetStatus(bool forceReset)
        {
            if (forceReset)
                Status = AwaiterStatus.Pending;
            else if (Status == AwaiterStatus.Succeeded) Status = AwaiterStatus.Pending;
        }

        public virtual bool TrySetCanceled()
        {
            if (Status == AwaiterStatus.Pending)
            {
                Status = AwaiterStatus.Canceled;
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public virtual bool TrySetException(Exception ex)
        {
            if (Status == AwaiterStatus.Pending)
            {
                Status = AwaiterStatus.Faulted;
                exception = ExceptionDispatchInfo.Capture(ex);
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public virtual bool TrySetResult(T result)
        {
            if (Status == AwaiterStatus.Pending)
            {
                Status = AwaiterStatus.Succeeded;
                RawResult = result;
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        protected void TryInvokeContinuation()
        {
            if (continuation == null) return;

            if (continuation is Action act)
            {
                continuation = null;
                act();
            }
            else
            {
                // reuse Queue(don't null clear)
                var q = (MinimumQueue<Action>) continuation;
                var size = q.Count;
                for (var i = 0; i < size; i++) q.Dequeue().Invoke();
            }
        }
    }

    public abstract class PlayerLoopReusablePromiseBase : ReusablePromise, IPlayerLoopItem
    {
        protected readonly CancellationToken cancellationToken;

#if UNITY_EDITOR
        private readonly string capturedStackTraceForDebugging;
#endif
        private readonly PlayerLoopTiming timing;
        private bool isRunning;

        public override bool IsCompleted
        {
            get
            {
                if (Status == AwaiterStatus.Canceled || Status == AwaiterStatus.Faulted) return true;

                if (!isRunning)
                {
                    isRunning = true;
                    ResetStatus(false);
                    OnRunningStart();
#if UNITY_EDITOR
                    TaskTracker.TrackActiveTask(this, capturedStackTraceForDebugging);
#endif
                    PlayerLoopHelper.AddAction(timing, this);
                }

                return false;
            }
        }

        public PlayerLoopReusablePromiseBase(PlayerLoopTiming timing, CancellationToken cancellationToken, int skipTrackFrameCountAdditive)
        {
            this.timing = timing;
            this.cancellationToken = cancellationToken;

#if UNITY_EDITOR
            capturedStackTraceForDebugging = TaskTracker.CaptureStackTrace(skipTrackFrameCountAdditive + 1); // 1 is self,
#endif
        }

        public abstract bool MoveNext();

        protected abstract void OnRunningStart();

        protected void Complete()
        {
            isRunning = false;
#if UNITY_EDITOR
            TaskTracker.RemoveTracking(this);
#endif
        }
    }

    public abstract class PlayerLoopReusablePromiseBase<T> : ReusablePromise<T>, IPlayerLoopItem
    {
        protected readonly CancellationToken cancellationToken;

#if UNITY_EDITOR
        private readonly string capturedStackTraceForDebugging;
#endif
        private readonly PlayerLoopTiming timing;
        private bool isRunning;

        public override bool IsCompleted
        {
            get
            {
                if (Status == AwaiterStatus.Canceled || Status == AwaiterStatus.Faulted) return true;

                if (!isRunning)
                {
                    isRunning = true;
                    ResetStatus(false);
                    OnRunningStart();
#if UNITY_EDITOR
                    TaskTracker.TrackActiveTask(this, capturedStackTraceForDebugging);
#endif
                    PlayerLoopHelper.AddAction(timing, this);
                }

                return false;
            }
        }

        public PlayerLoopReusablePromiseBase(PlayerLoopTiming timing, CancellationToken cancellationToken, int skipTrackFrameCountAdditive)
        {
            this.timing = timing;
            this.cancellationToken = cancellationToken;

#if UNITY_EDITOR
            capturedStackTraceForDebugging = TaskTracker.CaptureStackTrace(skipTrackFrameCountAdditive + 1); // 1 is self,
#endif
        }

        public abstract bool MoveNext();

        protected abstract void OnRunningStart();

        protected void Complete()
        {
            isRunning = false;
#if UNITY_EDITOR
            TaskTracker.RemoveTracking(this);
#endif
        }
    }
}
#endif
