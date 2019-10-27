using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UniRx.Async.Internal;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace UniRx.Async
{
    public partial struct UniTask
    {
        // UniTask

        public static async UniTask<T[]> WhenAll<T>(params UniTask<T>[] tasks)
        {
            return await new WhenAllPromise<T>(tasks, tasks.Length);
        }

        public static async UniTask<T[]> WhenAll<T>(IEnumerable<UniTask<T>> tasks)
        {
            WhenAllPromise<T> promise;
            using (var span = ArrayPoolUtil.Materialize(tasks))
            {
                promise = new WhenAllPromise<T>(span.Array, span.Length);
            }

            return await promise;
        }

        public static async UniTask WhenAll(params UniTask[] tasks)
        {
            await new WhenAllPromise(tasks, tasks.Length);
        }

        public static async UniTask WhenAll(IEnumerable<UniTask> tasks)
        {
            WhenAllPromise promise;
            using (var span = ArrayPoolUtil.Materialize(tasks))
            {
                promise = new WhenAllPromise(span.Array, span.Length);
            }

            await promise;
        }

        private class WhenAllPromise<T>
        {
            private readonly T[] result;
            private int completeCount;
            private ExceptionDispatchInfo exception;
            private Action whenComplete;

            public WhenAllPromise(UniTask<T>[] tasks, int tasksLength)
            {
                completeCount = 0;
                whenComplete = null;
                exception = null;
                result = new T[tasksLength];

                for (var i = 0; i < tasksLength; i++)
                    if (tasks[i].IsCompleted)
                    {
                        T value = default;
                        try
                        {
                            value = tasks[i].Result;
                        }
                        catch (Exception ex)
                        {
                            exception = ExceptionDispatchInfo.Capture(ex);
                            TryCallContinuation();
                            continue;
                        }

                        result[i] = value;
                        var count = Interlocked.Increment(ref completeCount);
                        if (count == result.Length) TryCallContinuation();
                    }
                    else
                    {
                        RunTask(tasks[i], i).Forget();
                    }
            }

            private void TryCallContinuation()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                if (action != null) action.Invoke();
            }

            private async UniTaskVoid RunTask(UniTask<T> task, int index)
            {
                var value = default(T);
                try
                {
                    value = await task;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryCallContinuation();
                    return;
                }

                result[index] = value;
                var count = Interlocked.Increment(ref completeCount);
                if (count == result.Length) TryCallContinuation();
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }

            public struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly WhenAllPromise<T> parent;

                public Awaiter(WhenAllPromise<T> parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.exception != null || parent.result.Length == parent.completeCount;

                public T[] GetResult()
                {
                    if (parent.exception != null) parent.exception.Throw();

                    return parent.result;
                }

                public void OnCompleted(Action continuation)
                {
                    UnsafeOnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    parent.whenComplete = continuation;
                    if (IsCompleted)
                    {
                        var action = Interlocked.Exchange(ref parent.whenComplete, null);
                        if (action != null) action();
                    }
                }
            }
        }

        private class WhenAllPromise
        {
            private readonly int resultLength;
            private int completeCount;
            private ExceptionDispatchInfo exception;
            private Action whenComplete;

            public WhenAllPromise(UniTask[] tasks, int tasksLength)
            {
                completeCount = 0;
                whenComplete = null;
                exception = null;
                resultLength = tasksLength;

                for (var i = 0; i < tasksLength; i++)
                    if (tasks[i].IsCompleted)
                    {
                        try
                        {
                            tasks[i].GetResult();
                        }
                        catch (Exception ex)
                        {
                            exception = ExceptionDispatchInfo.Capture(ex);
                            TryCallContinuation();
                            continue;
                        }

                        var count = Interlocked.Increment(ref completeCount);
                        if (count == resultLength) TryCallContinuation();
                    }
                    else
                    {
                        RunTask(tasks[i], i).Forget();
                    }
            }

            private void TryCallContinuation()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                if (action != null) action.Invoke();
            }

            private async UniTaskVoid RunTask(UniTask task, int index)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryCallContinuation();
                    return;
                }

                var count = Interlocked.Increment(ref completeCount);
                if (count == resultLength) TryCallContinuation();
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }

            public struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly WhenAllPromise parent;

                public Awaiter(WhenAllPromise parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.exception != null || parent.resultLength == parent.completeCount;

                public void GetResult()
                {
                    if (parent.exception != null) parent.exception.Throw();
                }

                public void OnCompleted(Action continuation)
                {
                    UnsafeOnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    parent.whenComplete = continuation;
                    if (IsCompleted)
                    {
                        var action = Interlocked.Exchange(ref parent.whenComplete, null);
                        if (action != null) action();
                    }
                }
            }
        }
    }
}
#endif
