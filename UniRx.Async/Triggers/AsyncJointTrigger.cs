using System.Threading;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncJointTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<float> onJointBreak;
        private AsyncTriggerPromise<Joint2D> onJointBreak2D;
        private AsyncTriggerPromiseDictionary<Joint2D> onJointBreak2Ds;
        private AsyncTriggerPromiseDictionary<float> onJointBreaks;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onJointBreak, onJointBreaks, onJointBreak2D, onJointBreak2Ds);
        }


        private void OnJointBreak(float breakForce)
        {
            TrySetResult(onJointBreak, onJointBreaks, breakForce);
        }


        public UniTask<float> OnJointBreakAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onJointBreak, ref onJointBreaks, cancellationToken);
        }


        private void OnJointBreak2D(Joint2D brokenJoint)
        {
            TrySetResult(onJointBreak2D, onJointBreak2Ds, brokenJoint);
        }


        public UniTask<Joint2D> OnJointBreak2DAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onJointBreak2D, ref onJointBreak2Ds, cancellationToken);
        }
    }
}

#endif
