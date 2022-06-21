using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreUtils.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.AssetBuckets {
    [CustomEditor(typeof(BaseAssetReferenceBucket), true)]
    public class AssetRefBucketEditor : Editor<BaseAssetReferenceBucket> {
        private const int kMinColumnWidth = 100;

        private float m_IDColumnWidth;
        private float m_EditColumnWidth;
        private float m_SourceObjectColumnWidth;
        private readonly List<AssetRefListItem> m_Duplicates = new List<AssetRefListItem>();
        private AssetSourceListItem[] m_SourceItems = { };
        private AssetRefListItem[] m_AssetItems = { };
        private int m_ShowAssets = -1;
        private Vector2 m_SourceScrollPos;
        private Vector2 m_AssetScrollPos;

        private string ShowAssetsKey => Target.name + "_ShowAssets";

        private float SourceObjectColumnWidth => GetOrSetFloat(ref m_SourceObjectColumnWidth, () => GetFieldWidth(m_SourceItems, i => i.GetFieldWidth()));
        private float IDColumnWidth => GetOrSetFloat(ref m_IDColumnWidth, () => GetFieldWidth(m_AssetItems, i => i.GetFieldWidth()));

        private float EditColumnWidth => GetOrSetFloat(ref m_EditColumnWidth, () => EditorStyles.boldLabel.CalcSize(new GUIContent("Edit")).x);

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
            DrawPropertiesExcluding(serializedObject, "m_Sources", "m_AssetRefs");
            serializedObject.ApplyModifiedProperties();

            bool wasEnabled = GUI.enabled;
            if (CoreUtilsSettings.DisableAssetBucketScanning) {
                EditorGUILayout.HelpBox("Note: The Asset Bucket Watcher is disabled in Edit > Project Settings > CoreUtils, so automatic updating is disabled for all asset buckets.", MessageType.Info);
                GUI.enabled = false;
            } else {
                Target.ManualUpdate = EditorGUILayout.Toggle("Manual Update", Target.ManualUpdate);
            }

            GUI.enabled = wasEnabled;

            m_SourceScrollPos = EditorGUILayout.BeginScrollView(m_SourceScrollPos);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Folder", EditorStyles.boldLabel, GUILayout.Width(SourceObjectColumnWidth));
            GUILayout.Label("Location", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            bool changed = m_SourceItems.Aggregate(false, (current, item) => current | item.OnSourceGUI(SourceObjectColumnWidth, Target));

            EditorGUILayout.EndScrollView();

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

            if (GUILayout.Button(new GUIContent("Force Refresh", "Clear the bucket and recheck every single file in the above folders to see if they can be added."))) {
                if (!AssetBucketWatcher.FindReferences(Target, forceRefresh: true)) {
                    Debug.Log($"<color=#6699cc>AssetBuckets</color>: No updates needed for {Target.name}", Target);
                } else {
                    Repaint();
                }
            }

            GUI.enabled = wasEnabled;

            if (m_Duplicates.Any()) {
                EditorGUILayout.HelpBox("Asset name collision found. Assets are loaded from buckets by name, so all names should be unique.", MessageType.Warning);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Edit", EditorStyles.boldLabel, GUILayout.Width(EditColumnWidth));
                GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(IDColumnWidth));
                GUILayout.Label("Location", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                m_Duplicates.ForEach(OnAssetGUI);
            }

            EditorGUILayout.Space();

            ShowAssets = EditorGUILayout.Foldout(ShowAssets, $"Asset List (x{m_AssetItems.Length})");

            m_AssetScrollPos = EditorGUILayout.BeginScrollView(m_AssetScrollPos);

            if (ShowAssets) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Edit", EditorStyles.boldLabel, GUILayout.Width(EditColumnWidth));
                GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(IDColumnWidth));
                GUILayout.Label("Location", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                m_AssetItems.ForEach(OnAssetGUI);
            }

            EditorGUILayout.EndScrollView();
        }

        protected virtual void OnAssetGUI(AssetRefListItem item) => item.OnAssetGUI(IDColumnWidth, EditColumnWidth);

        private void OnUndoRedoPerformed() => OnSourceItemsUpdated();

        private void OnBucketUpdated() {
            m_AssetItems = Target.AssetRefs
                .Select(a => new AssetRefListItem(a, Target.AssetType))
                .OrderBy(o => o.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            m_Duplicates.Clear();
            AssetRefListItem lastItem = null;

            foreach (AssetRefListItem item in m_AssetItems) {
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

        private void RefreshSourceItemList() => m_SourceItems = Target.EDITOR_Sources?.Select((a, i) => new AssetSourceListItem(a, a ? a.name : "", typeof(Object), i)).ToArray() ?? new AssetSourceListItem[0];

        private static float GetFieldWidth(IReadOnlyCollection<IAssetListItem> items, Func<IAssetListItem, float> getItemWidth) {
            return Mathf.Max(items.Count > 0 ? items.Max(getItemWidth) : kMinColumnWidth, kMinColumnWidth);
        }

        private static float GetOrSetFloat(ref float floatRef, Func<float> getFloat) {
            if (floatRef <= 0) {
                floatRef = getFloat();
            }

            return floatRef;
        }

        protected interface IAssetListItem {

            float GetFieldWidth();
        }

        protected class AssetSourceListItem : IAssetListItem {
            private readonly Object m_Item;
            private readonly Type m_Type;
            private readonly int m_Index;
            private readonly string m_Name;

            public string Name => !m_Name.IsNullOrEmpty() ? m_Name : m_Item ? m_Item.name : $"None ({m_Type.Name})";
            public string DisplayPath { get; }

            public AssetSourceListItem(Object item, string name, Type type, int index) {
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

            public bool OnSourceGUI(float objectFieldWidth, BaseAssetReferenceBucket bucket) {
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

            public float GetFieldWidth() {
                string name = m_Item == null || m_Item is GameObject ? m_Name : $"{m_Name} ({m_Item.GetType().Name})";
                Vector2 size = EditorStyles.objectField.CalcSize(new GUIContent(name));
                const int kObjectFieldAdjustment = -35;
                return Mathf.Max(size.x + kObjectFieldAdjustment, kMinColumnWidth);
            }
        }

        protected class AssetRefListItem : IAssetListItem {
            private readonly LazyAssetReference m_Item;
            private readonly Type m_Type;

            public string Name => m_Item != null ? m_Item.Name : $"None ({m_Type.Name})";
            public string DisplayPath { get; }

            public AssetRefListItem(LazyAssetReference item, Type type) {
                m_Item = item;
                m_Type = type;
                string path = AssetDatabase.GUIDToAssetPath(item.Guid);
                int lastFolderIndex = path?.LastIndexOf('/') ?? -1;
                path = path != null && lastFolderIndex >= 0
                    ? path.Substring(0, path.LastIndexOf('/')).ReplaceRegex("^Assets/", "")
                    : path;
                DisplayPath = path ?? " <null> ";
            }

            public void OnAssetGUI(float idFieldWidth, float editFieldWidth) {
                GUILayout.BeginHorizontal();

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.padding = new RectOffset(2, 2, 2, 2);
				// icon size is 16x16 so with padding size is 20x20
                if (GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml", "Open Asset"), style, GUILayout.Height(20), GUILayout.Width(20))) {
                    Selection.activeObject = m_Item.AssetRef.asset;
                }

                GUILayout.Label(Name, GUILayout.Width(idFieldWidth));              
                
                GUILayout.Label(DisplayPath);
                GUILayout.EndHorizontal();
            }

            public float GetFieldWidth() {
                return EditorStyles.boldLabel.CalcSize(new GUIContent(Name)).x;
            }
        }
    }
}
