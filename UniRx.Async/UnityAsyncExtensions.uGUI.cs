﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UniRx.Async.Internal;
using UnityEngine;
using UnityEngine.UI;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using UniRx.Async.Triggers;
using UnityEngine.Events;

namespace UniRx.Async
{
    public static partial class UnityAsyncExtensions
    {
        public static AsyncUnityEventHandler GetAsyncEventHandler(this UnityEvent unityEvent, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler(unityEvent, cancellationToken, false);
        }

        public static UniTask OnInvokeAsync(this UnityEvent unityEvent, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler(unityEvent, cancellationToken, true).OnInvokeAsync();
        }

        public static IAsyncClickEventHandler GetAsyncClickEventHandler(this Button button)
        {
            return new AsyncUnityEventHandler(button.onClick, button.GetCancellationTokenOnDestroy(), false);
        }

        public static IAsyncClickEventHandler GetAsyncClickEventHandler(this Button button, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler(button.onClick, cancellationToken, false);
        }

        public static UniTask OnClickAsync(this Button button)
        {
            return new AsyncUnityEventHandler(button.onClick, button.GetCancellationTokenOnDestroy(), true).OnInvokeAsync();
        }

        public static UniTask OnClickAsync(this Button button, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler(button.onClick, cancellationToken, true).OnInvokeAsync();
        }

        public static IAsyncValueChangedEventHandler<bool> GetAsyncValueChangedEventHandler(this Toggle toggle)
        {
            return new AsyncUnityEventHandler<bool>(toggle.onValueChanged, toggle.GetCancellationTokenOnDestroy(), false);
        }

        public static IAsyncValueChangedEventHandler<bool> GetAsyncValueChangedEventHandler(this Toggle toggle, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<bool>(toggle.onValueChanged, cancellationToken, false);
        }

        public static UniTask<bool> OnValueChangedAsync(this Toggle toggle)
        {
            return new AsyncUnityEventHandler<bool>(toggle.onValueChanged, toggle.GetCancellationTokenOnDestroy(), true).OnInvokeAsync();
        }

        public static UniTask<bool> OnValueChangedAsync(this Toggle toggle, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<bool>(toggle.onValueChanged, cancellationToken, true).OnInvokeAsync();
        }

        public static IAsyncValueChangedEventHandler<float> GetAsyncValueChangedEventHandler(this Scrollbar scrollbar)
        {
            return new AsyncUnityEventHandler<float>(scrollbar.onValueChanged, scrollbar.GetCancellationTokenOnDestroy(), false);
        }

        public static IAsyncValueChangedEventHandler<float> GetAsyncValueChangedEventHandler(this Scrollbar scrollbar, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<float>(scrollbar.onValueChanged, cancellationToken, false);
        }

        public static UniTask<float> OnValueChangedAsync(this Scrollbar scrollbar)
        {
            return new AsyncUnityEventHandler<float>(scrollbar.onValueChanged, scrollbar.GetCancellationTokenOnDestroy(), true).OnInvokeAsync();
        }

        public static UniTask<float> OnValueChangedAsync(this Scrollbar scrollbar, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<float>(scrollbar.onValueChanged, cancellationToken, true).OnInvokeAsync();
        }

        public static IAsyncValueChangedEventHandler<Vector2> GetAsyncValueChangedEventHandler(this ScrollRect scrollRect)
        {
            return new AsyncUnityEventHandler<Vector2>(scrollRect.onValueChanged, scrollRect.GetCancellationTokenOnDestroy(), false);
        }

        public static IAsyncValueChangedEventHandler<Vector2> GetAsyncValueChangedEventHandler(this ScrollRect scrollRect, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<Vector2>(scrollRect.onValueChanged, cancellationToken, false);
        }

        public static UniTask<Vector2> OnValueChangedAsync(this ScrollRect scrollRect)
        {
            return new AsyncUnityEventHandler<Vector2>(scrollRect.onValueChanged, scrollRect.GetCancellationTokenOnDestroy(), true).OnInvokeAsync();
        }

        public static UniTask<Vector2> OnValueChangedAsync(this ScrollRect scrollRect, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<Vector2>(scrollRect.onValueChanged, cancellationToken, true).OnInvokeAsync();
        }

        public static IAsyncValueChangedEventHandler<float> GetAsyncValueChangedEventHandler(this Slider slider)
        {
            return new AsyncUnityEventHandler<float>(slider.onValueChanged, slider.GetCancellationTokenOnDestroy(), false);
        }

        public static IAsyncValueChangedEventHandler<float> GetAsyncValueChangedEventHandler(this Slider slider, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<float>(slider.onValueChanged, cancellationToken, false);
        }

