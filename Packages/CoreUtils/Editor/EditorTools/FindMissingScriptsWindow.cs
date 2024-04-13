using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor {
    public class FindMissingScriptsWindow : EditorWindow {
        private const int kProgressBarThreshold = 300;

        private static bool s_Dirty;

        private static FindMissingScriptsWindow Instance {
            get {
                if (!s_Instance) {
                    s_Instance = GetWindow<FindMissingScriptsWindow>("Missing Scripts");
                }

                return s_Instance;
            }
        }
        private static FindMissingScriptsWindow s_Instance;

        [MenuItem("Tools/CoreUtils/Find Missing Scripts Window", false, (int)MenuOrder.Window)]
        public static void OpenWindow() {
            Instance.Show();
        }

        private enum Modes {
            Selection,
            Scene,
            Prefabs
        }

        [SerializeField] private List<RootObjectInfo> m_RootInfos;
        [SerializeField] private Vector2 m_Scroll;
        [SerializeField] private Modes m_Mode;
        [SerializeField] private Modes m_LastMode;
        private int m_SearchTotal;
        private int m_MissingTotal;

        private Modes Mode {
            get => m_Mode;
            set {
                if (m_Mode == value) {
                    return;
                }

                s_Dirty = true;
                m_LastMode = m_Mode;
                m_Mode = value;
            }
        }

        private void OnEnable() {
            EditorSelectionTracker.SelectionChanged += SelectionChanged;
        }

        private void OnDisable() {
            EditorSelectionTracker.SelectionChanged -= SelectionChanged;
        }

        private void Update() {
            if (s_Dirty || m_RootInfos == null) {
                RefreshSelectedInfos();
            }
        }

        private void SelectionChanged(Object selection) {
            if (Mode == Modes.Selection) {
                RefreshSelectedInfos();
            }
        }

        private void RefreshSelectedInfos() {
            GameObject[] gos = null;
            string[] paths = null;

            switch (Mode) {
                case Modes.Selection:
                    gos = new[] {Selection.activeGameObject};
                    break;
                case Modes.Scene:
                    gos = UnityUtils.GetRootObjects().ToArray();
                    break;
                case Modes.Prefabs:
                    if (EditorUtility.DisplayCancelableProgressBar("Finding missing scripts...", "Collecting all prefabs...", 0.05f)) {
                        Mode = m_LastMode;
                        return;
                    }

                    paths = AssetDatabase.FindAssets("t:Prefab").Select(AssetDatabase.GUIDToAssetPath).ToArray();
                    break;
                default:
                    throw new Exception("No matching mode " + Mode);
            }

            List<RootObjectInfo> rootInfos = new List<RootObjectInfo>();

            m_SearchTotal = 0;
            m_MissingTotal = 0;

            if (paths != null) {
                for (int i = 0; i < paths.Length; i++) {
                    if (FindMissingScriptsCancelable(AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]), rootInfos, i, paths.Length)) {
                        return;
                    }
                }
            } else {
                for (int i = 0; i < gos.Length; i++) {
                    if (FindMissingScriptsCancelable(gos[i], rootInfos, i, gos.Length)) {
                        return;
                    }
                }
            }

            m_RootInfos = rootInfos;

            EditorUtility.ClearProgressBar();
            s_Dirty = false;
            Repaint();
        }

        private bool FindMissingScriptsCancelable(GameObject go, List<RootObjectInfo> rootInfos, int index, int total) {
            RootObjectInfo rootInfo = new RootObjectInfo(go);
            m_SearchTotal += rootInfo.SearchTotal;
            m_MissingTotal += rootInfo.MissingTotal;

            if (rootInfo.HasMissingScripts) {
                rootInfos.Add(rootInfo);
            }

            if (total > kProgressBarThreshold && EditorUtility.DisplayCancelableProgressBar("Finding missing scripts...", "Searching " + go.name, index*1f/total)) {
                Mode = m_LastMode;
                return true;
            }

            return false;
        }

        private void OnGUI() {
            using (new GUILayout.HorizontalScope()) {
                using (new EditorGUI.DisabledScope(Mode == Modes.Selection)) {
                    if (ButtonToggle(Mode == Modes.Selection, "In Selection", EditorStyles.miniButtonLeft)) {
                        Mode = Modes.Selection;
                    }
                }

                using (new EditorGUI.DisabledScope(Mode == Modes.Scene)) {
                    if (ButtonToggle(Mode == Modes.Scene, "In Scene", EditorStyles.miniButtonMid)) {
                        Mode = Modes.Scene;
                    }
                }

                using (new EditorGUI.DisabledScope(Mode == Modes.Prefabs)) {
                    if (ButtonToggle(Mode == Modes.Prefabs, "In All Prefabs", EditorStyles.miniButtonRight)) {
                        Mode = Modes.Prefabs;
                    }
                }
            }

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(m_Scroll)) {
                m_Scroll = scrollView.scrollPosition;

                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (m_RootInfos != null && m_RootInfos.Count > 0) {
                        GUILayout.Label($"{m_MissingTotal:N0} missing references in {m_SearchTotal:N0} GameObjects", EditorStyles.boldLabel);
                    } else {
                        GUILayout.Label($"No missing references found in {m_SearchTotal:N0} GameObjects", EditorStyles.boldLabel);
                    }

                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();

                if (m_RootInfos != null) {
                    m_RootInfos.ForEach(si => si.OnGUI());
                }
            }

            if (GUILayout.Button("Clear All") && m_RootInfos != null) {
                m_RootInfos.ForEach(si => si.Clear());
            }
        }

        private static bool ButtonToggle(bool condition, string label, GUIStyle style) {
            bool newVal = GUILayout.Toggle(condition, label, style);
            return newVal != condition;
        }

        [Serializable]
        private class RootObjectInfo {
            [SerializeField] private GameObject m_GO;
            [SerializeField] private List<MissingScript> m_MissingScripts;

            public int MissingTotal {
                get {
                    if (m_MissingScripts != null) {
                        return m_MissingScripts.Count;
                    }

                    return 0;
                }
            }

            public bool HasMissingScripts => m_MissingScripts != null && m_MissingScripts.Any();

            public int SearchTotal { get; private set; }

            public RootObjectInfo() { } //  --- Required before deserialization

            public RootObjectInfo(GameObject go) {
                m_GO = go;
                m_MissingScripts = FindMissingIn(m_GO);
            }

            public void OnGUI() {
                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField("Missing Scripts found in ", GUILayout.Width(140));

                    using (new EditorGUI.DisabledScope(true)) {
                        Rect rect = EditorGUILayout.GetControlRect();
                        rect.xMax += 20;
                        EditorGUI.ObjectField(rect, m_GO, m_GO.GetType(), true);
                    }

                    if (GUILayout.Button("Clear Object", GUILayout.Width(100))) {
                        Clear();
                    }
                }

                using (new EditorGUI.IndentLevelScope(1)) {
                    if (m_MissingScripts != null) {
                        foreach (MissingScript missingScript in m_MissingScripts) {
                            missingScript.OnGUI();
                        }
                    }
                }
            }

            public void Clear() {
                if (m_MissingScripts != null) {
                    Enumerable.Reverse(m_MissingScripts).ForEach(ms => ms.Clear());
                }
            }

            private List<MissingScript> FindMissingIn(GameObject go, GameObject root = null) {
                root = root ? root : go;
                List<MissingScript> missingList = new List<MissingScript>();

                if (!go) {
                    return missingList;
                }

                SearchTotal++;

                Component[] components = go.GetComponents<Component>();

                for (int i = 0; i < components.Length; i++) {
                    Component component = components[i];

                    if (!component) {
                        missingList.Add(new MissingScript(go, i, root));
                    }
                }

                for (int i = 0; i < go.transform.childCount; i++) {
                    Transform child = go.transform.GetChild(i);
                    missingList.AddRange(FindMissingIn(child.gameObject, root));
                }

                return missingList;
            }
        }

        [Serializable]
        private class MissingScript {
            [SerializeField] private GameObject m_GO;
            [SerializeField] private int m_Index;
            [SerializeField] private string m_DisplayPath;

            public MissingScript() { } //  --- Required before deserialization

            public MissingScript(GameObject go, int index, Object root) {
                m_GO = go;
                m_Index = index;
                string rootPath = root.FullName(FullName.Parts.Name);
                string goPath = go.FullName(FullName.Parts.Name);
                string relativePath = goPath.Replace(rootPath, "");
                m_DisplayPath = $"{root.name}{relativePath} #{m_Index + 1}";
            }

            public void Clear() {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(m_GO);
                EditorUtility.SetDirty(m_GO);
                s_Dirty = true;
            }

            public void OnGUI() {
                using (new GUILayout.HorizontalScope()) {
                    using (new EditorGUI.DisabledScope(true)) {
                        EditorGUILayout.ObjectField(m_GO, m_GO.GetType(), true, GUILayout.Width(51));
                    }

                    // Don't want the assignment 'dot' from the above object field, so draw a rect to hide it!
                    Rect rect = EditorGUILayout.GetControlRect();
                    rect.xMin -= 15;
                    EditorGUI.DrawRect(rect, EditorUtils.BackgroundColor);
                    rect.xMin -= 20;
                    EditorGUI.LabelField(rect, m_DisplayPath);

                    if (GUILayout.Button("Clear", GUILayout.Width(100))) {
                        Clear();
                    }
                }
            }
        }
    }
}