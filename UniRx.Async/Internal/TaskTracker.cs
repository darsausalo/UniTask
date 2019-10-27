using System.Diagnostics;
using System.Threading;
using UnityEditor;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;

namespace UniRx.Async.Internal
{
    // public for add user custom.

    public static class TaskTracker
    {
        private static List<KeyValuePair<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>> listPool =
            new List<KeyValuePair<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>>();

        private static readonly WeakDictionary<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)> tracking =
            new WeakDictionary<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>();

        private static bool dirty;

        [Conditional("UNITY_EDITOR")]
        public static void TrackActiveTask(IAwaiter task, int skipFrame = 1)
        {
#if UNITY_EDITOR
            dirty = true;
            if (!EditorEnableState.EnableTracking) return;
            var stackTrace = EditorEnableState.EnableStackTrace ? new StackTrace(skipFrame, true).CleanupAsyncStackTrace() : "";
            tracking.TryAdd(task, (Interlocked.Increment(ref trackingId), DateTime.UtcNow, stackTrace));
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void TrackActiveTask(IAwaiter task, string stackTrace)
        {
#if UNITY_EDITOR
            dirty = true;
            if (!EditorEnableState.EnableTracking) return;
            var success = tracking.TryAdd(task, (Interlocked.Increment(ref trackingId), DateTime.UtcNow, stackTrace));
#endif
        }

        public static string CaptureStackTrace(int skipFrame)
        {
#if UNITY_EDITOR
            if (!EditorEnableState.EnableTracking) return "";
            var stackTrace = EditorEnableState.EnableStackTrace ? new StackTrace(skipFrame + 1, true).CleanupAsyncStackTrace() : "";
            return stackTrace;
#else
            return null;
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void RemoveTracking(IAwaiter task)
        {
#if UNITY_EDITOR
            dirty = true;
            if (!EditorEnableState.EnableTracking) return;
            var success = tracking.TryRemove(task);
#endif
        }

        public static bool CheckAndResetDirty()
        {
            var current = dirty;
            dirty = false;
            return current;
        }

        /// <summary>(trackingId, awaiterType, awaiterStatus, createdTime, stackTrace)</summary>
        public static void ForEachActiveTask(Action<int, string, AwaiterStatus, DateTime, string> action)
        {
            lock (listPool)
            {
                var count = tracking.ToList(ref listPool, false);
                try
                {
                    for (var i = 0; i < count; i++)
                    {
                        string typeName = null;
                        var keyType = listPool[i].Key.GetType();
                        if (keyType.IsNested)
                            typeName = keyType.DeclaringType.Name + "." + keyType.Name;
                        else
                            typeName = keyType.Name;

                        action(listPool[i].Value.trackingId, typeName, listPool[i].Key.Status, listPool[i].Value.addTime, listPool[i].Value.stackTrace);
                        listPool[i] = new KeyValuePair<IAwaiter, (int trackingId, DateTime addTime, string stackTrace)>(null, (0, default, null)); // clear
                    }
                }
                catch
                {
                    listPool.Clear();
                    throw;
                }
            }
        }
#if UNITY_EDITOR

        private static int trackingId;

        public const string EnableAutoReloadKey = "UniTaskTrackerWindow_EnableAutoReloadKey";
        public const string EnableTrackingKey = "UniTaskTrackerWindow_EnableTrackingKey";
        public const string EnableStackTraceKey = "UniTaskTrackerWindow_EnableStackTraceKey";

        public static class EditorEnableState
        {
            private static bool enableAutoReload;

            private static bool enableTracking;

            private static bool enableStackTrace;

            public static bool EnableAutoReload
            {
                get => enableAutoReload;
                set
                {
                    enableAutoReload = value;
                    EditorPrefs.SetBool(EnableAutoReloadKey, value);
                }
            }

            public static bool EnableTracking
            {
                get => enableTracking;
                set
                {
                    enableTracking = value;
                    EditorPrefs.SetBool(EnableTrackingKey, value);
                }
            }

            public static bool EnableStackTrace
            {
                get => enableStackTrace;
                set
                {
                    enableStackTrace = value;
                    EditorPrefs.SetBool(EnableStackTraceKey, value);
                }
            }
        }

#endif
    }
}

#endif
