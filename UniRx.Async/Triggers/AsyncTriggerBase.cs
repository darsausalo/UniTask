﻿using System;
using System.Collections.Generic;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Threading;
using UniRx.Async.Internal;

namespace UniRx.Async.Triggers
{
    public interface ICancelablePromise
    {
        CancellationToken RegisteredCancellationToken { get; }
        bool TrySetCanceled();
    }

    public class AsyncTriggerPromise<T> : ReusablePromise<T>, IPromise<T>, ICancelablePromise
    {
        public AsyncTriggerPromise()
            : this(CancellationToken.None)
        {
        }

        public AsyncTriggerPromise(CancellationToken cancellationToken)
        {
            RegisteredCancellationToken = cancellationToken;
            TaskTracker.TrackActiveTask(this);
        }

        public CancellationToken RegisteredCancellationToken { get; }

        public override bool TrySetResult(T result)
        {
            if (Status == AwaiterStatus.Pending)
            {
                // keep status as Pending.
                ForceSetResult(result);
                TryInvokeContinuation();
                return true;
            }

            return false;
        }

        public override bool TrySetCanceled()
        {
            if (Status == AwaiterStatus.Canceled) return false;
            TaskTracker.RemoveTracking(this);
            return base.TrySetCanceled();
        }

        public override T GetResult()
        {
            if (Status == AwaiterStatus.Pending) return RawResult;
            return base.GetResult();
        }
    }

    public interface ICancellationTokenKeyDictionary
    {
        void Remove(CancellationToken token);
    }

    public class AsyncTriggerPromiseDictionary<TPromiseType> :
        Dictionary<CancellationToken, AsyncTriggerPromise<TPromiseType>>,
        ICancellationTokenKeyDictionary,
        IEnumerable<ICancelablePromise>
    {
        public AsyncTriggerPromiseDictionary()
            : base(CancellationTokenEqualityComparer.Default)
        {
        }

        void ICancellationTokenKeyDictionary.Remove(CancellationToken token)
        {
            Remove(token);
        }

        IEnumerator<ICancelablePromise> IEnumerable<ICancelablePromise>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }

    public abstract class AsyncTriggerBase : MonoBehaviour
    {
        private static readonly Action<object> Callback = CancelCallback;

        private bool calledAwake;
        private bool destroyCalled;
        private CancellationTokenRegistration[] registeredCancellations;
        private int registeredCancellationsCount;

        protected abstract IEnumerable<ICancelablePromise> GetPromises();

        protected static IEnumerable<ICancelablePromise> Concat(ICancelablePromise p1, IEnumerable<ICancelablePromise> p1s)
        {
            if (p1 != null) yield return p1;
            if (p1s != null)
                foreach (var item in p1s)
                    yield return item;
        }

        protected static IEnumerable<ICancelablePromise> Concat(
            ICancelablePromise p1, IEnumerable<ICancelablePromise> p1s,
            ICancelablePromise p2, IEnumerable<ICancelablePromise> p2s)
        {
            if (p1 != null) yield return p1;
            if (p1s != null)
                foreach (var item in p1s)
                    yield return item;
            if (p2 != null) yield return p2;
            if (p2s != null)
                foreach (var item in p2s)
                    yield return item;
        }

        protected static IEnumerable<ICancelablePromise> Concat(
            ICancelablePromise p1, IEnumerable<ICancelablePromise> p1s,
            ICancelablePromise p2, IEnumerable<ICancelablePromise> p2s,
            ICancelablePromise p3, IEnumerable<ICancelablePromise> p3s)
        {
            if (p1 != null) yield return p1;
            if (p1s != null)
                foreach (var item in p1s)
                    yield return item;
            if (p2 != null) yield return p2;
            if (p2s != null)
                foreach (var item in p2s)
                    yield return item;
            if (p3 != null) yield return p3;
            if (p3s != null)
                foreach (var item in p3s)
                    yield return item;
        }

