using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncBeginDragTrigger : AsyncTriggerBase, IBeginDragHandler
    {
        private AsyncTriggerPromise<PointerEventData> onBeginDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onBeginDrags;


        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            TrySetResult(onBeginDrag, onBeginDrags, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onBeginDrag, onBeginDrags);
        }


        public UniTask<PointerEventData> OnBeginDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onBeginDrag, ref onBeginDrags, cancellationToken);
        }
    }
}

#endif
