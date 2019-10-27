using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncAnimatorTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<int> onAnimatorIK;
        private AsyncTriggerPromiseDictionary<int> onAnimatorIKs;
        private AsyncTriggerPromise<AsyncUnit> onAnimatorMove;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onAnimatorMoves;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onAnimatorIK, onAnimatorIKs, onAnimatorMove, onAnimatorMoves);
        }


        private void OnAnimatorIK(int layerIndex)
        {
            TrySetResult(onAnimatorIK, onAnimatorIKs, layerIndex);
        }


        public UniTask<int> OnAnimatorIKAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onAnimatorIK, ref onAnimatorIKs, cancellationToken);
        }


        private void OnAnimatorMove()
        {
            TrySetResult(onAnimatorMove, onAnimatorMoves, AsyncUnit.Default);
        }


        public UniTask OnAnimatorMoveAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onAnimatorMove, ref onAnimatorMoves, cancellationToken);
        }
    }
}

#endif