        protected static IEnumerable<ICancelablePromise> Concat(
            ICancelablePromise p1, IEnumerable<ICancelablePromise> p1s,
            ICancelablePromise p2, IEnumerable<ICancelablePromise> p2s,
            ICancelablePromise p3, IEnumerable<ICancelablePromise> p3s,
            ICancelablePromise p4, IEnumerable<ICancelablePromise> p4s)
        {
            if (p1 != null) yield return p1;
            if (p1s != null)
                foreach (var item in p1s)
                    yield return item;
            if (p2 != null) yield return p2;
            if (p2s != null)
                foreach (var item in p2s)
                    yield return item;
            if (p3 != null) yield return p3;
            if (p3s != null)
                foreach (var item in p3s)
                    yield return item;
            if (p4 != null) yield return p4;
            if (p4s != null)
                foreach (var item in p4s)
                    yield return item;
        }

        // otherwise...
        protected static IEnumerable<ICancelablePromise> Concat(params object[] promises)
        {
            foreach (var item in promises)
                if (item is ICancelablePromise p)
                    yield return p;
                else if (item is IEnumerable<ICancelablePromise> ps)
                    foreach (var p2 in ps)
                        yield return p2;
        }

        protected UniTask<T> GetOrAddPromise<T>(ref AsyncTriggerPromise<T> promise, ref AsyncTriggerPromiseDictionary<T> promises, CancellationToken cancellationToken)
        {
            if (destroyCalled) return UniTask.FromCanceled<T>();

            if (!cancellationToken.CanBeCanceled)
            {
                if (promise == null)
                {
                    promise = new AsyncTriggerPromise<T>();
                    if (!calledAwake) PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, new AwakeMonitor(this));
                }

                return promise.Task;
            }

            if (promises == null) promises = new AsyncTriggerPromiseDictionary<T>();

            if (promises.TryGetValue(cancellationToken, out var cancellablePromise)) return cancellablePromise.Task;

            cancellablePromise = new AsyncTriggerPromise<T>();
            promises.Add(cancellationToken, cancellablePromise);
            if (!calledAwake) PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, new AwakeMonitor(this));

            var registrationToken =
                cancellationToken.RegisterWithoutCaptureExecutionContext(Callback,
                    Tuple.Create((ICancellationTokenKeyDictionary) promises, (ICancelablePromise) cancellablePromise));
            if (registeredCancellations == null) registeredCancellations = ArrayPool<CancellationTokenRegistration>.Shared.Rent(4);
            ArrayPoolUtil.EnsureCapacity(ref registeredCancellations, registeredCancellationsCount + 1, ArrayPool<CancellationTokenRegistration>.Shared);
            registeredCancellations[registeredCancellationsCount++] = registrationToken;

            return cancellablePromise.Task;
        }

        private static void CancelCallback(object state)
        {
            var tuple = (Tuple<ICancellationTokenKeyDictionary, ICancelablePromise>) state;
            var dict = tuple.Item1;
            var promise = tuple.Item2;

            promise.TrySetCanceled();
            dict.Remove(promise.RegisteredCancellationToken);
        }

        protected void TrySetResult<T>(ReusablePromise<T> promise, AsyncTriggerPromiseDictionary<T> promises, T value)
        {
            if (promise != null) promise.TrySetResult(value);
            if (promises != null) PromiseHelper.TrySetResultAll(promises.Values, value);
        }

        private void Awake()
        {
            calledAwake = true;
        }

        private void OnDestroy()
        {
            if (destroyCalled) return;
            destroyCalled = true;
            foreach (var item in GetPromises()) item.TrySetCanceled();
            if (registeredCancellations != null)
            {
                for (var i = 0; i < registeredCancellationsCount; i++)
                {
                    registeredCancellations[i].Dispose();
                    registeredCancellations[i] = default;
                }

                ArrayPool<CancellationTokenRegistration>.Shared.Return(registeredCancellations);
            }
        }

        private class AwakeMonitor : IPlayerLoopItem
        {
            private readonly AsyncTriggerBase trigger;

            public AwakeMonitor(AsyncTriggerBase trigger)
            {
                this.trigger = trigger;
            }

            public bool MoveNext()
            {
                if (trigger.calledAwake) return false;
                if (trigger == null)
                {
                    trigger.OnDestroy();
                    return false;
                }

                return true;
            }
        }
    }
}

#endif
