using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncSelectTrigger : AsyncTriggerBase, ISelectHandler
    {
        private AsyncTriggerPromise<BaseEventData> onSelect;
        private AsyncTriggerPromiseDictionary<BaseEventData> onSelects;


        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            TrySetResult(onSelect, onSelects, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onSelect, onSelects);
        }


        public UniTask<BaseEventData> OnSelectAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onSelect, ref onSelects, cancellationToken);
        }
    }
}

#endif
