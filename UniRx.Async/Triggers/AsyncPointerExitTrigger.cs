using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncPointerExitTrigger : AsyncTriggerBase, IPointerExitHandler
    {
        private AsyncTriggerPromise<PointerEventData> onPointerExit;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerExits;


        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            TrySetResult(onPointerExit, onPointerExits, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onPointerExit, onPointerExits);
        }


        public UniTask<PointerEventData> OnPointerExitAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerExit, ref onPointerExits, cancellationToken);
        }
    }
}

#endif
