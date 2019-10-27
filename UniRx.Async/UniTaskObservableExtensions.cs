using System.Runtime.ExceptionServices;
using UniRx.Async.Internal;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Threading;

namespace UniRx.Async
{
    public static class UniTaskObservableExtensions
    {
        public static UniTask<T> ToUniTask<T>(this IObservable<T> source, CancellationToken cancellationToken = default, bool useFirstValue = false)
        {
            var promise = new UniTaskCompletionSource<T>();
            var disposable = new SingleAssignmentDisposable();

            var observer = useFirstValue
                ? new FirstValueToUniTaskObserver<T>(promise, disposable, cancellationToken)
                : (IObserver<T>) new ToUniTaskObserver<T>(promise, disposable, cancellationToken);

            try
            {
                disposable.Disposable = source.Subscribe(observer);
            }
            catch (Exception ex)
            {
                promise.TrySetException(ex);
            }

            return promise.Task;
        }

        public static IObservable<T> ToObservable<T>(this UniTask<T> task)
        {
            if (task.IsCompleted)
                try
                {
                    return new ReturnObservable<T>(task.GetAwaiter().GetResult());
                }
                catch (Exception ex)
                {
                    return new ThrowObservable<T>(ex);
                }

            var subject = new AsyncSubject<T>();
            Fire(subject, task).Forget();
            return subject;
        }

        /// <summary>
        ///     Ideally returns IObservabl[Unit] is best but UniRx.Async does not have Unit so return AsyncUnit instead.
        /// </summary>
        public static IObservable<AsyncUnit> ToObservable(this UniTask task)
        {
            if (task.IsCompleted)
                try
                {
                    return new ReturnObservable<AsyncUnit>(AsyncUnit.Default);
                }
                catch (Exception ex)
                {
                    return new ThrowObservable<AsyncUnit>(ex);
                }

            var subject = new AsyncSubject<AsyncUnit>();
            Fire(subject, task).Forget();
            return subject;
        }

        private static async UniTaskVoid Fire<T>(AsyncSubject<T> subject, UniTask<T> task)
        {
            try
            {
                var value = await task;
                subject.OnNext(value);
                subject.OnCompleted();
            }
            catch (Exception ex)
            {
                subject.OnError(ex);
            }
        }

        private static async UniTaskVoid Fire(AsyncSubject<object> subject, UniTask task)
        {
            try
            {
                await task;
                subject.OnNext(null);
                subject.OnCompleted();
            }
            catch (Exception ex)
            {
                subject.OnError(ex);
            }
        }

        private class ToUniTaskObserver<T> : IObserver<T>
        {
            private static readonly Action<object> callback = OnCanceled;
            private readonly CancellationToken cancellationToken;
            private readonly SingleAssignmentDisposable disposable;

            private readonly UniTaskCompletionSource<T> promise;
            private readonly CancellationTokenRegistration registration;

            private bool hasValue;
            private T latestValue;

            public ToUniTaskObserver(UniTaskCompletionSource<T> promise, SingleAssignmentDisposable disposable, CancellationToken cancellationToken)
            {
                this.promise = promise;
                this.disposable = disposable;
                this.cancellationToken = cancellationToken;

                if (this.cancellationToken.CanBeCanceled) registration = this.cancellationToken.RegisterWithoutCaptureExecutionContext(callback, this);
            }

            public void OnNext(T value)
            {
                hasValue = true;
                latestValue = value;
            }

            public void OnError(Exception error)
            {
                try
                {
                    promise.TrySetException(error);
                }
                finally
                {
                    registration.Dispose();
                    disposable.Dispose();
                }
            }

            public void OnCompleted()
            {
                try
                {
                    if (hasValue)
                        promise.TrySetResult(latestValue);
                    else
                        promise.TrySetException(new InvalidOperationException("Sequence has no elements"));
                }
                finally
                {
                    registration.Dispose();
                    disposable.Dispose();
                }
            }

            private static void OnCanceled(object state)
            {
                var self = (ToUniTaskObserver<T>) state;
                self.disposable.Dispose();
                self.promise.TrySetCanceled();
            }
        }

        private class FirstValueToUniTaskObserver<T> : IObserver<T>
        {
            private static readonly Action<object> callback = OnCanceled;
            private readonly CancellationToken cancellationToken;
            private readonly SingleAssignmentDisposable disposable;

            private readonly UniTaskCompletionSource<T> promise;
            private readonly CancellationTokenRegistration registration;

            private bool hasValue;

            public FirstValueToUniTaskObserver(UniTaskCompletionSource<T> promise, SingleAssignmentDisposable disposable, CancellationToken cancellationToken)
            {
                this.promise = promise;
                this.disposable = disposable;
                this.cancellationToken = cancellationToken;

                if (this.cancellationToken.CanBeCanceled) registration = this.cancellationToken.RegisterWithoutCaptureExecutionContext(callback, this);
            }

