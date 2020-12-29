using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    public class DragnetChange {
        private string m_Info;
        private bool m_ShowDetails;

        public string RootPath { get; }
        public string SourcePath { get; }
        public string DestinationPath { get; private set; }

        private readonly Dictionary<DragnetRule, string> m_ConvertResults = new Dictionary<DragnetRule, string>();

        public string FullSourcePath => Path.Combine(RootPath, SourcePath).Replace('\\', '/');
        public string FullDestinationPath => Path.Combine(RootPath, DestinationPath).Replace('\\', '/');

        public DragnetChange(string rootPath, string sourcePath) {
            RootPath = rootPath;
            SourcePath = sourcePath;
        }

        public bool ApplyRules(List<DragnetRule> rules) {
            string path = SourcePath;
            bool ruleFound = false;
            string info = "Source Path:  " + SourcePath;
            m_ConvertResults.Clear();
            int ruleNum = 1;
            foreach (DragnetRule rule in rules) {
                if (rule.OnlySearchChanges && !ruleFound && ruleNum != 1) {
                    ruleNum++;
                    continue;
                }

                string newPath = rule.Convert(path);

                if (path != newPath) {
                    // Even though the replacement is empty, still add the change to the rule.
                    rule.AddChange(this);

                    // Only count it as a match if the new path actually contains content.
                    if (!newPath.IsNullOrEmpty()) {
                        path = newPath;
                        ruleFound = true;
                        info += string.Format("\n Rule #{0} --> {1}", ruleNum, path);
                    }
                }

                m_ConvertResults[rule] = path;
                ruleNum++;
            }

            DestinationPath = path;
            m_Info = info;
            return DestinationPath != SourcePath;
        }

        public void Move(Func<string, string, string> moveAssetFunc) {
            if (File.Exists(FullDestinationPath)) {
                Debug.LogWarning("Cannot move " + SourcePath + " since " + DestinationPath + " already exists.");
                return;
            }

            string folder = Path.GetDirectoryName(FullDestinationPath);

            if (folder.IsNullOrEmpty() || !Directory.Exists(folder)) {
                Debug.LogWarning("Cannot move " + SourcePath + " since " + DestinationPath + "'s folder doesn't exist. (Should have already been created.)");
                return;
            }

            string error = moveAssetFunc(FullSourcePath, FullDestinationPath);

            if (!error.IsNullOrEmpty()) {
                Debug.LogError(error);
            }
        }

        public void OnChangeGUI(float width, DragnetRule rule = null, DragnetRule previousRule = null) {
            GUILayout.BeginHorizontal();

            float indentWidth = 15*EditorGUI.indentLevel;

            GUILayout.Space(indentWidth);

            width = width - indentWidth;

            string sourcePath = previousRule == null ? SourcePath : m_ConvertResults[previousRule];
            string designationPath = rule == null ? DestinationPath : m_ConvertResults[rule];

            if (GUILayout.Button(sourcePath, GUI.skin.label, GUILayout.Width(width/2)) ||
                GUILayout.Button(designationPath, GUI.skin.label, GUILayout.Width(width/2))) {
                m_ShowDetails = !m_ShowDetails;
            }

            GUILayout.EndHorizontal();

            if (m_ShowDetails) {
                OnDetailsGUI();
            }
        }

        public void OnDetailsGUI() {
            GUIContent content = new GUIContent(m_Info);
            Vector2 size = GUI.skin.label.CalcSize(content);
            Rect r = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(GUILayout.Height(size.y)));
            EditorGUI.SelectableLabel(r, m_Info, GUI.skin.label);
        }
    }
}