using System;
using System.Threading;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using UniRx.Async.Internal;
using UnityEngine;

namespace UniRx.Async
{
    public static partial class UnityAsyncExtensions
    {
        public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new AsyncOperationAwaiter(asyncOperation);
        }

        public static UniTask ToUniTask(this AsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new UniTask(new AsyncOperationAwaiter(asyncOperation));
        }

        public static UniTask ConfigureAwait(this AsyncOperation asyncOperation, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));

            var awaiter = new AsyncOperationConfiguredAwaiter(asyncOperation, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask(awaiter);
        }

        public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new ResourceRequestAwaiter(resourceRequest);
        }

        public static UniTask<Object> ToUniTask(this ResourceRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new UniTask<Object>(new ResourceRequestAwaiter(resourceRequest));
        }

        public static UniTask<Object> ConfigureAwait(this ResourceRequest resourceRequest, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));

            var awaiter = new ResourceRequestConfiguredAwaiter(resourceRequest, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask<Object>(awaiter);
        }

        public static AssetBundleRequestAwaiter GetAwaiter(this AssetBundleRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new AssetBundleRequestAwaiter(resourceRequest);
        }

        public static UniTask<Object> ToUniTask(this AssetBundleRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new UniTask<Object>(new AssetBundleRequestAwaiter(resourceRequest));
        }

        public static UniTask<Object> ConfigureAwait(this AssetBundleRequest resourceRequest, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));

            var awaiter = new AssetBundleRequestConfiguredAwaiter(resourceRequest, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask<Object>(awaiter);
        }

        public static AssetBundleCreateRequestAwaiter GetAwaiter(this AssetBundleCreateRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new AssetBundleCreateRequestAwaiter(resourceRequest);
        }

        public static UniTask<Object> ToUniTask(this AssetBundleCreateRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new UniTask<Object>(new AssetBundleCreateRequestAwaiter(resourceRequest));
        }

        public static UniTask<Object> ConfigureAwait(this AssetBundleCreateRequest resourceRequest, IProgress<float> progress = null,
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));

            var awaiter = new AssetBundleCreateRequestConfiguredAwaiter(resourceRequest, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask<Object>(awaiter);
        }

        public struct AsyncOperationAwaiter : IAwaiter
        {
            private AsyncOperation asyncOperation;
            private Action<AsyncOperation> continuationAction;

            public AsyncOperationAwaiter(AsyncOperation asyncOperation)
            {
                Status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = Status.IsCompleted() ? null : asyncOperation;
                continuationAction = null;
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            public void GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return;

                if (Status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                        Status = AwaiterStatus.Succeeded;
                    else
                        Error.ThrowNotYetCompleted();
                }

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class AsyncOperationConfiguredAwaiter : IAwaiter, IPlayerLoopItem
        {
            private AsyncOperation asyncOperation;
            private CancellationToken cancellationToken;
            private Action continuation;
            private IProgress<float> progress;

            public AsyncOperationConfiguredAwaiter(AsyncOperation asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;

                TaskTracker.TrackActiveTask(this, 2);
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            public void GetResult()
            {
                if (Status == AwaiterStatus.Succeeded)
                    return;
                if (Status == AwaiterStatus.Canceled) Error.ThrowOperationCanceledException();

                Error.ThrowNotYetCompleted();
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null) progress.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;

                if (cont != null) cont.Invoke();
            }
        }

        public struct ResourceRequestAwaiter : IAwaiter<Object>
        {
            private ResourceRequest asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private Object result;

            public ResourceRequestAwaiter(ResourceRequest asyncOperation)
            {
                Status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = Status.IsCompleted() ? null : asyncOperation;
                result = Status.IsCompletedSuccessfully() ? asyncOperation.asset : null;
                continuationAction = null;
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            public Object GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                        Status = AwaiterStatus.Succeeded;
                    else
                        Error.ThrowNotYetCompleted();
                }

                result = asyncOperation.asset;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }

                return result;
            }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class ResourceRequestConfiguredAwaiter : IAwaiter<Object>, IPlayerLoopItem
        {
            private ResourceRequest asyncOperation;
            private CancellationToken cancellationToken;
            private Action continuation;
            private IProgress<float> progress;
            private Object result;

            public ResourceRequestConfiguredAwaiter(ResourceRequest asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompletedSuccessfully()) result = asyncOperation.asset;
                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;
                result = null;

                TaskTracker.TrackActiveTask(this, 2);
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public Object GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Canceled) Error.ThrowOperationCanceledException();

                return Error.ThrowNotYetCompleted<Object>();
            }

            public void OnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null) progress.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    result = asyncOperation.asset;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;

                if (cont != null) cont.Invoke();
            }
        }

        public struct AssetBundleRequestAwaiter : IAwaiter<Object>
        {
            private AssetBundleRequest asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private Object result;

            public AssetBundleRequestAwaiter(AssetBundleRequest asyncOperation)
            {
                Status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = Status.IsCompleted() ? null : asyncOperation;
                result = Status.IsCompletedSuccessfully() ? asyncOperation.asset : null;
                continuationAction = null;
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            public Object GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                        Status = AwaiterStatus.Succeeded;
                    else
                        Error.ThrowNotYetCompleted();
                }

                result = asyncOperation.asset;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }

                return result;
            }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        public struct AssetBundleCreateRequestAwaiter : IAwaiter<AssetBundle>
        {
            private AssetBundleCreateRequest asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private AssetBundle result;

            public AssetBundleCreateRequestAwaiter(AssetBundleCreateRequest asyncOperation)
            {
                Status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = Status.IsCompleted() ? null : asyncOperation;
                result = Status.IsCompletedSuccessfully() ? asyncOperation.assetBundle : null;
                continuationAction = null;
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            public AssetBundle GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                        Status = AwaiterStatus.Succeeded;
                    else
                        Error.ThrowNotYetCompleted();
                }

                result = asyncOperation.assetBundle;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }

                return result;
            }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class AssetBundleRequestConfiguredAwaiter : IAwaiter<Object>, IPlayerLoopItem
        {
            private AssetBundleRequest asyncOperation;
            private CancellationToken cancellationToken;
            private Action continuation;
            private IProgress<float> progress;
            private Object result;

            public AssetBundleRequestConfiguredAwaiter(AssetBundleRequest asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompletedSuccessfully()) result = asyncOperation.asset;
                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;
                result = null;

                TaskTracker.TrackActiveTask(this, 2);
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public Object GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Canceled) Error.ThrowOperationCanceledException();

                return Error.ThrowNotYetCompleted<Object>();
            }

            public void OnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null) progress.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    result = asyncOperation.asset;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;

                if (cont != null) cont.Invoke();
            }
        }

        private class AssetBundleCreateRequestConfiguredAwaiter : IAwaiter<AssetBundle>, IPlayerLoopItem
        {
            private AssetBundleCreateRequest asyncOperation;
            private CancellationToken cancellationToken;
            private Action continuation;
            private IProgress<float> progress;
            private AssetBundle result;

            public AssetBundleCreateRequestConfiguredAwaiter(AssetBundleCreateRequest asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompletedSuccessfully()) result = asyncOperation.assetBundle;
                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;
                result = null;

                TaskTracker.TrackActiveTask(this, 2);
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public AssetBundle GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Canceled) Error.ThrowOperationCanceledException();

                return Error.ThrowNotYetCompleted<AssetBundle>();
            }

            public void OnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null) progress.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    result = asyncOperation.assetBundle;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;

                if (cont != null) cont.Invoke();
            }
        }

