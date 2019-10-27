using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncDeselectTrigger : AsyncTriggerBase, IDeselectHandler
    {
        private AsyncTriggerPromise<BaseEventData> onDeselect;
        private AsyncTriggerPromiseDictionary<BaseEventData> onDeselects;


        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            TrySetResult(onDeselect, onDeselects, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onDeselect, onDeselects);
        }


        public UniTask<BaseEventData> OnDeselectAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onDeselect, ref onDeselects, cancellationToken);
        }
    }
}

#endif
