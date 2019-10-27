using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncFixedUpdateTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<AsyncUnit> fixedUpdate;
        private AsyncTriggerPromiseDictionary<AsyncUnit> fixedUpdates;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(fixedUpdate, fixedUpdates);
        }


        private void FixedUpdate()
        {
            TrySetResult(fixedUpdate, fixedUpdates, AsyncUnit.Default);
        }


        public UniTask FixedUpdateAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref fixedUpdate, ref fixedUpdates, cancellationToken);
        }
    }
}

#endif
