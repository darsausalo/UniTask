﻿using System;
using UniRx.Async.Internal;
using UnityEngine;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Linq;
using UnityEditor;

namespace UniRx.Async.Editor
{
    public class UniTaskTrackerWindow : EditorWindow
    {
        private static int interval;

        private static UniTaskTrackerWindow window;

        private static readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];
        private object splitterState;

        private UniTaskTrackerTreeView treeView;

        [MenuItem("Window/UniRx/UniTask Tracker")]
        public static void OpenWindow()
        {
            if (window != null) window.Close();

            // will called OnEnable(singleton instance will be set).
            GetWindow<UniTaskTrackerWindow>("UniTask Tracker").Show();
        }

        private void OnEnable()
        {
            window = this; // set singleton.
            splitterState = SplitterGUILayout.CreateSplitterState(new[] {75f, 25f}, new[] {32, 32}, null);
            treeView = new UniTaskTrackerTreeView();
            TaskTracker.EditorEnableState.EnableAutoReload = EditorPrefs.GetBool(TaskTracker.EnableAutoReloadKey, false);
            TaskTracker.EditorEnableState.EnableTracking = EditorPrefs.GetBool(TaskTracker.EnableTrackingKey, false);
            TaskTracker.EditorEnableState.EnableStackTrace = EditorPrefs.GetBool(TaskTracker.EnableStackTraceKey, false);
        }

        private void OnGUI()
        {
            // Head
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(splitterState, EmptyLayoutOption);
            {
                // Column Tabble
                RenderTable();

                // StackTrace details
                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        #region HeadPanel

        public static bool EnableAutoReload => TaskTracker.EditorEnableState.EnableAutoReload;
        public static bool EnableTracking => TaskTracker.EditorEnableState.EnableTracking;
        public static bool EnableStackTrace => TaskTracker.EditorEnableState.EnableStackTrace;
        private static readonly GUIContent EnableAutoReloadHeadContent = EditorGUIUtility.TrTextContent("Enable AutoReload", "Reload automatically.");
        private static readonly GUIContent ReloadHeadContent = EditorGUIUtility.TrTextContent("Reload", "Reload View.");
        private static readonly GUIContent GCHeadContent = EditorGUIUtility.TrTextContent("GC.Collect", "Invoke GC.Collect.");

        private static readonly GUIContent EnableTrackingHeadContent =
            EditorGUIUtility.TrTextContent("Enable Tracking", "Start to track async/await UniTask. Performance impact: low");

        private static readonly GUIContent EnableStackTraceHeadContent =
            EditorGUIUtility.TrTextContent("Enable StackTrace", "Capture StackTrace when task is started. Performance impact: high");

        // [Enable Tracking] | [Enable StackTrace]
        private void RenderHeadPanel()
        {
            EditorGUILayout.BeginVertical(EmptyLayoutOption);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, EmptyLayoutOption);

            if (GUILayout.Toggle(EnableAutoReload, EnableAutoReloadHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption) != EnableAutoReload)
                TaskTracker.EditorEnableState.EnableAutoReload = !EnableAutoReload;

            if (GUILayout.Toggle(EnableTracking, EnableTrackingHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption) != EnableTracking)
                TaskTracker.EditorEnableState.EnableTracking = !EnableTracking;

            if (GUILayout.Toggle(EnableStackTrace, EnableStackTraceHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption) != EnableStackTrace)
                TaskTracker.EditorEnableState.EnableStackTrace = !EnableStackTrace;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(ReloadHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption))
            {
                TaskTracker.CheckAndResetDirty();
                treeView.ReloadAndSort();
                Repaint();
            }

            if (GUILayout.Button(GCHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption)) GC.Collect(0);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region TableColumn

        private Vector2 tableScroll;
        private GUIStyle tableListStyle;

        private void RenderTable()
        {
            if (tableListStyle == null)
            {
                tableListStyle = new GUIStyle("CN Box");
                tableListStyle.margin.top = 0;
                tableListStyle.padding.left = 3;
            }

            EditorGUILayout.BeginVertical(tableListStyle, EmptyLayoutOption);

            tableScroll = EditorGUILayout.BeginScrollView(tableScroll, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(2000f));
            var controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));


            treeView?.OnGUI(controlRect);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void Update()
        {
            if (EnableAutoReload)
                if (interval++ % 120 == 0)
                    if (TaskTracker.CheckAndResetDirty())
                    {
                        treeView.ReloadAndSort();
                        Repaint();
                    }
        }

        #endregion

        #region Details

        private static GUIStyle detailsStyle;
        private Vector2 detailsScroll;

        private void RenderDetailsPanel()
        {
            if (detailsStyle == null)
            {
                detailsStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                detailsStyle.wordWrap = false;
                detailsStyle.stretchHeight = true;
                detailsStyle.margin.right = 15;
            }

            var message = "";
            var selected = treeView.state.selectedIDs;
            if (selected.Count > 0)
            {
                var first = selected[0];
                var item = treeView.CurrentBindingItems.FirstOrDefault(x => x.id == first) as UniTaskTrackerViewItem;
                if (item != null) message = item.Position;
            }

            detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll, EmptyLayoutOption);
            var vector = detailsStyle.CalcSize(new GUIContent(message));
            EditorGUILayout.SelectableLabel(message, detailsStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MinWidth(vector.x),
                GUILayout.MinHeight(vector.y));
            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}

#endif
