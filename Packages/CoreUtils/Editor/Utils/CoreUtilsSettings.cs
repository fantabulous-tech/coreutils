using System.IO;
using CoreUtils.Editor.AssetUsages;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CoreUtils.Editor {
    public class CoreUtilsSettings : ScriptableObject {
        private static readonly string s_FilePath = $"ProjectSettings/{nameof(CoreUtilsSettings)}.asset";

        private static CoreUtilsSettings s_Instance;
        private static CoreUtilsSettings Instance => UnityUtils.GetOrSet(ref s_Instance, CreateOrLoad);

        [SerializeField] private bool m_DisableAssetBucketScanning;
        [SerializeField] private bool m_DisableAssetGuidDatabase;

        private CoreUtilsSettings() {
            s_Instance = this;
        }

        public static bool DisableAssetBucketScanning {
            get => Instance.m_DisableAssetBucketScanning;
            set {
                if (Instance.m_DisableAssetBucketScanning == value) {
                    return;
                }

                Instance.m_DisableAssetBucketScanning = value;
                Save();
            }
        }

        public static bool DisableAssetGuidDatabase {
            get => Instance.m_DisableAssetGuidDatabase;
            set {
                if (Instance.m_DisableAssetGuidDatabase == value) {
                    return;
                }

                Instance.m_DisableAssetGuidDatabase = value;
                Save();

                if (!value) {
                    GuidDataService.Init();
                }
            }
        }

        [SettingsProvider]
        private static SettingsProvider GetSettingsProvider() => new SettingsProvider("Project/CoreUtils", SettingsScope.Project) {guiHandler = searchContext => OnSettingsGUI()};

        private static void OnSettingsGUI() {
            DisableAssetBucketScanning = EditorGUILayout.Toggle(
                new GUIContent("Disable Bucket Watcher", "This stops Asset Bucket Watcher from auto-updating Asset Buckets on file import."),
                DisableAssetBucketScanning
            );

            DisableAssetGuidDatabase = EditorGUILayout.Toggle(
                new GUIContent("Disable GUID Database", "This stops tracking assets for the 'Asset Usages' window on file import."),
                DisableAssetGuidDatabase
            );
        }

        private static CoreUtilsSettings CreateOrLoad() {
            //try load
            InternalEditorUtility.LoadSerializedFileAndForget(s_FilePath);

            //else create
            if (s_Instance == null) {
                CoreUtilsSettings created = CreateInstance<CoreUtilsSettings>();
                created.hideFlags = HideFlags.HideAndDontSave;
            }

            System.Diagnostics.Debug.Assert(s_Instance != null);
            return s_Instance;
        }

        private static void Save() {
            if (s_Instance == null) {
                Debug.Log("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            string folderPath = Path.GetDirectoryName(s_FilePath);
            if (folderPath != null && !Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] {s_Instance}, s_FilePath, true);
        }
    }
}
