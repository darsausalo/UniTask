using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncPointerClickTrigger : AsyncTriggerBase, IPointerClickHandler
    {
        private AsyncTriggerPromise<PointerEventData> onPointerClick;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerClicks;


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            TrySetResult(onPointerClick, onPointerClicks, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onPointerClick, onPointerClicks);
        }


        public UniTask<PointerEventData> OnPointerClickAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerClick, ref onPointerClicks, cancellationToken);
        }
    }
}

#endif