#if ENABLE_WWW

#if UNITY_2018_3_OR_NEWER
#pragma warning disable CS0618
#endif

        private class WWWConfiguredAwaiter : IAwaiter, IPlayerLoopItem
        {
            private WWW asyncOperation;
            private CancellationToken cancellationToken;
            private Action continuation;
            private IProgress<float> progress;

            public WWWConfiguredAwaiter(WWW asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;

                TaskTracker.TrackActiveTask(this, 2);
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            public void GetResult()
            {
                if (Status == AwaiterStatus.Succeeded)
                    return;
                if (Status == AwaiterStatus.Canceled) Error.ThrowOperationCanceledException();

                Error.ThrowNotYetCompleted();
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null) progress.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;

                if (cont != null) cont.Invoke();
            }
        }

#if UNITY_2018_3_OR_NEWER
#pragma warning restore CS0618
#endif

#endif

#if ENABLE_WWW

#if UNITY_2018_3_OR_NEWER
#pragma warning disable CS0618
#endif

        public static IAwaiter GetAwaiter(this WWW www)
        {
            Error.ThrowArgumentNullException(www, nameof(www));

            var awaiter = new WWWConfiguredAwaiter(www, null, CancellationToken.None);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, awaiter);
            return awaiter;
        }

        public static UniTask ToUniTask(this WWW www)
        {
            Error.ThrowArgumentNullException(www, nameof(www));

            var awaiter = new WWWConfiguredAwaiter(www, null, CancellationToken.None);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, awaiter);
            return new UniTask(awaiter);
        }

        public static UniTask ConfigureAwait(this WWW www, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(www, nameof(www));

            var awaiter = new WWWConfiguredAwaiter(www, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask(awaiter);
        }

#if UNITY_2018_3_OR_NEWER
#pragma warning restore CS0618
#endif

#endif

#if ENABLE_UNITYWEBREQUEST

        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
        }

        public static UniTask<UnityWebRequest> ToUniTask(this UnityWebRequestAsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new UniTask<UnityWebRequest>(new UnityWebRequestAsyncOperationAwaiter(asyncOperation));
        }

        public static UniTask<UnityWebRequest> ConfigureAwait(this UnityWebRequestAsyncOperation asyncOperation, IProgress<float> progress = null,
            PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));

            var awaiter = new UnityWebRequestAsyncOperationConfiguredAwaiter(asyncOperation, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask<UnityWebRequest>(awaiter);
        }

