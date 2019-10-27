using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncPointerUpTrigger : AsyncTriggerBase, IPointerUpHandler
    {
        private AsyncTriggerPromise<PointerEventData> onPointerUp;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerUps;


        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            TrySetResult(onPointerUp, onPointerUps, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onPointerUp, onPointerUps);
        }


        public UniTask<PointerEventData> OnPointerUpAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerUp, ref onPointerUps, cancellationToken);
        }
    }
}

#endif
