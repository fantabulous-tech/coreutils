using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    public abstract class BaseDragnetConfig : ScriptableObject {
        public string IncludePattern;
        public string ExcludePattern;
        public bool RealtimePreview = true;
        public bool ShowValidPaths;
        public List<DragnetRule> Rules = new List<DragnetRule>();
        public bool ShowChanges = true;

        public event Action<DragnetRule> AddRuleEvent;
        public event Action<DragnetRule> RemoveRuleEvent;
        public event Action<DragnetRule, Direction> MoveRuleEvent;

        private Vector2 m_ResultsScroll;
        private string[] m_AssetPaths;
        private HashSet<DragnetChange> m_AllChanges;
        protected string m_Error;
        private FolderPreview m_Root;
        private int m_Page;
        private string[] m_PageItems;
        private const int kItemsPerPage = 100;

        private static readonly Dictionary<string, bool> s_Expanded = new Dictionary<string, bool>();

        private string[] PageItems {
            get {
                if (m_PageItems != null && m_PageItems.Length > 0) {
                    return m_PageItems;
                }

                UpdatePageItems();
                return m_PageItems;
            }
        }

        private int Page {
            get => m_Page;
            set {
                if (m_Page == value) {
                    return;
                }

                m_Page = value;
                UpdatePageItems();
            }
        }

        private string[] AssetPaths {
            get {
                if (m_AssetPaths == null) {
                    RefreshAssetPaths();
                }
                return m_AssetPaths;
            }
        }

        protected abstract string RootPath { get; }

        protected HashSet<DragnetChange> AllChanges {
            get {
                CheckInit();
                return m_AllChanges;
            }
        }

        protected abstract string MoveAsset(string source, string destination);

        private FolderPreview Root {
            get {
                if (m_Root != null) {
                    return m_Root;
                }

                m_Root = new FolderPreview(Path.GetFileName(RootPath));

                foreach (DragnetChange change in AllChanges) {
                    string[] pieces = change.DestinationPath.Split('/', '\\');
                    FolderPreview folder = m_Root;

                    for (int i = 0; i < pieces.Length; i++) {
                        string piece = pieces[i];
                        bool isChange = i == pieces.Length - 1;
                        string key = isChange ? piece : "_" + piece;
                        if (!folder.Children.ContainsKey(key)) {
                            folder.Children[key] = new FolderPreview(piece, isChange ? change : null, folder.Path);
                        }
                        folder = folder.Children[key];
                    }
                }

                return m_Root;
            }
        }

        private void UpdatePageItems() {
            int pageCount = AssetPaths.Length%kItemsPerPage > 0 ? AssetPaths.Length/kItemsPerPage : AssetPaths.Length/kItemsPerPage - 1;
            m_Page = Mathf.Clamp(m_Page, 0, pageCount);
            m_PageItems = null;

            if (AssetPaths.Length == 0 && m_PageItems == null) {
                m_PageItems = new string[0];
            } else if (AssetPaths.Length > 0) {
                int index = Mathf.Clamp(m_Page*kItemsPerPage, 0, AssetPaths.Length - 1);
                int count = Mathf.Clamp(AssetPaths.Length - index + 1, 0, kItemsPerPage);
                m_PageItems = AssetPaths.Skip(index).Take(count).ToArray();
            }
        }

        private void CheckInit() {
            if (m_AllChanges == null || m_AssetPaths == null) {
                RefreshAssetPaths();
            }
        }

        protected abstract IEnumerable<string> GetUnfilteredAssetPaths();

        protected void RefreshAssetPaths() {
            m_AssetPaths = null;
            m_Error = null;

            IEnumerable<string> assetPaths = GetUnfilteredAssetPaths();

            if (!IncludePattern.IsNullOrEmpty()) {
                try {
                    Regex includeRegex = new Regex(IncludePattern, RegexOptions.IgnoreCase);
                    assetPaths = assetPaths.Where(p => includeRegex.IsMatch(p));
                }
                catch (Exception e) {
                    m_Error = e.Message;
                    return;
                }
            }

            if (!ExcludePattern.IsNullOrEmpty()) {
                try {
                    Regex excludeRegex = new Regex(ExcludePattern, RegexOptions.IgnoreCase);
                    assetPaths = assetPaths.Where(p => !excludeRegex.IsMatch(p));
                }
                catch (Exception e) {
                    m_Error = e.Message;
                    return;
                }
            }

            m_AssetPaths = assetPaths.ToArray();
            Array.Sort(m_AssetPaths, (p1, p2) => string.Compare(p1, p2, StringComparison.OrdinalIgnoreCase));
            UpdatePageItems();
            RefreshChanges();
        }

        public void RefreshChanges() {
            if (m_AllChanges == null) {
                m_AllChanges = new HashSet<DragnetChange>();
            } else {
                m_AllChanges.Clear();
            }

            Rules.ForEach(r => r.ClearChanges());

            foreach (string path in AssetPaths) {
                DragnetChange change = GetChange(RootPath, path);
                if (change == null) {
                    continue;
                }

                m_AllChanges.Add(change);
            }

            ResetRoot();
        }

        private void ResetRoot() {
            m_Root = null;
        }

        private DragnetChange GetChange(string root, string path) {
            DragnetChange change = Rules.SelectMany(r => r.Changes).FirstOrDefault(r => r.SourcePath == path);

            if (!Rules.Where(rule => !rule.OnlySearchChanges).Any(rule => rule.IsMatch(path))) {
                Rules.ForEach(r => r.RemoveChange(change));
                return null;
            }

            change = change ?? new DragnetChange(root, path);
            change.ApplyRules(Rules);
            return change;
        }

        public void UpdateRuleReplace(DragnetRule rule) {
            List<DragnetChange> changeToRemove = rule.Changes.ToList().Where(change => !change.ApplyRules(Rules)).ToList();
            changeToRemove.ForEach(rule.RemoveChange);
            ResetRoot();
        }

        public void OnConfigGUI(DragnetWindow window) {
            CheckInit();
            m_ResultsScroll = GUILayout.BeginScrollView(m_ResultsScroll);
            TopConfigGUI();
            ScrollViewGUI(window);
            GUILayout.EndScrollView();

            if (GUILayout.Button("Move All")) {
                MoveAll();
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(this);
            }
        }

        private void ScrollViewGUI(DragnetWindow window) {
            float scrollViewWidth = window.position.width - 28;
            string includePattern = EditorGUILayout.DelayedTextField("Include Regex", IncludePattern);
            if (IncludePattern != includePattern) {
                Undo.RecordObject(this, "Include Regex Change");
                IncludePattern = includePattern;
                RefreshAssetPaths();
            }

            string excludePattern = EditorGUILayout.DelayedTextField("Exclude Regex", ExcludePattern);
            if (ExcludePattern != excludePattern) {
                Undo.RecordObject(this, "Exclude Regex Change");
                ExcludePattern = excludePattern;
                RefreshAssetPaths();
            }

            if (!m_Error.IsNullOrEmpty()) {
                GUILayout.Space(10);
                GUILayout.Label(m_Error, DragnetUtils.ErrorLabel);
                return;
            }

            GUILayout.BeginHorizontal();
            string pageSummary = AssetPaths.Length == 0
                                     ? "Files (0)"
                                     : string.Format("Files ({0}-{1}/{2})", Page*kItemsPerPage + 1, Page*kItemsPerPage + PageItems.Length,
                                                     AssetPaths.Length);
            ShowValidPaths = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), ShowValidPaths, pageSummary, true);

            if (GUILayout.Button("<", DragnetUtils.SmallIconButton)) {
                Page--;
            }

            if (GUILayout.Button(">", DragnetUtils.SmallIconButton)) {
                Page++;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (ShowValidPaths) {
                EditorGUI.indentLevel++;
                PageItems.ForEach(s => {
                    EditorGUILayout.LabelField(s);
                });
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            GUILayout.Label(string.Format("Rules (x{0})", Rules.Count), DragnetUtils.BoldLabel);

            RealtimePreview = GUILayout.Toggle(RealtimePreview,
                                               new GUIContent("Realtime Preview",
                                                              "If checked, rule pattern changes will update as you type. Uncheck when there are lots of files to avoid slowness."));

            DragnetRule previousRule = null;
            for (int i = 0; i < Rules.Count; i++) {
                DragnetRule rule = Rules[i];
                rule.OnRuleGUI(this, scrollViewWidth, i, window, previousRule);
                previousRule = rule;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(window.IconAdd, DragnetUtils.IconButton)) {
                AddRule();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            ShowChanges = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), ShowChanges,
                                            string.Format("All Changes ({0})", AllChanges.Count), true, DragnetUtils.BoldFoldout);

            if (GUILayout.Button(new GUIContent(window.IconExpandAll, "Expand All"), DragnetUtils.SmallIconButton)) {
                m_Root.ExpandAll();
            }

            if (GUILayout.Button(new GUIContent(window.IconCollapseAll, "Collapse All"), DragnetUtils.SmallIconButton)) {
                m_Root.Children.Values.ForEach(c => c.CollapseAll());
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            if (ShowChanges) {
                EditorGUI.indentLevel++;
                Root.OnGUI();
                EditorGUI.indentLevel--;
            }
        }

        protected virtual void TopConfigGUI() { }

        private void MoveAll() {
            EditorUtility.DisplayCancelableProgressBar("Asset Dragnet", "Loading...", -1);

            // First create all missing folders.
            string[] folders = AllChanges.Select(c => Path.GetDirectoryName(c.FullDestinationPath)).Distinct().ToArray();

            EditorUtility.DisplayCancelableProgressBar("Asset Dragnet", "Creating directories...", -1);

            foreach (string folder in folders) {
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
            }

            PostFolderCreation();

            // Move all files into place.
            int i = 0;
            foreach (DragnetChange change in AllChanges) {
                change.Move(MoveAsset);

                if (EditorUtility.DisplayCancelableProgressBar("Asset Dragnet",
                                                               string.Format("Moving {0}", Path.GetFileName(change.DestinationPath)), i*1f/AllChanges.Count*0.95f)) {
                    EditorUtility.ClearProgressBar();
                    Debug.LogWarning($"Move cancelled. Only {i + 1}/{AllChanges.Count} files were moved.");
                    break;
                }

                i++;
            }

            // Delete empty folders.
            EditorUtility.DisplayCancelableProgressBar("Asset Dragnet", "Removing empty folders...", 0.95f);
            DragnetUtils.DeleteEmptyFolders(AllChanges, DeleteAsset);

            PostFileMove();

            EditorUtility.DisplayCancelableProgressBar("Asset Dragnet", "Refreshing...", 0.95f);
            RefreshAssetPaths();
            EditorUtility.ClearProgressBar();
        }

        protected virtual void PostFolderCreation() { }

        protected virtual void PostFileMove() { }

        private void AddRule() {
            EditorApplication.delayCall += () => {
                Undo.RecordObject(this, "Add Rule");
                DragnetRule rule = new DragnetRule();
                Rules.Add(rule);
                RefreshChanges();
                RaiseAddRuleEvent(rule);
            };
        }

        public void AddRuleAt(int index) {
            EditorApplication.delayCall += () => {
                Undo.RecordObject(this, "Insert Rule");
                DragnetRule rule = new DragnetRule();
                Rules.Insert(index, rule);
                RefreshChanges();
                RaiseAddRuleEvent(rule);
            };
        }

        public void RemoveRule(DragnetRule rule) {
            EditorApplication.delayCall += () => {
                Undo.RecordObject(this, "Remove Rule");
                Rules.Remove(rule);
                RefreshChanges();
                RaiseRemoveRuleEvent(rule);
            };
        }

        public void MoveRule(DragnetRule rule, Direction direction) {
            EditorApplication.delayCall += () => {
                Undo.RecordObject(this, "Move Rule");
                int index = Rules.IndexOf(rule);
                Rules.Remove(rule);

                switch (direction) {
                    case Direction.Top:
                        Rules.Insert(0, rule);
                        break;
                    case Direction.Up:
                        Rules.Insert(Mathf.Max(index - 1, 0), rule);
                        break;
                    case Direction.Down:
                        Rules.Insert(Mathf.Min(index + 1, Rules.Count), rule);
                        break;
                    case Direction.Bottom:
                        Rules.Add(rule);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("direction", direction, null);
                }

                RefreshChanges();
                RaiseMoveRuleEvent(rule, direction);
            };
        }

        private void RaiseAddRuleEvent(DragnetRule rule) {
            if (AddRuleEvent != null) {
                AddRuleEvent(rule);
            }
        }

        private void RaiseMoveRuleEvent(DragnetRule rule, Direction direction) {
            if (MoveRuleEvent != null) {
                MoveRuleEvent(rule, direction);
            }
        }

        private void RaiseRemoveRuleEvent(DragnetRule rule) {
            if (RemoveRuleEvent != null) {
                RemoveRuleEvent(rule);
            }
        }

        protected class FolderPreview {
            public readonly string Path;
            private readonly string m_Name;
            public readonly SortedDictionary<string, FolderPreview> Children = new SortedDictionary<string, FolderPreview>();
            private readonly DragnetChange m_Change;

            private bool IsChange => m_Change != null;

            public FolderPreview(string name, DragnetChange change = null, string parentPath = null) {
                Path = parentPath.IsNullOrEmpty() ? name : parentPath + "/" + name;
                m_Name = name;
                m_Change = change;
                if (!s_Expanded.ContainsKey(Path)) {
                    s_Expanded[Path] = !IsChange;
                }
            }

            public void OnGUI() {
                s_Expanded[Path] = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), s_Expanded[Path], m_Name, true,
                                                     IsChange ? DragnetUtils.BoldFoldout : DragnetUtils.GrayFoldout);

                if (!s_Expanded[Path]) {
                    return;
                }

                if (IsChange) {
                    m_Change.OnDetailsGUI();
                } else {
                    EditorGUI.indentLevel++;
                    foreach (FolderPreview child in Children.Values) {
                        child.OnGUI();
                    }
                    EditorGUI.indentLevel--;
                }
            }

            public void ExpandAll() {
                if (IsChange) {
                    return;
                }

                s_Expanded[Path] = true;
                foreach (FolderPreview folder in Children.Values) {
                    folder.ExpandAll();
                }
            }

            public void CollapseAll() {
                s_Expanded[Path] = false;
                foreach (FolderPreview folder in Children.Values) {
                    folder.CollapseAll();
                }
            }
        }

        protected abstract bool DeleteAsset(string path);
    }
}