        public static UniTask<float> OnValueChangedAsync(this Slider slider)
        {
            return new AsyncUnityEventHandler<float>(slider.onValueChanged, slider.GetCancellationTokenOnDestroy(), true).OnInvokeAsync();
        }

        public static UniTask<float> OnValueChangedAsync(this Slider slider, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<float>(slider.onValueChanged, cancellationToken, true).OnInvokeAsync();
        }

        public static IAsyncEndEditEventHandler<string> GetAsyncEndEditEventHandler(this InputField inputField)
        {
            return new AsyncUnityEventHandler<string>(inputField.onEndEdit, inputField.GetCancellationTokenOnDestroy(), false);
        }

        public static IAsyncEndEditEventHandler<string> GetAsyncEndEditEventHandler(this InputField inputField, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<string>(inputField.onEndEdit, cancellationToken, false);
        }

        public static UniTask<string> OnEndEditAsync(this InputField inputField)
        {
            return new AsyncUnityEventHandler<string>(inputField.onEndEdit, inputField.GetCancellationTokenOnDestroy(), true).OnInvokeAsync();
        }

        public static UniTask<string> OnEndEditAsync(this InputField inputField, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<string>(inputField.onEndEdit, cancellationToken, true).OnInvokeAsync();
        }

        public static IAsyncValueChangedEventHandler<int> GetAsyncValueChangedEventHandler(this Dropdown dropdown)
        {
            return new AsyncUnityEventHandler<int>(dropdown.onValueChanged, dropdown.GetCancellationTokenOnDestroy(), false);
        }

        public static IAsyncValueChangedEventHandler<int> GetAsyncValueChangedEventHandler(this Dropdown dropdown, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<int>(dropdown.onValueChanged, cancellationToken, false);
        }

        public static UniTask<int> OnValueChanged(this Dropdown dropdown)
        {
            return new AsyncUnityEventHandler<int>(dropdown.onValueChanged, dropdown.GetCancellationTokenOnDestroy(), true).OnInvokeAsync();
        }

        public static UniTask<int> OnValueChanged(this Dropdown dropdown, CancellationToken cancellationToken)
        {
            return new AsyncUnityEventHandler<int>(dropdown.onValueChanged, cancellationToken, true).OnInvokeAsync();
        }
    }

    public interface IAsyncClickEventHandler : IDisposable
    {
        UniTask OnClickAsync();
        UniTask<bool> OnClickAsyncSuppressCancellationThrow();
    }

    public interface IAsyncValueChangedEventHandler<T> : IDisposable
    {
        UniTask<T> OnValueChangedAsync();
        UniTask<(bool IsCanceled, T Result)> OnValueChangedAsyncSuppressCancellationThrow();
    }

    public interface IAsyncEndEditEventHandler<T> : IDisposable
    {
        UniTask<T> OnEndEditAsync();
        UniTask<(bool IsCanceled, T Result)> OnEndEditAsyncSuppressCancellationThrow();
    }

    // event handler is reusable when callOnce = false.
    public class AsyncUnityEventHandler : IAwaiter, IDisposable, IAsyncClickEventHandler
    {
        private static readonly Action<object> cancellationCallback = CancellationCallback;

        private readonly UnityAction action;
        private readonly bool callOnce;
        private readonly UnityEvent unityEvent;
        private Action continuation;
        private bool isDisposed;
        private CancellationTokenRegistration registration;
        private UniTask<bool>? suppressCancellationThrowTask;

        public AsyncUnityEventHandler(UnityEvent unityEvent, CancellationToken cancellationToken, bool callOnce)
        {
            this.callOnce = callOnce;

            if (cancellationToken.IsCancellationRequested)
            {
                isDisposed = true;
                return;
            }

            action = Invoke;
            unityEvent.AddListener(action);
            this.unityEvent = unityEvent;

            if (cancellationToken.CanBeCanceled) registration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallback, this);

            TaskTracker.TrackActiveTask(this, 3);
        }

        // Interface events.

        UniTask IAsyncClickEventHandler.OnClickAsync()
        {
            return OnInvokeAsync();
        }

        UniTask<bool> IAsyncClickEventHandler.OnClickAsyncSuppressCancellationThrow()
        {
            return OnInvokeAsyncSuppressCancellationThrow();
        }

        bool IAwaiter.IsCompleted => isDisposed ? true : false;
        AwaiterStatus IAwaiter.Status => isDisposed ? AwaiterStatus.Canceled : AwaiterStatus.Pending;

        void IAwaiter.GetResult()
        {
            if (isDisposed) throw new OperationCanceledException();
            if (callOnce) Dispose();
        }

