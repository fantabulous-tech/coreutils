using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetUsages {
    public class AssetUsageWindow : EditorWindow {
        private const string kWindowName = "Asset Usages";

        private static AssetUsageWindow s_Instance;
        private static UsageInfo s_LastInfo;

        public static AssetUsageWindow Instance => UnityUtils.GetOrSet(ref s_Instance, () => GetWindow<AssetUsageWindow>(kWindowName));

        [MenuItem("Assets/Find All Uses in Project", true, (int)MenuOrder.Usages)]
        private static bool OpenEnabled() => !CoreUtilsSettings.DisableAssetGuidDatabase && Selection.objects.Length > 0;

        [MenuItem("Assets/Find All Uses in Project", false, (int)MenuOrder.Usages)]
        private static void Open() {
            Instance.Show();
            Instance.SelectionChanged(Selection.objects);
        }

        [SerializeField] private Object[] m_SelectedObjects;
        [SerializeField] private bool m_LockSelection;
        [SerializeField] private List<Object[]> m_GoBackStack = new List<Object[]>();
        [SerializeField] private List<Object[]> m_GoForwardStack = new List<Object[]>();

        private void Reselect(AssetChanges changes) {
            s_LastInfo = null;
            SelectionChanged(m_SelectedObjects, true);
        }

        [MenuItem("Tools/CoreUtils/Asset Usages Window", false, (int)MenuOrder.Window)]
        public static void OpenWindow() => Instance.Show();

        private static UsageInfo GetUsageInfo(Object[] objects) => objects != null && objects.Length > 0 ? new UsageInfo(objects) : null;

        private void OnEnable() {
            GuidDataService.Init();
            GuidDataService.Updated += Reselect;
            EditorSelectionTracker.SelectedObjectsChanged += SelectionChanged;
        }

        private void OnDisable() {
            GuidDataService.Updated -= Reselect;
            EditorSelectionTracker.SelectedObjectsChanged -= SelectionChanged;
        }

        private void OnGUI() {
            if (CoreUtilsSettings.DisableAssetGuidDatabase) {
                EditorGUILayout.HelpBox("Asset Usages file scanning is disabled in Edit > Project Settings > CoreUtils. Usages may not be accurate. Please use 'Update ALL Files' to manually refresh the database if files have changed.", MessageType.Warning);
            } else {
                EditorGUILayout.HelpBox("Asset Usages file scanning is enabled in Edit > Project Settings > CoreUtils. If this was recently enabled, usages may not be accurate. Please use 'Update ALL Files' to manually refresh the database if this setting was recently enabled.", MessageType.Info);
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = m_GoBackStack.Any();
            if (GUILayout.Button("<", GUILayout.Width(20))) {
                GoBack();
                EditorGUIUtility.PingObject(Selection.activeObject);
            }

            GUI.enabled = m_GoForwardStack.Any();
            if (GUILayout.Button(">", GUILayout.Width(20))) {
                GoForward();
                EditorGUIUtility.PingObject(Selection.activeObject);
            }

            GUI.enabled = true;

            m_LockSelection = GUILayout.Toggle(m_LockSelection, "Lock Current Selection");

            GUILayout.FlexibleSpace();

            if (s_LastInfo != null && s_LastInfo.Targets.Length == 1 && GUILayout.Button("Find in Hierarchy")) {
                SetSearchFilter(s_LastInfo.Targets[0]);
            }

            if (GUILayout.Button("Update ALL Files", GUILayout.Width(120))) {
                EditorApplication.delayCall += GuidDataService.Refresh;
            }

            GUILayout.EndHorizontal();

            s_LastInfo?.OnGUI();
        }

        private void SelectionChanged(Object[] selection) => SelectionChanged(selection, false);

        private void SelectionChanged(Object[] selection, bool force) {
            Object[] oldSelection = m_SelectedObjects;

            // We don't need to update if the selection is null.
            if (selection == null || selection.Length == 0) {
                return;
            }

            Object[] projectAssets = selection.Select(GetRootObject).Where(o => o).Distinct().ToArray();

            if (projectAssets.Length == 0) {
                return;
            }

            if (!m_LockSelection || m_SelectedObjects == null || m_SelectedObjects.Length == 0) {
                m_SelectedObjects = projectAssets;
            }

            // If we haven't changed selections, skip it.
            if (!force && s_LastInfo != null && s_LastInfo.Targets.IsEqual(m_SelectedObjects)) {
                return;
            }

            // Only update the flow stack if the selection has changed.
            if (!oldSelection.IsEqual(m_SelectedObjects)) {
                Object[] prev = m_GoBackStack.LastOrDefault();
                Object[] next = m_GoForwardStack.LastOrDefault();

                if (prev != null && prev.Length > 0 && prev.IsEqual(m_SelectedObjects)) {
                    // If the previous item is the new selection, then go 'back' by removing 'prev' and adding old selection to forward stack.
                    Pop(m_GoBackStack);
                    if (oldSelection != null) {
                        m_GoForwardStack.Add(oldSelection);
                    }
                } else if (next != null && next.Length > 0 && next.IsEqual(m_SelectedObjects)) {
                    // If the next item is the new select, then go 'forward' by removing 'next' and adding old selection to back stack.
                    Pop(m_GoForwardStack);
                    if (oldSelection != null) {
                        m_GoBackStack.Add(oldSelection);
                    }
                } else {
                    // Otherwise, this is just a new selection, to put the old one in the back.
                    if (oldSelection != null) {
                        m_GoBackStack.Add(oldSelection);
                    }

                    m_GoForwardStack.Clear();
                }
            }

            s_LastInfo = GetUsageInfo(m_SelectedObjects);
            EditorApplication.delayCall += Repaint;
        }

        private static Object GetRootObject(Object asset) {
            string path = AssetDatabase.GetAssetPath(asset);
            return path.IsNullOrEmpty() ? null : AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        private void GoBack() => Selection.objects = m_GoBackStack.LastOrDefault(l => l != null && l.Any(o => o));

        private void GoForward() => Selection.objects = m_GoForwardStack.LastOrDefault(l => l != null && l.Any(o => o));

        // Special version that will pop again if it finds a null result. (e.g. the object has been deleted)
        private static void Pop<T>(IList<T> list) where T : class {
            T result = null;

            while (result == null && list.Count > 0) {
                result = list.Last();
                list.RemoveAt(list.Count - 1);
            }
        }

        private static void SetSearchFilter(Object obj) {
            SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
            SearchableEditorWindow hierarchy = windows.FirstOrDefault(window => window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow");

            if (hierarchy == null) {
                return;
            }
            // SearchableEditorWindow.SearchForReferencesToInstanceID(Selection.activeInstanceID);

            MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SearchForReferencesToInstanceID", BindingFlags.NonPublic | BindingFlags.Static);
            object[] parameters = {obj.GetInstanceID()};
            setSearchType.Invoke(hierarchy, parameters);
        }
    }
}
