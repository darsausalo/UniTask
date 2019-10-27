using System;
using UniRx.Async.Internal;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6)) && ENABLE_MANAGED_JOBS
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Threading;
using Unity.Jobs;

namespace UniRx.Async
{
    public static partial class UnityAsyncExtensions
    {
        public static IAwaiter GetAwaiter(this JobHandle jobHandle)
        {
            var awaiter = new JobHandleAwaiter(jobHandle, CancellationToken.None);
            if (!awaiter.IsCompleted)
            {
                PlayerLoopHelper.AddAction(PlayerLoopTiming.EarlyUpdate, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.PreUpdate, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.PreLateUpdate, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.PostLateUpdate, awaiter);
            }

            return awaiter;
        }

        public static UniTask ToUniTask(this JobHandle jobHandle, CancellationToken cancellation = default)
        {
            var awaiter = new JobHandleAwaiter(jobHandle, cancellation);
            if (!awaiter.IsCompleted)
            {
                PlayerLoopHelper.AddAction(PlayerLoopTiming.EarlyUpdate, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.PreUpdate, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.PreLateUpdate, awaiter);
                PlayerLoopHelper.AddAction(PlayerLoopTiming.PostLateUpdate, awaiter);
            }

            return new UniTask(awaiter);
        }

        public static UniTask ConfigureAwait(this JobHandle jobHandle, PlayerLoopTiming waitTiming, CancellationToken cancellation = default)
        {
            var awaiter = new JobHandleAwaiter(jobHandle, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(waitTiming, awaiter);
            return new UniTask(awaiter);
        }

        private class JobHandleAwaiter : IAwaiter, IPlayerLoopItem
        {
            private CancellationToken cancellationToken;
            private Action continuation;
            private JobHandle jobHandle;

            public JobHandleAwaiter(JobHandle jobHandle, CancellationToken cancellationToken, int skipFrame = 2)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : jobHandle.IsCompleted ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompleted()) return;

                this.jobHandle = jobHandle;
                this.cancellationToken = cancellationToken;
                Status = AwaiterStatus.Pending;
                continuation = null;

                TaskTracker.TrackActiveTask(this, skipFrame);
            }

            public bool IsCompleted => Status.IsCompleted();

            public AwaiterStatus Status { get; private set; }

            public void GetResult()
            {
                if (Status == AwaiterStatus.Succeeded)
                    return;
                if (Status == AwaiterStatus.Canceled) Error.ThrowOperationCanceledException();

                Error.ThrowNotYetCompleted();
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    // Call jobHandle.Complete after finished.
                    PlayerLoopHelper.AddAction(PlayerLoopTiming.EarlyUpdate, new JobHandleAwaiter(jobHandle, CancellationToken.None, 1));
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (jobHandle.IsCompleted)
                {
                    jobHandle.Complete();
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                jobHandle = default;

                if (cont != null) cont.Invoke();
            }
        }
    }
}

#endif
