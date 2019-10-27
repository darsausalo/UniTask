using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncMoveTrigger : AsyncTriggerBase, IMoveHandler
    {
        private AsyncTriggerPromise<AxisEventData> onMove;
        private AsyncTriggerPromiseDictionary<AxisEventData> onMoves;


        void IMoveHandler.OnMove(AxisEventData eventData)
        {
            TrySetResult(onMove, onMoves, eventData);
        }


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onMove, onMoves);
        }


        public UniTask<AxisEventData> OnMoveAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMove, ref onMoves, cancellationToken);
        }
    }
}

#endif
