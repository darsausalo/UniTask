using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncEndDragTrigger : AsyncTriggerBase, IEndDragHandler
    {
        private AsyncTriggerPromise<PointerEventData> onEndDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onEndDrags;


        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            TrySetResult(onEndDrag, onEndDrags, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onEndDrag, onEndDrags);
        }


        public UniTask<PointerEventData> OnEndDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onEndDrag, ref onEndDrags, cancellationToken);
        }
    }
}

#endif
