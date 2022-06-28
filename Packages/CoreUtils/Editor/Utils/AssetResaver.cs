using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public partial class AssetResaver : EditorWindow {
        [MenuItem("Tools/CoreUtils/Resave Assets...", priority = (int)MenuOrder.Window)]
        public static void OpenResaverWindow() {
            GetWindow<AssetResaver>().Show();
        }

        private static GUIContent[] s_ActionLabels;
        private static GUIContent[] ActionLabels => s_ActionLabels = s_ActionLabels ?? new[] { new GUIContent("Resave All"), new GUIContent("Resave Selection") };

        private int m_SelectedAction;
        private Resaver m_Resaver;

        protected void OnEnable() {
            titleContent = new GUIContent("Asset Resaver");
            minSize = new Vector2(200, 200);
        }

        protected void OnGUI() {
            using (new GUILayout.VerticalScope()) {
                bool selectionValid = Selection.objects != null && Selection.objects.Length > 0;
                if (m_Resaver == null) {
                    using (new EditorGUI.DisabledGroupScope(m_Resaver != null)) {
                        m_SelectedAction = GUILayout.SelectionGrid(m_SelectedAction, ActionLabels, 1, "Radio");
                        if (m_SelectedAction == 0) {
                            EditorGUILayout.HelpBox("Resaving everything will take a looooong time", MessageType.Warning);
                        } else {
                            if (selectionValid) {
                                EditorGUILayout.HelpBox(string.Format("{0} objects selected", Selection.objects.Length), MessageType.Info);
                            } else {
                                EditorGUILayout.HelpBox("Nothing Selected", MessageType.Warning);
                            }
                        }
                    }
                } else {
                    EditorGUILayout.HelpBox("Working, Please don't let Unity lose focus or the Inspector will stop updating", MessageType.Warning);
                }

                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope()) {
                    if (m_Resaver == null) {
                        GUILayout.FlexibleSpace();
                        using (new EditorGUI.DisabledGroupScope(m_SelectedAction == 1 && !selectionValid)) {
                            if (GUILayout.Button("Resave")) {
                                m_Resaver = new Resaver();
                                if (m_SelectedAction == 0) {
                                    m_Resaver.ResaveAll();
                                } else {
                                    m_Resaver.ResaveSelection();
                                }
                            }
                        }
                    } else {
                        GUIContent progressContent = new GUIContent(m_Resaver.ProgressMessage);
                        Rect rect = GUILayoutUtility.GetRect(progressContent, "ProgressBarBack", GUILayout.ExpandWidth(true));
                        EditorGUI.ProgressBar(rect, m_Resaver.Progress, m_Resaver.ProgressMessage);
                        if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false))) {
                            m_Resaver.Cancel();
                        }
                    }
                }
            }

            if (Event.current.type == EventType.Repaint) {
                if (m_Resaver != null) {
                    m_Resaver.Update();
                    if (m_Resaver.IsRunning) {
                        Repaint();
                        RepaintAllInspectors();
                    } else {
                        m_Resaver = null;
                    }
                }
            }
        }

        protected void OnSelectionChange() {
            Repaint();
        }

        private static MethodInfo s_RepaintInspectors;

        // Gross hack to call private method
        public static void RepaintAllInspectors() {
            if (s_RepaintInspectors == null) {
                Assembly editorAssembly = typeof(EditorApplication).Assembly;
                Type inspectorWindow = editorAssembly.GetType("UnityEditor.InspectorWindow");
                s_RepaintInspectors = inspectorWindow.GetMethod("RepaintAllInspectors",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
            s_RepaintInspectors.Invoke(null, null);
        }
    }
}
