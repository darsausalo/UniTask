using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncSubmitTrigger : AsyncTriggerBase, ISubmitHandler
    {
        private AsyncTriggerPromise<BaseEventData> onSubmit;
        private AsyncTriggerPromiseDictionary<BaseEventData> onSubmits;


        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            TrySetResult(onSubmit, onSubmits, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onSubmit, onSubmits);
        }


        public UniTask<BaseEventData> OnSubmitAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onSubmit, ref onSubmits, cancellationToken);
        }
    }
}

#endif
