using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncTransformChangedTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<AsyncUnit> onBeforeTransformParentChanged;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onBeforeTransformParentChangeds;
        private AsyncTriggerPromise<AsyncUnit> onTransformChildrenChanged;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onTransformChildrenChangeds;
        private AsyncTriggerPromise<AsyncUnit> onTransformParentChanged;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onTransformParentChangeds;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onBeforeTransformParentChanged, onBeforeTransformParentChangeds, onTransformParentChanged, onTransformParentChangeds, onTransformChildrenChanged,
                onTransformChildrenChangeds);
        }


        private void OnBeforeTransformParentChanged()
        {
            TrySetResult(onBeforeTransformParentChanged, onBeforeTransformParentChangeds, AsyncUnit.Default);
        }


        public UniTask OnBeforeTransformParentChangedAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onBeforeTransformParentChanged, ref onBeforeTransformParentChangeds, cancellationToken);
        }


        private void OnTransformParentChanged()
        {
            TrySetResult(onTransformParentChanged, onTransformParentChangeds, AsyncUnit.Default);
        }


        public UniTask OnTransformParentChangedAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTransformParentChanged, ref onTransformParentChangeds, cancellationToken);
        }


        private void OnTransformChildrenChanged()
        {
            TrySetResult(onTransformChildrenChanged, onTransformChildrenChangeds, AsyncUnit.Default);
        }


        public UniTask OnTransformChildrenChangedAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTransformChildrenChanged, ref onTransformChildrenChangeds, cancellationToken);
        }
    }
}

#endif
