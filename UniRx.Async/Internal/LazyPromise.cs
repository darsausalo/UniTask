#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Threading;

namespace UniRx.Async.Internal
{
    internal sealed class LazyPromise : IAwaiter
    {
        private Func<UniTask> factory;
        private UniTask value;

        public LazyPromise(Func<UniTask> factory)
        {
            this.factory = factory;
        }

        public bool IsCompleted
        {
            get
            {
                Create();
                return value.IsCompleted;
            }
        }

        public AwaiterStatus Status
        {
            get
            {
                Create();
                return value.Status;
            }
        }

        void IAwaiter.GetResult()
        {
            GetResult();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            Create();
            value.GetAwaiter().UnsafeOnCompleted(continuation);
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        private void Create()
        {
            var f = Interlocked.Exchange(ref factory, null);
            if (f != null) value = f();
        }

        public void GetResult()
        {
            Create();
            value.GetResult();
        }
    }

    internal sealed class LazyPromise<T> : IAwaiter<T>
    {
        private Func<UniTask<T>> factory;
        private UniTask<T> value;

        public LazyPromise(Func<UniTask<T>> factory)
        {
            this.factory = factory;
        }

        public bool IsCompleted
        {
            get
            {
                Create();
                return value.IsCompleted;
            }
        }

        public AwaiterStatus Status
        {
            get
            {
                Create();
                return value.Status;
            }
        }

        public T GetResult()
        {
            Create();
            return value.Result;
        }

        void IAwaiter.GetResult()
        {
            GetResult();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            Create();
            value.GetAwaiter().UnsafeOnCompleted(continuation);
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        private void Create()
        {
            var f = Interlocked.Exchange(ref factory, null);
            if (f != null) value = f();
        }
    }
}

#endif
