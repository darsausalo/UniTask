using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncInitializePotentialDragTrigger : AsyncTriggerBase, IInitializePotentialDragHandler
    {
        private AsyncTriggerPromise<PointerEventData> onInitializePotentialDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onInitializePotentialDrags;


        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            TrySetResult(onInitializePotentialDrag, onInitializePotentialDrags, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onInitializePotentialDrag, onInitializePotentialDrags);
        }


        public UniTask<PointerEventData> OnInitializePotentialDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onInitializePotentialDrag, ref onInitializePotentialDrags, cancellationToken);
        }
    }
}

#endif