        void INotifyCompletion.OnCompleted(Action action)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(action);
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            Error.ThrowWhenContinuationIsAlreadyRegistered(continuation);
            continuation = action;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                TaskTracker.RemoveTracking(this);
                registration.Dispose();
                if (unityEvent != null) unityEvent.RemoveListener(action);
            }
        }

        public UniTask OnInvokeAsync()
        {
            // zero allocation wait handler.
            return new UniTask(this);
        }

        public UniTask<bool> OnInvokeAsyncSuppressCancellationThrow()
        {
            if (suppressCancellationThrowTask == null) suppressCancellationThrowTask = OnInvokeAsync().SuppressCancellationThrow();
            return suppressCancellationThrowTask.Value;
        }

        private void Invoke()
        {
            var c = continuation;
            continuation = null;
            if (c != null) c.Invoke();
        }

        private static void CancellationCallback(object state)
        {
            var self = (AsyncUnityEventHandler) state;
            self.Dispose();
            self.Invoke(); // call continuation if exists yet(GetResult -> throw OperationCanceledException).
        }
    }

    // event handler is reusable when callOnce = false.
    public class AsyncUnityEventHandler<T> : IAwaiter<T>, IDisposable, IAsyncValueChangedEventHandler<T>, IAsyncEndEditEventHandler<T>
    {
        private static readonly Action<object> cancellationCallback = CancellationCallback;

        private readonly UnityAction<T> action;
        private readonly bool callOnce;
        private readonly UnityEvent<T> unityEvent;
        private Action continuation;
        private T eventValue;
        private bool isDisposed;
        private CancellationTokenRegistration registration;
        private UniTask<(bool, T)>? suppressCancellationThrowTask;

        public AsyncUnityEventHandler(UnityEvent<T> unityEvent, CancellationToken cancellationToken, bool callOnce)
        {
            this.callOnce = callOnce;

            if (cancellationToken.IsCancellationRequested)
            {
                isDisposed = true;
                return;
            }

            action = Invoke;
            unityEvent.AddListener(action);
            this.unityEvent = unityEvent;

            if (cancellationToken.CanBeCanceled) registration = cancellationToken.RegisterWithoutCaptureExecutionContext(cancellationCallback, this);

            TaskTracker.TrackActiveTask(this, 3);
        }

        UniTask<T> IAsyncEndEditEventHandler<T>.OnEndEditAsync()
        {
            return OnInvokeAsync();
        }

        UniTask<(bool IsCanceled, T Result)> IAsyncEndEditEventHandler<T>.OnEndEditAsyncSuppressCancellationThrow()
        {
            return OnInvokeAsyncSuppressCancellationThrow();
        }

        // Interface events.

        UniTask<T> IAsyncValueChangedEventHandler<T>.OnValueChangedAsync()
        {
            return OnInvokeAsync();
        }

        UniTask<(bool IsCanceled, T Result)> IAsyncValueChangedEventHandler<T>.OnValueChangedAsyncSuppressCancellationThrow()
        {
            return OnInvokeAsyncSuppressCancellationThrow();
        }

        bool IAwaiter.IsCompleted => isDisposed ? true : false;
        AwaiterStatus IAwaiter.Status => isDisposed ? AwaiterStatus.Canceled : AwaiterStatus.Pending;

        T IAwaiter<T>.GetResult()
        {
            if (isDisposed) throw new OperationCanceledException();
            if (callOnce) Dispose();
            return eventValue;
        }

        void IAwaiter.GetResult()
        {
            if (isDisposed) throw new OperationCanceledException();
            if (callOnce) Dispose();
        }

        void INotifyCompletion.OnCompleted(Action action)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(action);
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            Error.ThrowWhenContinuationIsAlreadyRegistered(continuation);
            continuation = action;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                TaskTracker.RemoveTracking(this);
                registration.Dispose();
                if (unityEvent != null) unityEvent.RemoveListener(action);
            }
        }

        public UniTask<T> OnInvokeAsync()
        {
            // zero allocation wait handler.
            return new UniTask<T>(this);
        }

        public UniTask<(bool IsCanceled, T Result)> OnInvokeAsyncSuppressCancellationThrow()
        {
            if (suppressCancellationThrowTask == null) suppressCancellationThrowTask = OnInvokeAsync().SuppressCancellationThrow();
            return suppressCancellationThrowTask.Value;
        }

        private void Invoke(T value)
        {
            eventValue = value;

            var c = continuation;
            continuation = null;
            if (c != null) c.Invoke();
        }

        private static void CancellationCallback(object state)
        {
            var self = (AsyncUnityEventHandler<T>) state;
            self.Dispose();
            self.Invoke(default); // call continuation if exists yet(GetResult -> throw OperationCanceledException).
        }
    }
}

#endif