            public void OnNext(T value)
            {
                hasValue = true;
                try
                {
                    promise.TrySetResult(value);
                }
                finally
                {
                    registration.Dispose();
                    disposable.Dispose();
                }
            }

            public void OnError(Exception error)
            {
                try
                {
                    promise.TrySetException(error);
                }
                finally
                {
                    registration.Dispose();
                    disposable.Dispose();
                }
            }

            public void OnCompleted()
            {
                try
                {
                    if (!hasValue) promise.TrySetException(new InvalidOperationException("Sequence has no elements"));
                }
                finally
                {
                    registration.Dispose();
                    disposable.Dispose();
                }
            }

            private static void OnCanceled(object state)
            {
                var self = (FirstValueToUniTaskObserver<T>) state;
                self.disposable.Dispose();
                self.promise.TrySetCanceled();
            }
        }

        private class ReturnObservable<T> : IObservable<T>
        {
            private readonly T value;

            public ReturnObservable(T value)
            {
                this.value = value;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                observer.OnNext(value);
                observer.OnCompleted();
                return EmptyDisposable.Instance;
            }
        }

        private class ThrowObservable<T> : IObservable<T>
        {
            private readonly Exception value;

            public ThrowObservable(Exception value)
            {
                this.value = value;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                observer.OnError(value);
                return EmptyDisposable.Instance;
            }
        }
    }
}

namespace UniRx.Async.Internal
{
    // Bridges for Rx.

    internal class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance = new EmptyDisposable();

        private EmptyDisposable()
        {
        }

