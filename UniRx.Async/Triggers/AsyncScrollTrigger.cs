using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncScrollTrigger : AsyncTriggerBase, IScrollHandler
    {
        private AsyncTriggerPromise<PointerEventData> onScroll;
        private AsyncTriggerPromiseDictionary<PointerEventData> onScrolls;


        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            TrySetResult(onScroll, onScrolls, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onScroll, onScrolls);
        }


        public UniTask<PointerEventData> OnScrollAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onScroll, ref onScrolls, cancellationToken);
        }
    }
}

#endif
