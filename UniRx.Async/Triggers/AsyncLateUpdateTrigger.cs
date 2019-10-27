using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncLateUpdateTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<AsyncUnit> lateUpdate;
        private AsyncTriggerPromiseDictionary<AsyncUnit> lateUpdates;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(lateUpdate, lateUpdates);
        }


        private void LateUpdate()
        {
            TrySetResult(lateUpdate, lateUpdates, AsyncUnit.Default);
        }


        public UniTask LateUpdateAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref lateUpdate, ref lateUpdates, cancellationToken);
        }
    }
}

#endif
