using System.Threading;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncTrigger2DTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<Collider2D> onTriggerEnter2D;
        private AsyncTriggerPromiseDictionary<Collider2D> onTriggerEnter2Ds;
        private AsyncTriggerPromise<Collider2D> onTriggerExit2D;
        private AsyncTriggerPromiseDictionary<Collider2D> onTriggerExit2Ds;
        private AsyncTriggerPromise<Collider2D> onTriggerStay2D;
        private AsyncTriggerPromiseDictionary<Collider2D> onTriggerStay2Ds;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onTriggerEnter2D, onTriggerEnter2Ds, onTriggerExit2D, onTriggerExit2Ds, onTriggerStay2D, onTriggerStay2Ds);
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            TrySetResult(onTriggerEnter2D, onTriggerEnter2Ds, other);
        }


        public UniTask<Collider2D> OnTriggerEnter2DAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTriggerEnter2D, ref onTriggerEnter2Ds, cancellationToken);
        }


        private void OnTriggerExit2D(Collider2D other)
        {
            TrySetResult(onTriggerExit2D, onTriggerExit2Ds, other);
        }


        public UniTask<Collider2D> OnTriggerExit2DAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTriggerExit2D, ref onTriggerExit2Ds, cancellationToken);
        }


        private void OnTriggerStay2D(Collider2D other)
        {
            TrySetResult(onTriggerStay2D, onTriggerStay2Ds, other);
        }


        public UniTask<Collider2D> OnTriggerStay2DAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTriggerStay2D, ref onTriggerStay2Ds, cancellationToken);
        }
    }
}

#endif
