using System.Threading;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncParticleTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<GameObject> onParticleCollision;
        private AsyncTriggerPromiseDictionary<GameObject> onParticleCollisions;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onParticleCollision, onParticleCollisions);
        }


        private void OnParticleCollision(GameObject other)
        {
            TrySetResult(onParticleCollision, onParticleCollisions, other);
        }


        public UniTask<GameObject> OnParticleCollisionAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onParticleCollision, ref onParticleCollisions, cancellationToken);
        }
    }
}

#endif
