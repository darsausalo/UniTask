using System.Runtime.CompilerServices;
using System.Threading;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Runtime.ExceptionServices;

namespace UniRx.Async
{
    public partial struct UniTask
    {
        // UniTask

        public static async UniTask<(bool hasResultLeft, T0 result)> WhenAny<T0>(UniTask<T0> task0, UniTask task1)
        {
            return await new UnitWhenAnyPromise<T0>(task0, task1);
        }

        public static async UniTask<(int winArgumentIndex, T result)> WhenAny<T>(params UniTask<T>[] tasks)
        {
            return await new WhenAnyPromise<T>(tasks);
        }

        /// <summary>Return value is winArgumentIndex</summary>
        public static async UniTask<int> WhenAny(params UniTask[] tasks)
        {
            return await new WhenAnyPromise(tasks);
        }

        private class UnitWhenAnyPromise<T0>
        {
            private int completeCount;
            private ExceptionDispatchInfo exception;
            private T0 result0;
            private Action whenComplete;
            private int winArgumentIndex;

            private bool IsCompleted => exception != null || Volatile.Read(ref winArgumentIndex) != -1;

            public UnitWhenAnyPromise(UniTask<T0> task0, UniTask task1)
            {
                whenComplete = null;
                exception = null;
                completeCount = 0;
                winArgumentIndex = -1;
                result0 = default;

                RunTask0(task0).Forget();
                RunTask1(task1).Forget();
            }

            private void TryCallContinuation()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                if (action != null) action.Invoke();
            }

            private async UniTaskVoid RunTask0(UniTask<T0> task)
            {
                T0 value;
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

                var count = Interlocked.Increment(ref completeCount);
                if (count == 1)
                {
                    result0 = value;
                    Volatile.Write(ref winArgumentIndex, 0);
                    TryCallContinuation();
                }
            }

            private async UniTaskVoid RunTask1(UniTask task)
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
                if (count == 1)
                {
                    Volatile.Write(ref winArgumentIndex, 1);
                    TryCallContinuation();
                }
            }


            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }

            public struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly UnitWhenAnyPromise<T0> parent;

                public Awaiter(UnitWhenAnyPromise<T0> parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.IsCompleted;

                public (bool, T0) GetResult()
                {
                    if (parent.exception != null) parent.exception.Throw();

                    return (parent.winArgumentIndex == 0, parent.result0);
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


        private class WhenAnyPromise<T>
        {
            private int completeCount;
            private ExceptionDispatchInfo exception;
            private T result;
            private Action whenComplete;
            private int winArgumentIndex;

            public bool IsComplete => exception != null || Volatile.Read(ref winArgumentIndex) != -1;

            public WhenAnyPromise(UniTask<T>[] tasks)
            {
                completeCount = 0;
                winArgumentIndex = -1;
                whenComplete = null;
                exception = null;
                result = default;

                for (var i = 0; i < tasks.Length; i++) RunTask(tasks[i], i).Forget();
            }

            private async UniTaskVoid RunTask(UniTask<T> task, int index)
            {
                T value;
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

                var count = Interlocked.Increment(ref completeCount);
                if (count == 1)
                {
                    result = value;
                    Volatile.Write(ref winArgumentIndex, index);
                    TryCallContinuation();
                }
            }

            private void TryCallContinuation()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                if (action != null) action.Invoke();
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }

            public struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly WhenAnyPromise<T> parent;

                public Awaiter(WhenAnyPromise<T> parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.IsComplete;

                public (int, T) GetResult()
                {
                    if (parent.exception != null) parent.exception.Throw();

                    return (parent.winArgumentIndex, parent.result);
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

        private class WhenAnyPromise
        {
            private int completeCount;
            private ExceptionDispatchInfo exception;
            private Action whenComplete;
            private int winArgumentIndex;

            public bool IsComplete => exception != null || Volatile.Read(ref winArgumentIndex) != -1;

            public WhenAnyPromise(UniTask[] tasks)
            {
                completeCount = 0;
                winArgumentIndex = -1;
                whenComplete = null;
                exception = null;

                for (var i = 0; i < tasks.Length; i++) RunTask(tasks[i], i).Forget();
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
                if (count == 1)
                {
                    Volatile.Write(ref winArgumentIndex, index);
                    TryCallContinuation();
                }
            }

            private void TryCallContinuation()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                if (action != null) action.Invoke();
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }

            public struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly WhenAnyPromise parent;

                public Awaiter(WhenAnyPromise parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.IsComplete;

                public int GetResult()
                {
                    if (parent.exception != null) parent.exception.Throw();

                    return parent.winArgumentIndex;
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
