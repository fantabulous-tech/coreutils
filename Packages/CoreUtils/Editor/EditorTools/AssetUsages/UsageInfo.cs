using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor.AssetUsages {
    [Serializable]
    public class UsageInfo {
        [SerializeField] private Object[] m_Targets;
        [SerializeField] private Guid[] m_Guids;
        [SerializeField] private string[] m_Paths;
        [SerializeField] private List<FileEntry> m_TargetFiles;
        [SerializeField] private List<FileEntry> m_Using;
        [SerializeField] private List<FileEntry> m_UsedBy;
        [SerializeField] private List<GameObject> m_UsedByInScene;
        [SerializeField] private string m_Summary;
        [SerializeField] private Vector2 m_UsesScroll = Vector2.zero;
        [SerializeField] private Vector2 m_UsedByScroll = Vector2.zero;

        public Object[] Targets => m_Targets;
        public Guid[] Guids => m_Guids;
        public List<FileEntry> TargetFiles => UnityUtils.GetOrSet(ref m_TargetFiles, () => GuidDataService.LoadFiles(m_Guids));
        public List<FileEntry> Using => UnityUtils.GetOrSet(ref m_Using, () => GuidDataService.LoadUsing(m_Guids));
        public List<FileEntry> UsedBy => UnityUtils.GetOrSet(ref m_UsedBy, () => GuidDataService.LoadUsedBy(m_Guids));
        public List<GameObject> UsedByInScene => UnityUtils.GetOrSet(ref m_UsedByInScene, GetUsedByInScene);

        private string Summary => UnityUtils.GetOrSet(ref m_Summary, () => m_Targets.Select(o => o ? o.name : "--null--").AggregateToString());

        public UsageInfo(Object obj) : this(new [] { obj }) { }

        public UsageInfo(Object[] targets) {
            m_Targets = targets;
            m_Guids = GetGUIDs(m_Targets).ToArray();
            m_Paths = m_Targets.Select(AssetDatabase.GetAssetPath).ToArray();
        }

        public void OnGUI() {
            if (m_Targets == null || m_Targets.Length == 0) {
                return;
            }

            float width = AssetUsageWindow.Instance.position.width - 10;
            int columns = Mathf.Clamp((int)(width/100), 1, m_Targets.Length);
            float columnWidth = (width + 8)/columns;

            GUILayout.Label(m_Guids.Length != 1 ? "Selected Assets x" + m_Guids.Length.ToString("N0") : "Selected Asset", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            for (int i = 0; i < Targets.Length; i++) {
                Object target = Targets[i];
                Object obj = EditorGUILayout.ObjectField(target, typeof(Object), false, GUILayout.Width(columnWidth - 4));

                if (obj != target) {
                    Object[] copy = Targets.ToArray();
                    copy[i] = obj;
                    Selection.objects = copy;
                }

                if (i%columns == columns - 1) {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.EndHorizontal();

            if (m_Guids.Length == 0) {
                GUILayout.Label("Nothing selected.", EditorStyles.boldLabel);
                return;
            }

            if (m_Guids.Length > 997) {
                GUILayout.Label("Too many file IDs selected. (" + m_Guids.Length.ToString("N0") + "/900 max)", EditorStyles.boldLabel);
                return;
            }

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(width/2));

            GUILayout.Label("Referenced by " + UsedBy.Count.ToString("N0") + (UsedBy.Count != 1 ? " assets:" : " asset:"));
            m_UsedByScroll = GUILayout.BeginScrollView(m_UsedByScroll, GUIStyle.none);
            UsedBy.ForEach(OnAssetButtonGUI);

            CleanUsedByInScene();

            if (UsedByInScene.Count > 0) {
                EditorGUILayout.Space();
                GUILayout.Label("References in Hierarchy:");
                UsedByInScene.ForEach(OnHierarchyObjectButtonGUI);
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Select all")) {
                Object[] selection = new Object[UsedBy.Count];

                for (int index = 0; index < selection.Length; index++) {
                    selection[index] = AssetDatabase.LoadAssetAtPath<Object>(UsedBy[index].Path);
                }

                Selection.objects = selection;
                EditorGUIUtility.PingObject(Selection.activeObject);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(width/2));
            GUILayout.Label("Referencing " + Using.Count.ToString("N0") + (Using.Count != 1 ? " assets:" : " asset:"));
            m_UsesScroll = GUILayout.BeginScrollView(m_UsesScroll, GUIStyle.none);
            Using.ForEach(OnAssetButtonGUI);
            GUILayout.EndScrollView();

            if (GUILayout.Button("Select all")) {
                Object[] selection = new Object[Using.Count];

                for (int index = 0; index < selection.Length; index++) {
                    selection[index] = AssetDatabase.LoadAssetAtPath<Object>(Using[index].Path);
                }

                Selection.objects = selection;
                EditorGUIUtility.PingObject(Selection.activeObject);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void CleanUsedByInScene() {
            if (UsedByInScene.Count > 0) {
                for (int i = UsedByInScene.Count - 1; i >= 0; i--) {
                    if (!UsedByInScene[i]) {
                        UsedByInScene.RemoveAt(i);
                    }
                }
            }
        }

        private static void OnAssetButtonGUI(FileEntry file) {
            GUI.enabled = file.Exists;

            if (GUILayout.Button(file.DisplayPath, CustomEditorStyles.ButtonLeft)) {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(file.Path);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }

            GUI.enabled = true;
        }

        private static void OnHierarchyObjectButtonGUI(Object sceneObject) {
            if (GUILayout.Button(sceneObject.FullName(FullName.Parts.FullScenePath), CustomEditorStyles.ButtonLeft)) {
                Selection.activeObject = sceneObject;
                EditorGUIUtility.PingObject(sceneObject);
            }
        }

        private static IEnumerable<Guid> GetGUIDs(IEnumerable<Object> targets) {
            HashSet<Guid> guids = new HashSet<Guid>();

            foreach (Object target in targets) {
                if (!target) {
                    continue;
                }

                DefaultAsset folder = target as DefaultAsset;

                if (folder) {
                    string folderPath = AssetDatabase.GetAssetPath(folder);

                    if (Directory.Exists(folderPath)) {
                        string[] searchGuids = AssetDatabase.FindAssets("t:object", new[] {folderPath});
                        searchGuids.ForEach(f => guids.Add(new Guid(f)));
                        continue;
                    }
                }

                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out string guid, out long _)) {
                    guids.Add(new Guid(guid));
                }
            }

            return guids;
        }

        private List<GameObject> GetUsedByInScene() {
            List<GameObject> usedByInScene = new List<GameObject>();
            return usedByInScene;

            // GameObject[] objects = Object.FindObjectsOfType(typeof(GameObject)).Cast<GameObject>().ToArray();
            //
            // foreach (GameObject go in objects) {
            // 	if (usedByInScene.Contains(go)) {
            // 		continue;
            // 	}
            //
            // 	bool isRoot = PrefabUtility.IsAnyPrefabInstanceRoot(go);
            //
            // 	if (isRoot && m_Paths.Contains(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go))) {
            // 		usedByInScene.Add(go);
            // 		continue;
            // 	}
            //
            // 	Component[] components = go.GetComponents<Component>();
            // 	if (components.Any(c => Targets.Any(t => IsReferencedBy(t, c)))) {
            // 		usedByInScene.Add(go);
            // 	}
            // }
            //
            // return usedByInScene.OrderBy(o => o.FullName(FullName.Parts.FullScenePath)).ToList();
        }

        private static bool IsReferencedBy(Object obj, Component component) {
            if (!component || !obj) {
                return false;
            }

            SerializedObject so = new SerializedObject(component);
            SerializedProperty sp = so.GetIterator();

            while (sp.NextVisible(true)) {
                if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue == obj) {
                    return true;
                }
            }

            return false;
        }

        public override string ToString() {
            return Summary;
        }
    }
}