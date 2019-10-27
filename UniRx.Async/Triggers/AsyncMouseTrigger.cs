using UnityEngine;
#if !(UNITY_IPHONE || UNITY_ANDROID || UNITY_METRO)
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Threading;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncMouseTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<AsyncUnit> onMouseDown;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onMouseDowns;
        private AsyncTriggerPromise<AsyncUnit> onMouseDrag;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onMouseDrags;
        private AsyncTriggerPromise<AsyncUnit> onMouseEnter;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onMouseEnters;
        private AsyncTriggerPromise<AsyncUnit> onMouseExit;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onMouseExits;
        private AsyncTriggerPromise<AsyncUnit> onMouseOver;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onMouseOvers;
        private AsyncTriggerPromise<AsyncUnit> onMouseUp;
        private AsyncTriggerPromise<AsyncUnit> onMouseUpAsButton;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onMouseUpAsButtons;
        private AsyncTriggerPromiseDictionary<AsyncUnit> onMouseUps;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onMouseDown, onMouseDowns, onMouseDrag, onMouseDrags, onMouseEnter, onMouseEnters, onMouseExit, onMouseExits, onMouseOver, onMouseOvers, onMouseUp,
                onMouseUps,
                onMouseUpAsButton, onMouseUpAsButtons);
        }


        private void OnMouseDown()
        {
            TrySetResult(onMouseDown, onMouseDowns, AsyncUnit.Default);
        }


        public UniTask OnMouseDownAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMouseDown, ref onMouseDowns, cancellationToken);
        }


        private void OnMouseDrag()
        {
            TrySetResult(onMouseDrag, onMouseDrags, AsyncUnit.Default);
        }


        public UniTask OnMouseDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMouseDrag, ref onMouseDrags, cancellationToken);
        }


        private void OnMouseEnter()
        {
            TrySetResult(onMouseEnter, onMouseEnters, AsyncUnit.Default);
        }


        public UniTask OnMouseEnterAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMouseEnter, ref onMouseEnters, cancellationToken);
        }


        private void OnMouseExit()
        {
            TrySetResult(onMouseExit, onMouseExits, AsyncUnit.Default);
        }


        public UniTask OnMouseExitAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMouseExit, ref onMouseExits, cancellationToken);
        }


        private void OnMouseOver()
        {
            TrySetResult(onMouseOver, onMouseOvers, AsyncUnit.Default);
        }


        public UniTask OnMouseOverAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMouseOver, ref onMouseOvers, cancellationToken);
        }


        private void OnMouseUp()
        {
            TrySetResult(onMouseUp, onMouseUps, AsyncUnit.Default);
        }


        public UniTask OnMouseUpAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMouseUp, ref onMouseUps, cancellationToken);
        }


        private void OnMouseUpAsButton()
        {
            TrySetResult(onMouseUpAsButton, onMouseUpAsButtons, AsyncUnit.Default);
        }


        public UniTask OnMouseUpAsButtonAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMouseUpAsButton, ref onMouseUpAsButtons, cancellationToken);
        }
    }
}

#endif


#endif
