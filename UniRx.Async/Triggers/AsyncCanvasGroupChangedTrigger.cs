using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncCanvasGroupChangedTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<AsyncUnit> onCanvasGroupChanged;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onCanvasGroupChangeds;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onCanvasGroupChanged, onCanvasGroupChangeds);
        }


        private void OnCanvasGroupChanged()
        {
            TrySetResult(onCanvasGroupChanged, onCanvasGroupChangeds, AsyncUnit.Default);
        }


        public UniTask OnCanvasGroupChangedAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onCanvasGroupChanged, ref onCanvasGroupChangeds, cancellationToken);
        }
    }
}

#endif