#endif

#if ENABLE_UNITYWEBREQUEST

        public struct UnityWebRequestAsyncOperationAwaiter : IAwaiter<UnityWebRequest>
        {
            private UnityWebRequestAsyncOperation asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private UnityWebRequest result;

            public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation asyncOperation)
            {
                Status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = Status.IsCompleted() ? null : asyncOperation;
                result = Status.IsCompletedSuccessfully() ? asyncOperation.webRequest : null;
                continuationAction = null;
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            public UnityWebRequest GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                        Status = AwaiterStatus.Succeeded;
                    else
                        Error.ThrowNotYetCompleted();
                }

                result = asyncOperation.webRequest;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }


                return result;
            }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class UnityWebRequestAsyncOperationConfiguredAwaiter : IAwaiter<UnityWebRequest>, IPlayerLoopItem
        {
            private UnityWebRequestAsyncOperation asyncOperation;
            private CancellationToken cancellationToken;
            private Action continuation;
            private IProgress<float> progress;
            private UnityWebRequest result;

            public UnityWebRequestAsyncOperationConfiguredAwaiter(UnityWebRequestAsyncOperation asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompletedSuccessfully()) result = asyncOperation.webRequest;
                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;
                result = null;

                TaskTracker.TrackActiveTask(this, 2);
            }

            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            void IAwaiter.GetResult()
            {
                GetResult();
            }

            public UnityWebRequest GetResult()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Canceled) Error.ThrowOperationCanceledException();

                return Error.ThrowNotYetCompleted<UnityWebRequest>();
            }

            public void OnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null) progress.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    result = asyncOperation.webRequest;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;

                if (cont != null) cont.Invoke();
            }
        }

#endif
    }
}
#endif
