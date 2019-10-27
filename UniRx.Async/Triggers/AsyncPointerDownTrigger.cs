using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncPointerDownTrigger : AsyncTriggerBase, IPointerDownHandler
    {
        private AsyncTriggerPromise<PointerEventData> onPointerDown;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerDowns;


        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            TrySetResult(onPointerDown, onPointerDowns, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onPointerDown, onPointerDowns);
        }


        public UniTask<PointerEventData> OnPointerDownAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerDown, ref onPointerDowns, cancellationToken);
        }
    }
}

#endif
