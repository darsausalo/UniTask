using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using UniRx.Async.Internal;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections;
using System.Threading;

namespace UniRx.Async
{
    public static class EnumeratorAsyncExtensions
    {
        public static IAwaiter GetAwaiter(this IEnumerator enumerator)
        {
            var awaiter = new EnumeratorAwaiter(enumerator, CancellationToken.None);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, awaiter);
            return awaiter;
        }

        public static UniTask ToUniTask(this IEnumerator enumerator)
        {
            var awaiter = new EnumeratorAwaiter(enumerator, CancellationToken.None);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, awaiter);
            return new UniTask(awaiter);
        }

        public static UniTask ConfigureAwait(this IEnumerator enumerator, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            var awaiter = new EnumeratorAwaiter(enumerator, cancellationToken);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask(awaiter);
        }

        private class EnumeratorAwaiter : IAwaiter, IPlayerLoopItem
        {
            private static readonly FieldInfo waitForSeconds_Seconds =
                typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

            private CancellationToken cancellationToken;
            private Action continuation;
            private ExceptionDispatchInfo exception;
            private IEnumerator innerEnumerator;

            public EnumeratorAwaiter(IEnumerator innerEnumerator, CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Status = AwaiterStatus.Canceled;
                    return;
                }

                this.innerEnumerator = ConsumeEnumerator(innerEnumerator);
                Status = AwaiterStatus.Pending;
                this.cancellationToken = cancellationToken;
                continuation = null;

                TaskTracker.TrackActiveTask(this, 2);
            }

            public bool IsCompleted => Status.IsCompleted();

            public AwaiterStatus Status { get; private set; }

            public void GetResult()
            {
                switch (Status)
                {
                    case AwaiterStatus.Succeeded:
                        break;
                    case AwaiterStatus.Pending:
                        Error.ThrowNotYetCompleted();
                        break;
                    case AwaiterStatus.Faulted:
                        exception.Throw();
                        break;
                    case AwaiterStatus.Canceled:
                        Error.ThrowOperationCanceledException();
                        break;
                }
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

                var success = false;
                try
                {
                    if (innerEnumerator.MoveNext())
                        return true;
                    success = true;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                }

                InvokeContinuation(success ? AwaiterStatus.Succeeded : AwaiterStatus.Faulted);
                return false;
            }

            private void InvokeContinuation(AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;

                // cleanup
                TaskTracker.RemoveTracking(this);
                continuation = null;
                cancellationToken = CancellationToken.None;
                innerEnumerator = null;

                if (cont != null) cont.Invoke();
            }

            // Unwrap YieldInstructions

            private static IEnumerator ConsumeEnumerator(IEnumerator enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current == null)
                    {
                        yield return null;
                    }
                    else if (current is CustomYieldInstruction)
                    {
                        // WWW, WaitForSecondsRealtime
                        var e2 = UnwrapWaitCustomYieldInstruction((CustomYieldInstruction) current);
                        while (e2.MoveNext()) yield return null;
                    }
                    else if (current is YieldInstruction)
                    {
                        IEnumerator innerCoroutine = null;
                        switch (current)
                        {
                            case AsyncOperation ao:
                                innerCoroutine = UnwrapWaitAsyncOperation(ao);
                                break;
                            case WaitForSeconds wfs:
                                innerCoroutine = UnwrapWaitForSeconds(wfs);
                                break;
                        }

                        if (innerCoroutine != null)
                            while (innerCoroutine.MoveNext())
                                yield return null;
                        else
                            yield return null;
                    }
                    else if (current is IEnumerator e3)
                    {
                        var e4 = ConsumeEnumerator(e3);
                        while (e4.MoveNext()) yield return null;
                    }
                    else
                    {
                        // WaitForEndOfFrame, WaitForFixedUpdate, others.
                        yield return null;
                    }
                }
            }

            // WWW and others as CustomYieldInstruction.
            private static IEnumerator UnwrapWaitCustomYieldInstruction(CustomYieldInstruction yieldInstruction)
            {
                while (yieldInstruction.keepWaiting) yield return null;
            }

            private static IEnumerator UnwrapWaitForSeconds(WaitForSeconds waitForSeconds)
            {
                var second = (float) waitForSeconds_Seconds.GetValue(waitForSeconds);
                var startTime = DateTimeOffset.UtcNow;
                while (true)
                {
                    yield return null;

                    var elapsed = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
                    if (elapsed >= second) break;
                }

                ;
            }

            private static IEnumerator UnwrapWaitAsyncOperation(AsyncOperation asyncOperation)
            {
                while (!asyncOperation.isDone) yield return null;
            }
        }
    }
}

#endif
