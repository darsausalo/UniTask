using System.Threading;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UniRx.Async.Triggers
{
    [DisallowMultipleComponent]
    public class AsyncEventTrigger : AsyncTriggerBase
    {
        private AsyncTriggerPromise<PointerEventData> onBeginDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onBeginDrags;
        private AsyncTriggerPromise<BaseEventData> onCancel;
        private AsyncTriggerPromiseDictionary<BaseEventData> onCancels;
        private AsyncTriggerPromise<BaseEventData> onDeselect;
        private AsyncTriggerPromiseDictionary<BaseEventData> onDeselects;
        private AsyncTriggerPromise<PointerEventData> onDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onDrags;
        private AsyncTriggerPromise<PointerEventData> onDrop;
        private AsyncTriggerPromiseDictionary<PointerEventData> onDrops;
        private AsyncTriggerPromise<PointerEventData> onEndDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onEndDrags;
        private AsyncTriggerPromise<PointerEventData> onInitializePotentialDrag;
        private AsyncTriggerPromiseDictionary<PointerEventData> onInitializePotentialDrags;
        private AsyncTriggerPromise<AxisEventData> onMove;
        private AsyncTriggerPromiseDictionary<AxisEventData> onMoves;
        private AsyncTriggerPromise<PointerEventData> onPointerClick;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerClicks;
        private AsyncTriggerPromise<PointerEventData> onPointerDown;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerDowns;
        private AsyncTriggerPromise<PointerEventData> onPointerEnter;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerEnters;
        private AsyncTriggerPromise<PointerEventData> onPointerExit;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerExits;
        private AsyncTriggerPromise<PointerEventData> onPointerUp;
        private AsyncTriggerPromiseDictionary<PointerEventData> onPointerUps;
        private AsyncTriggerPromise<PointerEventData> onScroll;
        private AsyncTriggerPromiseDictionary<PointerEventData> onScrolls;
        private AsyncTriggerPromise<BaseEventData> onSelect;
        private AsyncTriggerPromiseDictionary<BaseEventData> onSelects;
        private AsyncTriggerPromise<BaseEventData> onSubmit;
        private AsyncTriggerPromiseDictionary<BaseEventData> onSubmits;
        private AsyncTriggerPromise<BaseEventData> onUpdateSelected;
        private AsyncTriggerPromiseDictionary<BaseEventData> onUpdateSelecteds;


        protected override IEnumerable<ICancelablePromise> GetPromises()
        {
            return Concat(onDeselect, onDeselects, onMove, onMoves, onPointerDown, onPointerDowns, onPointerEnter, onPointerEnters, onPointerExit, onPointerExits, onPointerUp,
                onPointerUps, onSelect, onSelects, onPointerClick, onPointerClicks, onSubmit, onSubmits, onDrag, onDrags, onBeginDrag, onBeginDrags, onEndDrag, onEndDrags, onDrop,
                onDrops,
                onUpdateSelected, onUpdateSelecteds, onInitializePotentialDrag, onInitializePotentialDrags, onCancel, onCancels, onScroll, onScrolls);
        }

        private void OnDeselect(BaseEventData eventData)
        {
            TrySetResult(onDeselect, onDeselects, eventData);
        }


        public UniTask<BaseEventData> OnDeselectAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onDeselect, ref onDeselects, cancellationToken);
        }


        private void OnMove(AxisEventData eventData)
        {
            TrySetResult(onMove, onMoves, eventData);
        }


        public UniTask<AxisEventData> OnMoveAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onMove, ref onMoves, cancellationToken);
        }


        private void OnPointerDown(PointerEventData eventData)
        {
            TrySetResult(onPointerDown, onPointerDowns, eventData);
        }


        public UniTask<PointerEventData> OnPointerDownAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerDown, ref onPointerDowns, cancellationToken);
        }


        private void OnPointerEnter(PointerEventData eventData)
        {
            TrySetResult(onPointerEnter, onPointerEnters, eventData);
        }


        public UniTask<PointerEventData> OnPointerEnterAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerEnter, ref onPointerEnters, cancellationToken);
        }


        private void OnPointerExit(PointerEventData eventData)
        {
            TrySetResult(onPointerExit, onPointerExits, eventData);
        }


        public UniTask<PointerEventData> OnPointerExitAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerExit, ref onPointerExits, cancellationToken);
        }


        private void OnPointerUp(PointerEventData eventData)
        {
            TrySetResult(onPointerUp, onPointerUps, eventData);
        }


        public UniTask<PointerEventData> OnPointerUpAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerUp, ref onPointerUps, cancellationToken);
        }


        private void OnSelect(BaseEventData eventData)
        {
            TrySetResult(onSelect, onSelects, eventData);
        }


        public UniTask<BaseEventData> OnSelectAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onSelect, ref onSelects, cancellationToken);
        }


        private void OnPointerClick(PointerEventData eventData)
        {
            TrySetResult(onPointerClick, onPointerClicks, eventData);
        }


        public UniTask<PointerEventData> OnPointerClickAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onPointerClick, ref onPointerClicks, cancellationToken);
        }


        private void OnSubmit(BaseEventData eventData)
        {
            TrySetResult(onSubmit, onSubmits, eventData);
        }


        public UniTask<BaseEventData> OnSubmitAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onSubmit, ref onSubmits, cancellationToken);
        }


        private void OnDrag(PointerEventData eventData)
        {
            TrySetResult(onDrag, onDrags, eventData);
        }


        public UniTask<PointerEventData> OnDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onDrag, ref onDrags, cancellationToken);
        }


        private void OnBeginDrag(PointerEventData eventData)
        {
            TrySetResult(onBeginDrag, onBeginDrags, eventData);
        }


        public UniTask<PointerEventData> OnBeginDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onBeginDrag, ref onBeginDrags, cancellationToken);
        }


        private void OnEndDrag(PointerEventData eventData)
        {
            TrySetResult(onEndDrag, onEndDrags, eventData);
        }


        public UniTask<PointerEventData> OnEndDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onEndDrag, ref onEndDrags, cancellationToken);
        }


        private void OnDrop(PointerEventData eventData)
        {
            TrySetResult(onDrop, onDrops, eventData);
        }


        public UniTask<PointerEventData> OnDropAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onDrop, ref onDrops, cancellationToken);
        }


        private void OnUpdateSelected(BaseEventData eventData)
        {
            TrySetResult(onUpdateSelected, onUpdateSelecteds, eventData);
        }


        public UniTask<BaseEventData> OnUpdateSelectedAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onUpdateSelected, ref onUpdateSelecteds, cancellationToken);
        }


        private void OnInitializePotentialDrag(PointerEventData eventData)
        {
            TrySetResult(onInitializePotentialDrag, onInitializePotentialDrags, eventData);
        }


        public UniTask<PointerEventData> OnInitializePotentialDragAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onInitializePotentialDrag, ref onInitializePotentialDrags, cancellationToken);
        }


        private void OnCancel(BaseEventData eventData)
        {
            TrySetResult(onCancel, onCancels, eventData);
        }


        public UniTask<BaseEventData> OnCancelAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onCancel, ref onCancels, cancellationToken);
        }


        private void OnScroll(PointerEventData eventData)
        {
            TrySetResult(onScroll, onScrolls, eventData);
        }


        public UniTask<PointerEventData> OnScrollAsync(CancellationToken cancellationToken = default)
        {
            return GetOrAddPromise(ref onScroll, ref onScrolls, cancellationToken);
        }
    }
}

#endif
