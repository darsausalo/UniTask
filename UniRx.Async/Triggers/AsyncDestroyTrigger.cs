using UniRx.Async.Internal;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Threading;
using UnityEngine;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncDestroyTrigger : MonoBehaviour
    {
        private bool called;
        private CancellationTokenSource cancellationTokenSource; // main cancellation
        private object canellationTokenSourceOrQueue; // external from AddCancellationTriggerOnDestory
        private UniTaskCompletionSource promise;

        public CancellationToken CancellationToken
        {
            get
            {
                if (cancellationTokenSource == null) cancellationTokenSource = new CancellationTokenSource();
                return cancellationTokenSource.Token;
            }
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        private void OnDestroy()
        {
            called = true;
            promise?.TrySetResult();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            if (canellationTokenSourceOrQueue != null)
            {
                if (canellationTokenSourceOrQueue is CancellationTokenSource cts)
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                else
                {
                    var q = (MinimumQueue<CancellationTokenSource>) canellationTokenSourceOrQueue;
                    while (q.Count != 0)
                    {
                        var c = q.Dequeue();
                        c.Cancel();
                        c.Dispose();
                    }
                }

                canellationTokenSourceOrQueue = null;
            }
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public UniTask OnDestroyAsync()
        {
            if (called) return UniTask.CompletedTask;
            return new UniTask(promise ?? (promise = new UniTaskCompletionSource()));
        }

        /// <summary>Add Cancellation Triggers on destroy</summary>
        public void AddCancellationTriggerOnDestory(CancellationTokenSource cts)
        {
            if (called)
            {
                cts.Cancel();
                cts.Dispose();
            }

            if (canellationTokenSourceOrQueue == null)
            {
                canellationTokenSourceOrQueue = cts;
            }
            else if (canellationTokenSourceOrQueue is CancellationTokenSource c)
            {
                var q = new MinimumQueue<CancellationTokenSource>(4);
                q.Enqueue(c);
                q.Enqueue(cts);
                canellationTokenSourceOrQueue = q;
            }
            else
            {
                ((MinimumQueue<CancellationTokenSource>) canellationTokenSourceOrQueue).Enqueue(cts);
            }
        }
    }
}

#endif
