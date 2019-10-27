using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncDragTrigger : AsyncTriggerBase, IDragHandler
    {
        private AsyncTriggerPromise<PointerEventData> onDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onDrags;


        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            TrySetResult(onDrag, onDrags, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onDrag, onDrags);
        }


        public UniTask<PointerEventData> OnDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onDrag, ref onDrags, cancellationToken);
        }
    }
}

#endif