using System.Threading;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncTriggerTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<Collider> onTriggerEnter;
        private AsyncTriggerPromiseDictionary<Collider> onTriggerEnters;
        private AsyncTriggerPromise<Collider> onTriggerExit;
        private AsyncTriggerPromiseDictionary<Collider> onTriggerExits;
        private AsyncTriggerPromise<Collider> onTriggerStay;
        private AsyncTriggerPromiseDictionary<Collider> onTriggerStays;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onTriggerEnter, onTriggerEnters, onTriggerExit, onTriggerExits, onTriggerStay, onTriggerStays);
        }


        private void OnTriggerEnter(Collider other)
        {
            TrySetResult(onTriggerEnter, onTriggerEnters, other);
        }


        public UniTask<Collider> OnTriggerEnterAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTriggerEnter, ref onTriggerEnters, cancellationToken);
        }


        private void OnTriggerExit(Collider other)
        {
            TrySetResult(onTriggerExit, onTriggerExits, other);
        }


        public UniTask<Collider> OnTriggerExitAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTriggerExit, ref onTriggerExits, cancellationToken);
        }


        private void OnTriggerStay(Collider other)
        {
            TrySetResult(onTriggerStay, onTriggerStays, other);
        }


        public UniTask<Collider> OnTriggerStayAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onTriggerStay, ref onTriggerStays, cancellationToken);
        }
    }
}

#endif
