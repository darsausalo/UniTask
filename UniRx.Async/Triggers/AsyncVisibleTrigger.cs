using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncVisibleTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<AsyncUnit> onBecameInvisible;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onBecameInvisibles;
        private AsyncTriggerPromise<AsyncUnit> onBecameVisible;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onBecameVisibles;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onBecameInvisible, onBecameInvisibles, onBecameVisible, onBecameVisibles);
        }


        private void OnBecameInvisible()
        {
            TrySetResult(onBecameInvisible, onBecameInvisibles, AsyncUnit.Default);
        }


        public UniTask OnBecameInvisibleAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onBecameInvisible, ref onBecameInvisibles, cancellationToken);
        }


        private void OnBecameVisible()
        {
            TrySetResult(onBecameVisible, onBecameVisibles, AsyncUnit.Default);
        }


        public UniTask OnBecameVisibleAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onBecameVisible, ref onBecameVisibles, cancellationToken);
        }
    }
}

#endif
