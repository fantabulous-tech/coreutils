using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    [Serializable]
    public class DragnetRule {
        [UsedImplicitly] public string Name;
        [UsedImplicitly] public string SearchPattern;
        [UsedImplicitly] public string ReplacePattern;
        [UsedImplicitly] public bool OnlySearchChanges;
        [UsedImplicitly] public bool ShowDetails = true;
        [UsedImplicitly] public bool ShowChanges;

        private string m_Error;
        private Regex m_SearchRegex;
        private HashSet<DragnetChange> m_Changes;
        private int m_Index;
        private int m_Page;
        private DragnetChange[] m_PageItems;
        private const int kItemsPerPage = 100;

        public HashSet<DragnetChange> Changes => m_Changes ?? (m_Changes = new HashSet<DragnetChange>());

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

        private void UpdatePageItems() {
            int pageCount = Changes.Count%kItemsPerPage > 0 ? Changes.Count/kItemsPerPage : Changes.Count/kItemsPerPage - 1;
            m_Page = Mathf.Clamp(m_Page, 0, pageCount);
            m_PageItems = null;
        }

        private DragnetChange[] PageItems {
            get {
                if (m_PageItems != null && m_PageItems.Length > 0) {
                    return m_PageItems;
                }

                if (Changes.Count == 0 && m_PageItems == null) {
                    m_PageItems = new DragnetChange[0];
                } else if (Changes.Count > 0) {
                    int index = Mathf.Clamp(m_Page*kItemsPerPage, 0, Changes.Count - 1);
                    int count = Mathf.Clamp(Changes.Count - index + 1, 0, kItemsPerPage);
                    m_PageItems = Changes.Skip(index).Take(count).ToArray();
                }

                return m_PageItems;
            }
        }

        private Regex SearchRegex {
            get {
                if (m_SearchRegex == null) {
                    UpdateRegex();
                }
                return m_SearchRegex;
            }
        }

        public string Convert(string path) {
            return m_Error.IsNullOrEmpty() && SearchRegex != null ? SearchRegex.Replace(path, ReplacePattern ?? "") : "";
        }

        private void UpdateRegex() {
            m_Error = null;
            m_SearchRegex = null;

            if (SearchPattern.IsNullOrEmpty()) {
                m_Error = "No search pattern defined.";
                return;
            }

            try {
                m_SearchRegex = new Regex(SearchPattern, RegexOptions.IgnoreCase);
            }
            catch (Exception e) {
                m_Error = e.Message;
            }

            UpdatePageItems();
        }

        public void ClearChanges() {
            Changes.Clear();
            UpdatePageItems();
        }

        public void AddChange(DragnetChange change) {
            if (!Changes.Contains(change)) {
                Changes.Add(change);
                UpdatePageItems();
            }
        }

        public void RemoveChange(DragnetChange change) {
            if (change == null) {
                return;
            }

            Changes.Remove(change);
            UpdatePageItems();
        }

        public void OnRuleGUI(BaseDragnetConfig config, float scrollViewWidth, int index, DragnetWindow window, DragnetRule previousRule) {
            if (!window) {
                return;
            }

            m_Index = index;
            GUILayout.BeginHorizontal();

            ShowDetails = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), ShowDetails, ToString(), true, Changes.Count > 0 ? EditorStyles.foldout : DragnetUtils.GrayFoldout);

            if (GUILayout.Button(window.IconAdd, DragnetUtils.IconButton)) {
                config.AddRuleAt(index);
            }

            GUI.enabled = index > 0;
            if (GUILayout.Button(window.IconTop, DragnetUtils.IconButton)) {
                config.MoveRule(this, Direction.Top);
            }

            if (GUILayout.Button(window.IconUp, DragnetUtils.IconButton)) {
                config.MoveRule(this, Direction.Up);
            }

            GUI.enabled = index < config.Rules.Count - 1;
            if (GUILayout.Button(window.IconDown, DragnetUtils.IconButton)) {
                config.MoveRule(this, Direction.Down);
            }

            if (GUILayout.Button(window.IconBottom, DragnetUtils.IconButton)) {
                config.MoveRule(this, Direction.Bottom);
            }

            GUI.enabled = true;
            if (GUILayout.Button(window.IconDelete, DragnetUtils.IconButton)) {
                config.RemoveRule(this);
            }

            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            if (ShowDetails) {
                Name = EditorGUILayout.TextField("Name", Name);

                string lastSearch = config.RealtimePreview ? EditorGUILayout.TextField("Find Regex", SearchPattern) : EditorGUILayout.DelayedTextField("Find Regex", SearchPattern);

                if (SearchPattern != lastSearch) {
                    Undo.RecordObject(config, "Find Rule Change");
                    SearchPattern = lastSearch;
                    UpdateRegex();
                    config.RefreshChanges();
                }

                string lastReplace = config.RealtimePreview ? EditorGUILayout.TextField("Replace Regex", ReplacePattern) : EditorGUILayout.DelayedTextField("Replace Regex", ReplacePattern);
                if (ReplacePattern != lastReplace) {
                    Undo.RecordObject(config, "Replace Rule Change");
                    ReplacePattern = lastReplace;
                    config.UpdateRuleReplace(this);
                }

                if (index > 0) {
                    bool onlySearchChanges = EditorGUILayout.Toggle("Only Search Changes", OnlySearchChanges);

                    if (OnlySearchChanges != onlySearchChanges) {
                        Undo.RecordObject(config, "Only Search Changes Rule Change");
                        OnlySearchChanges = onlySearchChanges;
                        config.RefreshChanges();
                    }
                }
            }

            if (!m_Error.IsNullOrEmpty()) {
                GUILayout.Space(10);
                GUILayout.Label(m_Error, DragnetUtils.ErrorLabel);
            }

            if (ShowDetails && m_Error.IsNullOrEmpty() && Changes.Count > 0) {
                GUILayout.BeginHorizontal();
                string pageSummary = string.Format("Changes ({0}-{1}/{2})", Page*kItemsPerPage + 1, Page*kItemsPerPage + PageItems.Length, Changes.Count);
                ShowChanges = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), ShowChanges, pageSummary, true);

                if (GUILayout.Button("<", DragnetUtils.SmallIconButton)) {
                    Page--;
                }

                if (GUILayout.Button(">", DragnetUtils.SmallIconButton)) {
                    Page++;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                if (ShowChanges) {
                    PageItems.ForEach(p => p.OnChangeGUI(scrollViewWidth, this, previousRule));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;

            if (ShowDetails) {
                GUILayout.Space(10);
            }
        }

        public override string ToString() {
            string prefix = string.Format("Rule #{0}: {1}", m_Index + 1, Name.IsNullOrEmpty() ? SearchPattern + " > " + ReplacePattern : Name);
            return string.Format("{0} (x{1})", prefix, Changes == null ? 0 : Changes.Count);
        }

        public bool IsMatch(string path) {
            return SearchRegex != null && SearchRegex.IsMatch(path);
        }
    }
}