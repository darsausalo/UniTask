using System;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Threading;
using UnityEngine;

namespace UniRx.Async
{
    // UniTask has no scheduler like TaskScheduler.
    // Only handle unobserved exception.

    public static class UniTaskScheduler
    {
        /// <summary>
        ///     Propagate OperationCanceledException to UnobservedTaskException when true. Default is false.
        /// </summary>
        public static bool PropagateOperationCanceledException = false;

        /// <summary>
        ///     Write log type when catch unobserved exception and not registered UnobservedTaskException. Default is Warning.
        /// </summary>
        public static LogType UnobservedExceptionWriteLogType = LogType.Warning;

        /// <summary>
        ///     Dispatch exception event to Unity MainThread.
        /// </summary>
        public static bool DispatchUnityMainThread = true;

        // cache delegate.
        private static readonly SendOrPostCallback handleExceptionInvoke = InvokeUnobservedTaskException;
        public static event Action<Exception> UnobservedTaskException;

        internal static void PublishUnobservedTaskException(Exception ex)
        {
            if (ex != null)
            {
                if (!PropagateOperationCanceledException && ex is OperationCanceledException) return;

                if (UnobservedTaskException != null)
                {
                    if (Thread.CurrentThread.ManagedThreadId == PlayerLoopHelper.MainThreadId)
                        // allows inlining call.
                        UnobservedTaskException.Invoke(ex);
                    else
                        // Post to MainThread.
                        PlayerLoopHelper.UnitySynchronizationContext.Post(handleExceptionInvoke, ex);
                }
                else
                {
                    string msg = null;
                    if (UnobservedExceptionWriteLogType != LogType.Exception) msg = "UnobservedTaskException:" + ex;
                    switch (UnobservedExceptionWriteLogType)
                    {
                        case LogType.Error:
                            Debug.LogError(msg);
                            break;
                        case LogType.Assert:
                            Debug.LogAssertion(msg);
                            break;
                        case LogType.Warning:
                            Debug.LogWarning(msg);
                            break;
                        case LogType.Log:
                            Debug.Log(msg);
                            break;
                        case LogType.Exception:
                            Debug.LogException(ex);
                            break;
                    }
                }
            }
        }

        private static void InvokeUnobservedTaskException(object state)
        {
            UnobservedTaskException((Exception) state);
        }
    }
}

#endif
