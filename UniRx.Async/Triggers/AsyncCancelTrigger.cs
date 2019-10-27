using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncCancelTrigger : AsyncTriggerBase, ICancelHandler
    {
        private AsyncTriggerPromise<BaseEventData> onCancel;
        private AsyncTriggerPromiseDictionary<BaseEventData> onCancels;


        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
            TrySetResult(onCancel, onCancels, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onCancel, onCancels);
        }


        public UniTask<BaseEventData> OnCancelAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onCancel, ref onCancels, cancellationToken);
        }
    }
}

#endif
