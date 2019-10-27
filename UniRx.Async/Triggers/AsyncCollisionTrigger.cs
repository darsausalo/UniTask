using System.Threading;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncCollisionTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<Collision> onCollisionEnter;
        private AsyncTriggerPromiseDictionary<Collision> onCollisionEnters;
        private AsyncTriggerPromise<Collision> onCollisionExit;
        private AsyncTriggerPromiseDictionary<Collision> onCollisionExits;
        private AsyncTriggerPromise<Collision> onCollisionStay;
        private AsyncTriggerPromiseDictionary<Collision> onCollisionStays;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onCollisionEnter, onCollisionEnters, onCollisionExit, onCollisionExits, onCollisionStay, onCollisionStays);
        }


        private void OnCollisionEnter(Collision collision)
        {
            TrySetResult(onCollisionEnter, onCollisionEnters, collision);
        }


        public UniTask<Collision> OnCollisionEnterAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onCollisionEnter, ref onCollisionEnters, cancellationToken);
        }


        private void OnCollisionExit(Collision collisionInfo)
        {
            TrySetResult(onCollisionExit, onCollisionExits, collisionInfo);
        }


        public UniTask<Collision> OnCollisionExitAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onCollisionExit, ref onCollisionExits, cancellationToken);
        }


        private void OnCollisionStay(Collision collisionInfo)
        {
            TrySetResult(onCollisionStay, onCollisionStays, collisionInfo);
        }


        public UniTask<Collision> OnCollisionStayAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onCollisionStay, ref onCollisionStays, cancellationToken);
        }
    }
}

#endif
