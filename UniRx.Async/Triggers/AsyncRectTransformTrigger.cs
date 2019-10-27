using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncRectTransformTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<AsyncUnit> onRectTransformDimensionsChange;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onRectTransformDimensionsChanges;
        private AsyncTriggerPromise<AsyncUnit> onRectTransformRemoved;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onRectTransformRemoveds;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onRectTransformDimensionsChange, onRectTransformDimensionsChanges, onRectTransformRemoved, onRectTransformRemoveds);
        }


        private void OnRectTransformDimensionsChange()
        {
            TrySetResult(onRectTransformDimensionsChange, onRectTransformDimensionsChanges, AsyncUnit.Default);
        }


        public UniTask OnRectTransformDimensionsChangeAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onRectTransformDimensionsChange, ref onRectTransformDimensionsChanges, cancellationToken);
        }


        private void OnRectTransformRemoved()
        {
            TrySetResult(onRectTransformRemoved, onRectTransformRemoveds, AsyncUnit.Default);
        }


        public UniTask OnRectTransformRemovedAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onRectTransformRemoved, ref onRectTransformRemoveds, cancellationToken);
        }
    }
}

#endif
