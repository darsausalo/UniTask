using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UniRx.Async.Internal;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Runtime.ExceptionServices;

namespace UniRx.Async
{
    internal class ExceptionHolder
    {
        private readonly ExceptionDispatchInfo exception;
        private bool calledGet;

        public ExceptionHolder(ExceptionDispatchInfo exception)
        {
            this.exception = exception;
        }

        public ExceptionDispatchInfo GetException()
        {
            if (!calledGet)
            {
                calledGet = true;
                GC.SuppressFinalize(this);
            }

            return exception;
        }

        ~ExceptionHolder()
        {
            UniTaskScheduler.PublishUnobservedTaskException(exception.SourceException);
        }
    }

    public interface IResolvePromise
    {
        bool TrySetResult();
    }

    public interface IResolvePromise<T>
    {
        bool TrySetResult(T value);
    }

    public interface IRejectPromise
    {
        bool TrySetException(Exception exception);
    }

    public interface ICancelPromise
    {
        bool TrySetCanceled();
    }

    public interface IPromise<T> : IResolvePromise<T>, IRejectPromise, ICancelPromise
    {
    }

    public interface IPromise : IResolvePromise, IRejectPromise, ICancelPromise
    {
    }

    public class UniTaskCompletionSource : IAwaiter, IPromise
    {
        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;
        private object continuation; // action or list
        private ExceptionHolder exception;
        private bool handled;

        private int state;

        public UniTask Task => new UniTask(this);

        public UniTaskCompletionSource()
        {
            TaskTracker.TrackActiveTask(this, 2);
        }

        AwaiterStatus IAwaiter.Status => (AwaiterStatus) state;

        bool IAwaiter.IsCompleted => state != Pending;

        void IAwaiter.GetResult()
        {
            MarkHandled();

            if (state == Succeeded)
            {
            }
            else if (state == Faulted)
            {
                exception.GetException().Throw();
            }
            else if (state == Canceled)
            {
                if (exception != null) exception.GetException().Throw(); // guranteed operation canceled exception.

                throw new OperationCanceledException();
            }
            else // Pending
            {
                throw new NotSupportedException("UniTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            if (Interlocked.CompareExchange(ref continuation, action, null) == null)
            {
                if (state != Pending) TryInvokeContinuation();
            }
            else
            {
                var c = continuation;
                if (c is Action)
                {
                    var list = new List<Action>();
                    list.Add((Action) c);
                    list.Add(action);
                    if (Interlocked.CompareExchange(ref continuation, list, c) == c) goto TRYINVOKE;
                }

                var l = (List<Action>) continuation;
                lock (l)
                {
                    l.Add(action);
                }

                TRYINVOKE:
                if (state != Pending) TryInvokeContinuation();
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(continuation);
        }

        public bool TrySetResult()
        {
            if (Interlocked.CompareExchange(ref state, Succeeded, Pending) == Pending)
            {
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public bool TrySetException(Exception exception)
        {
            if (Interlocked.CompareExchange(ref state, Faulted, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public bool TrySetCanceled()
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        [Conditional("UNITY_EDITOR")]
        internal void MarkHandled()
        {
            if (!handled)
            {
                handled = true;
                TaskTracker.RemoveTracking(this);
            }
        }

        private void TryInvokeContinuation()
        {
            var c = Interlocked.Exchange(ref continuation, null);
            if (c != null)
            {
                if (c is Action)
                {
                    ((Action) c).Invoke();
                }
                else
                {
                    var l = (List<Action>) c;
                    var cnt = l.Count;
                    for (var i = 0; i < cnt; i++) l[i].Invoke();
                }
            }
        }

        public bool TrySetCanceled(OperationCanceledException exception)
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }

            return false;
        }
    }

    public class UniTaskCompletionSource<T> : IAwaiter<T>, IPromise<T>
    {
        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;
        private object continuation; // action or list
        private ExceptionHolder exception;
        private bool handled;

        private int state;
        private T value;

        public UniTask<T> Task => new UniTask<T>(this);
        public UniTask UnitTask => new UniTask(this);

        public UniTaskCompletionSource()
        {
            TaskTracker.TrackActiveTask(this, 2);
        }

        bool IAwaiter.IsCompleted => state != Pending;

        AwaiterStatus IAwaiter.Status => (AwaiterStatus) state;

        T IAwaiter<T>.GetResult()
        {
            MarkHandled();

            if (state == Succeeded) return value;

            if (state == Faulted)
            {
                exception.GetException().Throw();
            }
            else if (state == Canceled)
            {
                if (exception != null) exception.GetException().Throw(); // guranteed operation canceled exception.

                throw new OperationCanceledException();
            }
            else // Pending
            {
                throw new NotSupportedException("UniTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }

            return default;
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            if (Interlocked.CompareExchange(ref continuation, action, null) == null)
            {
                if (state != Pending) TryInvokeContinuation();
            }
            else
            {
                var c = continuation;
                if (c is Action)
                {
                    var list = new List<Action>();
                    list.Add((Action) c);
                    list.Add(action);
                    if (Interlocked.CompareExchange(ref continuation, list, c) == c) goto TRYINVOKE;
                }

                var l = (List<Action>) continuation;
                lock (l)
                {
                    l.Add(action);
                }

                TRYINVOKE:
                if (state != Pending) TryInvokeContinuation();
            }
        }

        void IAwaiter.GetResult()
        {
            ((IAwaiter<T>) this).GetResult();
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(continuation);
        }

        public bool TrySetResult(T value)
        {
            if (Interlocked.CompareExchange(ref state, Succeeded, Pending) == Pending)
            {
                this.value = value;
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public bool TrySetException(Exception exception)
        {
            if (Interlocked.CompareExchange(ref state, Faulted, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public bool TrySetCanceled()
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        [Conditional("UNITY_EDITOR")]
        internal void MarkHandled()
        {
            if (!handled)
            {
                handled = true;
                TaskTracker.RemoveTracking(this);
            }
        }

        private void TryInvokeContinuation()
        {
            var c = Interlocked.Exchange(ref continuation, null);
            if (c != null)
            {
                if (c is Action)
                {
                    ((Action) c).Invoke();
                }
                else
                {
                    var l = (List<Action>) c;
                    var cnt = l.Count;
                    for (var i = 0; i < cnt; i++) l[i].Invoke();
                }
            }
        }

        public bool TrySetCanceled(OperationCanceledException exception)
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }

            return false;
        }
    }
}

#endif
