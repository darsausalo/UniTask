﻿using System;
using System.Threading.Tasks;
using UniRx.Async.Internal;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.CompilerServices;
using System.Threading;

namespace UniRx.Async
{
    public partial struct UniTask
    {
        /// <summary>
        ///     If running on mainthread, do nothing. Otherwise, same as UniTask.Yield(PlayerLoopTiming.Update).
        /// </summary>
        public static SwitchToMainThreadAwaitable SwitchToMainThread()
        {
            return new SwitchToMainThreadAwaitable();
        }

        public static SwitchToThreadPoolAwaitable SwitchToThreadPool()
        {
            return new SwitchToThreadPoolAwaitable();
        }

        public static SwitchToTaskPoolAwaitable SwitchToTaskPool()
        {
            return new SwitchToTaskPoolAwaitable();
        }

        public static SwitchToSynchronizationContextAwaitable SwitchToSynchronizationContext(SynchronizationContext syncContext)
        {
            Error.ThrowArgumentNullException(syncContext, nameof(syncContext));
            return new SwitchToSynchronizationContextAwaitable(syncContext);
        }
    }

    public struct SwitchToMainThreadAwaitable
    {
        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted
            {
                get
                {
                    var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                    if (PlayerLoopHelper.MainThreadId == currentThreadId)
                        return true; // run immediate.
                    return false; // register continuation.
                }
            }

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                PlayerLoopHelper.AddContinuation(PlayerLoopTiming.Update, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                PlayerLoopHelper.AddContinuation(PlayerLoopTiming.Update, continuation);
            }
        }
    }

    public struct SwitchToThreadPoolAwaitable
    {
        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private static readonly WaitCallback switchToCallback = Callback;

            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                ThreadPool.UnsafeQueueUserWorkItem(switchToCallback, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                ThreadPool.UnsafeQueueUserWorkItem(switchToCallback, continuation);
            }

            private static void Callback(object state)
            {
                var continuation = (Action) state;
                continuation();
            }
        }
    }

    public struct SwitchToTaskPoolAwaitable
    {
        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private static readonly Action<object> switchToCallback = Callback;

            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                Task.Factory.StartNew(switchToCallback, continuation, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Task.Factory.StartNew(switchToCallback, continuation, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            private static void Callback(object state)
            {
                var continuation = (Action) state;
                continuation();
            }
        }
    }

    public struct SwitchToSynchronizationContextAwaitable
    {
        private readonly SynchronizationContext synchronizationContext;

        public SwitchToSynchronizationContextAwaitable(SynchronizationContext synchronizationContext)
        {
            this.synchronizationContext = synchronizationContext;
        }

        public Awaiter GetAwaiter()
        {
            return new Awaiter(synchronizationContext);
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private static readonly SendOrPostCallback switchToCallback = Callback;
            private readonly SynchronizationContext synchronizationContext;

            public Awaiter(SynchronizationContext synchronizationContext)
            {
                this.synchronizationContext = synchronizationContext;
            }

            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                synchronizationContext.Post(switchToCallback, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                synchronizationContext.Post(switchToCallback, continuation);
            }

            private static void Callback(object state)
            {
                var continuation = (Action) state;
                continuation();
            }
        }
    }
}

#endif