        public void Dispose()
        {
        }
    }

    internal sealed class SingleAssignmentDisposable : IDisposable
    {
        private readonly object gate = new object();
        private IDisposable current;
        private bool disposed;

        public bool IsDisposed
        {
            get
            {
                lock (gate)
                {
                    return disposed;
                }
            }
        }

        public IDisposable Disposable
        {
            get => current;
            set
            {
                var old = default(IDisposable);
                bool alreadyDisposed;
                lock (gate)
                {
                    alreadyDisposed = disposed;
                    old = current;
                    if (!alreadyDisposed)
                    {
                        if (value == null) return;
                        current = value;
                    }
                }

                if (alreadyDisposed && value != null)
                {
                    value.Dispose();
                    return;
                }

                if (old != null) throw new InvalidOperationException("Disposable is already set");
            }
        }


        public void Dispose()
        {
            IDisposable old = null;

            lock (gate)
            {
                if (!disposed)
                {
                    disposed = true;
                    old = current;
                    current = null;
                }
            }

            if (old != null) old.Dispose();
        }
    }

    internal sealed class AsyncSubject<T> : IObservable<T>, IObserver<T>
    {
        private readonly object observerLock = new object();
        private bool hasValue;
        private bool isDisposed;
        private Exception lastError;

        private T lastValue;
        private IObserver<T> outObserver = EmptyObserver<T>.Instance;

        public T Value
        {
            get
            {
                ThrowIfDisposed();
                if (!IsCompleted) throw new InvalidOperationException("AsyncSubject is not completed yet");
                if (lastError != null) ExceptionDispatchInfo.Capture(lastError).Throw();
                return lastValue;
            }
        }

        public bool HasObservers => !(outObserver is EmptyObserver<T>) && !IsCompleted && !isDisposed;

        public bool IsCompleted { get; private set; }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException("observer");

            var ex = default(Exception);
            var v = default(T);
            var hv = false;

            lock (observerLock)
            {
                ThrowIfDisposed();
                if (!IsCompleted)
                {
                    var listObserver = outObserver as ListObserver<T>;
                    if (listObserver != null)
                    {
                        outObserver = listObserver.Add(observer);
                    }
                    else
                    {
                        var current = outObserver;
                        if (current is EmptyObserver<T>)
                            outObserver = observer;
                        else
                            outObserver = new ListObserver<T>(new ImmutableList<IObserver<T>>(new[] {current, observer}));
                    }

                    return new Subscription(this, observer);
                }

                ex = lastError;
                v = lastValue;
                hv = hasValue;
            }

            if (ex != null)
            {
                observer.OnError(ex);
            }
            else if (hv)
            {
                observer.OnNext(v);
                observer.OnCompleted();
            }
            else
            {
                observer.OnCompleted();
            }

            return EmptyDisposable.Instance;
        }

        public void OnCompleted()
        {
            IObserver<T> old;
            T v;
            bool hv;
            lock (observerLock)
            {
                ThrowIfDisposed();
                if (IsCompleted) return;

                old = outObserver;
                outObserver = EmptyObserver<T>.Instance;
                IsCompleted = true;
                v = lastValue;
                hv = hasValue;
            }

            if (hv)
            {
                old.OnNext(v);
                old.OnCompleted();
            }
            else
            {
                old.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            if (error == null) throw new ArgumentNullException("error");

            IObserver<T> old;
            lock (observerLock)
            {
                ThrowIfDisposed();
                if (IsCompleted) return;

                old = outObserver;
                outObserver = EmptyObserver<T>.Instance;
                IsCompleted = true;
                lastError = error;
            }

            old.OnError(error);
        }

        public void OnNext(T value)
        {
            lock (observerLock)
            {
                ThrowIfDisposed();
                if (IsCompleted) return;

                hasValue = true;
                lastValue = value;
            }
        }

        public void Dispose()
        {
            lock (observerLock)
            {
                isDisposed = true;
                outObserver = DisposedObserver<T>.Instance;
                lastError = null;
                lastValue = default;
            }
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException("");
        }

        private class Subscription : IDisposable
        {
            private readonly object gate = new object();
            private AsyncSubject<T> parent;
            private IObserver<T> unsubscribeTarget;

            public Subscription(AsyncSubject<T> parent, IObserver<T> unsubscribeTarget)
            {
                this.parent = parent;
                this.unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                lock (gate)
                {
                    if (parent != null)
                        lock (parent.observerLock)
                        {
                            var listObserver = parent.outObserver as ListObserver<T>;
                            if (listObserver != null)
                                parent.outObserver = listObserver.Remove(unsubscribeTarget);
                            else
                                parent.outObserver = EmptyObserver<T>.Instance;

                            unsubscribeTarget = null;
                            parent = null;
                        }
                }
            }
        }
    }

    internal class ListObserver<T> : IObserver<T>
    {
        private readonly ImmutableList<IObserver<T>> _observers;

        public ListObserver(ImmutableList<IObserver<T>> observers)
        {
            _observers = observers;
        }

        public void OnCompleted()
        {
            var targetObservers = _observers.Data;
            for (var i = 0; i < targetObservers.Length; i++) targetObservers[i].OnCompleted();
        }

        public void OnError(Exception error)
        {
            var targetObservers = _observers.Data;
            for (var i = 0; i < targetObservers.Length; i++) targetObservers[i].OnError(error);
        }

        public void OnNext(T value)
        {
            var targetObservers = _observers.Data;
            for (var i = 0; i < targetObservers.Length; i++) targetObservers[i].OnNext(value);
        }

        internal IObserver<T> Add(IObserver<T> observer)
        {
            return new ListObserver<T>(_observers.Add(observer));
        }

        internal IObserver<T> Remove(IObserver<T> observer)
        {
            var i = Array.IndexOf(_observers.Data, observer);
            if (i < 0)
                return this;

            if (_observers.Data.Length == 2)
                return _observers.Data[1 - i];
            return new ListObserver<T>(_observers.Remove(observer));
        }
    }

    internal class EmptyObserver<T> : IObserver<T>
    {
        public static readonly EmptyObserver<T> Instance = new EmptyObserver<T>();

        private EmptyObserver()
        {
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
        }
    }

    internal class ThrowObserver<T> : IObserver<T>
    {
        public static readonly ThrowObserver<T> Instance = new ThrowObserver<T>();

        private ThrowObserver()
        {
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }

        public void OnNext(T value)
        {
        }
    }

    internal class DisposedObserver<T> : IObserver<T>
    {
        public static readonly DisposedObserver<T> Instance = new DisposedObserver<T>();

        private DisposedObserver()
        {
        }

        public void OnCompleted()
        {
            throw new ObjectDisposedException("");
        }

        public void OnError(Exception error)
        {
            throw new ObjectDisposedException("");
        }

        public void OnNext(T value)
        {
            throw new ObjectDisposedException("");
        }
    }

    internal class ImmutableList<T>
    {
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        public T[] Data { get; }

        private ImmutableList()
        {
            Data = new T[0];
        }

        public ImmutableList(T[] data)
        {
            Data = data;
        }

        public ImmutableList<T> Add(T value)
        {
            var newData = new T[Data.Length + 1];
            Array.Copy(Data, newData, Data.Length);
            newData[Data.Length] = value;
            return new ImmutableList<T>(newData);
        }

        public ImmutableList<T> Remove(T value)
        {
            var i = IndexOf(value);
            if (i < 0) return this;

            var length = Data.Length;
            if (length == 1) return Empty;

            var newData = new T[length - 1];

            Array.Copy(Data, 0, newData, 0, i);
            Array.Copy(Data, i + 1, newData, i, length - i - 1);

            return new ImmutableList<T>(newData);
        }

        public int IndexOf(T value)
        {
            for (var i = 0; i < Data.Length; ++i)
                // ImmutableList only use for IObserver(no worry for boxed)
                if (Equals(Data[i], value))
                    return i;
            return -1;
        }
    }
}

#endif
