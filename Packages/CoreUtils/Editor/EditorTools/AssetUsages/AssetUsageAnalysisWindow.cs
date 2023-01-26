using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreUtils.Editor.AssetUsages {
    public class AssetUsageAnalysisWindow : EditorWindow {
        private const string kWindowName = "Asset Usage Analysis";
        private const string kAll = "ALL";

        private static AssetUsageAnalysisWindow s_Instance;

        public static AssetUsageAnalysisWindow Instance => UnityUtils.GetOrSet(ref s_Instance, () => GetWindow<AssetUsageAnalysisWindow>(kWindowName));

        [MenuItem("Tools/CoreUtils/Asset Usage Analysis Window", false, (int)MenuOrder.Window)]
        public static void OpenWindow() => Instance.Show();

        [SerializeField] private List<FileEntryCount> m_Files;
        [SerializeField] private Vector2 m_Scroll;
        [SerializeField] private List<string> m_FileTypeFilters = new List<string>();
        [SerializeField] private List<FileEntryCount> m_FilteredFiles = new List<FileEntryCount>();

        private ListView m_FileList;

        private void OnEnable() {
            GuidDataService.Init();
            GuidDataService.Updated += UpdateFiles;

            UpdateFiles();
        }

        private void OnDisable() {
            GuidDataService.Updated -= UpdateFiles;
        }

        private void UpdateFiles(AssetChanges changes) {
            UpdateFiles();
        }

        private void UpdateFiles() {
            m_Files = GuidDataService.GetFileReferences();

            m_FileTypeFilters.Clear();
            m_FileTypeFilters.Add(kAll);

            foreach (FileEntryCount file in m_Files) {
                string extension = Path.GetExtension(file.Path);
                if (!m_FileTypeFilters.Contains(extension)) {
                    m_FileTypeFilters.Add(extension);
                }
            }
        }

        public void CreateGUI() {
            Label label = new Label($"{m_Files.Count} Referenced Assets");
            rootVisualElement.Add(label);

            PopupField<string> filterPopup = new PopupField<string>("File Type Filter", m_FileTypeFilters, kAll, OnFilterList);

            rootVisualElement.Add(filterPopup);

            m_FileList = new ListView();
            m_FileList.itemsSource = m_FilteredFiles;
            m_FileList.selectionType = SelectionType.Single;
#if UNITY_2021_1_OR_NEWER
            m_FileList.fixedItemHeight = 16;
#else
            m_FileList.itemHeight = 16;
#endif
            m_FileList.makeItem = OnMakeFileItem;
            m_FileList.bindItem = OnBindFileItem;
            m_FileList.style.flexGrow = 1.0f;

            rootVisualElement.Add(m_FileList);
        }

        private VisualElement OnMakeFileItem() {
            Button b = new Button();
            b.clicked += () => OnAssetButtonClicked(b);
            return b;
        }

        private void OnBindFileItem(VisualElement element, int index) {
            if (element is Button b) {
                FileEntryCount file = m_FilteredFiles[index];
                b.style.unityTextAlign = TextAnchor.MiddleLeft;
                b.text = $"{file.DisplayPath} ({file.ReferenceCount})";
                b.userData = file;
                b.SetEnabled(file.Exists);
            }
        }

        private void OnAssetButtonClicked(Button button) {
            FileEntryCount file = button.userData as FileEntryCount;
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(file.Path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private string OnFilterList(string filter) {
            m_FilteredFiles.Clear();

            if (filter != kAll) {
                foreach (FileEntryCount file in m_Files) {
                    string extension = Path.GetExtension(file.Path);
                    if (extension.Equals(filter)) {
                        m_FilteredFiles.Add(file);
                    }
                }
            } else {
                m_FilteredFiles.AddRange(m_Files);
            }

            if (m_FileList != null) {
                m_FileList.itemsSource = m_FilteredFiles;
#if UNITY_2021_1_OR_NEWER
                m_FileList.Rebuild();
#else
                m_FileList.Refresh();
#endif
            }

            return filter;
        }
    }
}
