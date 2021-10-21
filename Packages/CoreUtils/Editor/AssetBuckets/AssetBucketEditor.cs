using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreUtils.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.AssetBuckets {
    [CustomEditor(typeof(BaseAssetBucket), true)]
    public class AssetBucketEditor : Editor<BaseAssetBucket> {
        private const int kIdWidth = 100;

        protected float m_ObjectColumnWidth;
        private readonly List<AssetListItem> m_Duplicates = new List<AssetListItem>();
        private AssetListItem[] m_SourceItems = { };
        private AssetListItem[] m_AssetItems = { };
        private int m_ShowAssets = -1;
        private string ShowAssetsKey => Target.name + "_ShowAssets";

        private float ObjectColumnWidth => m_ObjectColumnWidth > 0 ? m_ObjectColumnWidth : m_ObjectColumnWidth = GetLargestObjectFieldWidth();

        private bool ShowAssets {
            get => EditorUtils.GetEditorPrefBool(ref m_ShowAssets, ShowAssetsKey);
            set => EditorUtils.SetEditorPrefBool(ref m_ShowAssets, value, ShowAssetsKey);
        }

        private void OnEnable() {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Target.EDITOR_Updated += OnBucketUpdated;
            RefreshSourceItemList();
            OnBucketUpdated();
        }

        private void OnDisable() {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            if (Target) {
                Target.EDITOR_Updated -= OnBucketUpdated;
            }
        }

        public override void OnInspectorGUI() {
            DrawPropertiesExcluding(serializedObject, "m_Sources", "m_AssetRefs", "m_ManualUpdate");
            serializedObject.ApplyModifiedProperties();

            bool wasEnabled = GUI.enabled;
            if (CoreUtilsSettings.DisableAssetBucketScanning) {
                EditorGUILayout.HelpBox("Note: The Asset Bucket Watcher is disabled in Edit > Project Settings > CoreUtils, so automatic updating is disabled for all asset buckets.", MessageType.Info);
                GUI.enabled = false;
            } else {
                Target.ManualUpdate = EditorGUILayout.Toggle("Manual Update", Target.ManualUpdate);
            }

            GUI.enabled = wasEnabled;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Folder", EditorStyles.boldLabel, GUILayout.Width(ObjectColumnWidth));
            GUILayout.Label("Location", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            bool changed = m_SourceItems.Aggregate(false, (current, item) => current | item.OnSourceGUI(ObjectColumnWidth, Target));

            if (changed) {
                Undo.RecordObject(Target, "Change Bucket Source");
                AssetBucketWatcher.FindReferences(Target);
                OnSourceItemsUpdated();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"))) {
                Undo.RecordObject(Target, "Change Bucket Source");
                Target.EDITOR_Sources.Add(null);
                OnSourceItemsUpdated();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            wasEnabled = GUI.enabled;
            GUI.enabled = true;

            if (GUILayout.Button("Force Refresh")) {
                AssetBucketWatcher.FindReferences(Target);
                Repaint();
            }

            GUI.enabled = wasEnabled;

            if (m_Duplicates.Any()) {
                EditorGUILayout.HelpBox("Asset name collision found. Assets are loaded from buckets by name, so all names should be unique.", MessageType.Warning);
                GUILayout.BeginHorizontal();
                GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(kIdWidth));
                GUILayout.Label("Name Collision Asset", EditorStyles.boldLabel, GUILayout.Width(ObjectColumnWidth));
                GUILayout.Label("Location", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                m_Duplicates.ForEach(i => i.OnAssetGUI(ObjectColumnWidth));
            }

            EditorGUILayout.Space();

            ShowAssets = EditorGUILayout.Foldout(ShowAssets, $"Asset List (x{m_AssetItems.Length})");

            if (ShowAssets) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(kIdWidth));
                GUILayout.Label("Asset", EditorStyles.boldLabel, GUILayout.Width(ObjectColumnWidth));
                GUILayout.Label("Location", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                m_AssetItems.ForEach(OnAssetGUI);
            }
        }

        protected virtual void OnAssetGUI(AssetListItem item) => item.OnAssetGUI(ObjectColumnWidth);

        private void OnUndoRedoPerformed() => OnSourceItemsUpdated();

        private void OnBucketUpdated() {
            m_AssetItems = Target.EDITOR_Objects
                                 .Select((a, i) => new AssetListItem(a, Target.EDITOR_GetAssetName(a), Target.AssetType, i))
                                 .OrderBy(o => o.Name, StringComparer.OrdinalIgnoreCase)
                                 .ToArray();

            m_ObjectColumnWidth = -1;
            m_Duplicates.Clear();
            AssetListItem lastItem = null;

            foreach (AssetListItem item in m_AssetItems) {
                if (lastItem != null && item.Name.Equals(lastItem.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (!m_Duplicates.Contains(lastItem)) {
                        m_Duplicates.Add(lastItem);
                    }

                    m_Duplicates.Add(item);
                }

                lastItem = item;
            }

            Repaint();
        }

        private void OnSourceItemsUpdated() {
            Target.EDITOR_SourcesUpdated();
            EditorUtility.SetDirty(Target);
            RefreshSourceItemList();
            OnBucketUpdated();
        }

        private void RefreshSourceItemList() => m_SourceItems = Target.EDITOR_Sources?.Select((a, i) => new AssetListItem(a, a ? a.name : "", typeof(Object), i)).ToArray() ?? new AssetListItem[0];

        private float GetLargestObjectFieldWidth() {
            float assetItemMax = m_AssetItems.Length > 0 ? m_AssetItems.Max(i => i.GetObjectFieldWidth()) : 0;
            float sourceItemMax = m_SourceItems.Length > 0 ? m_SourceItems.Max(i => i.GetObjectFieldWidth()) : 0;
            return Mathf.Max(assetItemMax, sourceItemMax);
        }

        protected class AssetListItem {
            private readonly Object m_Item;
            private readonly Type m_Type;
            private readonly int m_Index;
            private readonly string m_Name;
            // private float m_ObjectFieldWidth;

            public string Name => !m_Name.IsNullOrEmpty() ? m_Name : m_Item ? m_Item.name : $"None ({m_Type.Name})";
            public string DisplayPath { get; }

            public AssetListItem(Object item, string name, Type type, int index) {
                m_Item = item;
                m_Name = name;
                m_Type = type;
                m_Index = index;
                string path = AssetDatabase.GetAssetPath(item);
                int lastFolderIndex = path?.LastIndexOf('/') ?? -1;
                path = path != null && lastFolderIndex >= 0
                           ? path.Substring(0, path.LastIndexOf('/')).ReplaceRegex("^Assets/", "")
                           : path;
                DisplayPath = path ?? " <null> ";
            }

            public bool OnSourceGUI(float objectFieldWidth, BaseAssetBucket bucket) {
                GUILayout.BeginHorizontal();

                Object newItem = EditorGUILayout.ObjectField(m_Item, m_Type, false, GUILayout.Width(objectFieldWidth));
                bool changed = false;

                if (newItem != m_Item && newItem is DefaultAsset && Directory.Exists(AssetDatabase.GetAssetPath(newItem))) {
                    Undo.RecordObject(bucket, "Assign bucket source");
                    bucket.EDITOR_Sources[m_Index] = newItem;
                    EditorUtility.SetDirty(bucket);
                    changed = true;
                }

                GUILayout.Label(DisplayPath);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Height(15))) {
                    Undo.RecordObject(bucket, "Remove bucket source");
                    bucket.EDITOR_Sources.Remove(m_Item);
                    changed = true;
                }

                GUILayout.EndHorizontal();
                return changed;
            }

            public void OnAssetGUI(float objectFieldWidth) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(m_Name, GUILayout.Width(kIdWidth));
                EditorGUILayout.ObjectField(m_Item, m_Type, false, GUILayout.Width(objectFieldWidth));
                GUILayout.Label(DisplayPath);
                GUILayout.EndHorizontal();
            }

            public float GetObjectFieldWidth() {
                string name = m_Item == null || m_Item is GameObject ? m_Name : $"{m_Name} ({m_Item.GetType().Name})";
                Vector2 size = EditorStyles.objectField.CalcSize(new GUIContent(name));
                return Mathf.Max(size.x, 150) + 15;
            }
        }
    }
}
