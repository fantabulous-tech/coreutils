using System;
using System.Collections.Generic;
using System.IO;
using CoreUtils.Editor.AssetUsages;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

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
            private set => SetBoolShared(ref Instance.m_DisableAssetBucketScanning, value);
        }

        public static bool DisableAssetGuidDatabase {
            get => Instance.m_DisableAssetGuidDatabase;
            private set {
                SetBoolShared(ref Instance.m_DisableAssetGuidDatabase, value);
                if (!value) {
                    GuidDataService.Init();
                }
            }
        }

        [SettingsProvider]
        private static SettingsProvider GetSettingsProvider() => new SettingsProvider("Project/CoreUtils", SettingsScope.Project) { guiHandler = searchContext => OnSettingsGUI() };

        private static void OnSettingsGUI() {
            EditorGUILayout.Space();
            GUILayout.Label(new GUIContent("Shared Settings", $"Shared Settings are saved to:\n{s_FilePath}\n\nChanges here go to this file and, if committed, will be shared with the team."), EditorStyles.boldLabel);
            DisableAssetBucketScanning = EditorGUILayout.Toggle(
                new GUIContent("Disable Bucket Watcher", "This stops Asset Bucket Watcher from auto-updating Asset Buckets on file import."),
                DisableAssetBucketScanning
            );

            DisableAssetGuidDatabase = EditorGUILayout.Toggle(
                new GUIContent("Disable GUID Database", "This stops tracking assets for the 'Asset Usages' window on file import."),
                DisableAssetGuidDatabase
            );

            EditorGUILayout.Space();
            GUILayout.Label(new GUIContent("Local Settings", "Local Settings are saved to EditorPrefs and won't check out files or be shared with the team."), EditorStyles.boldLabel);

            PreferencesGUI();
        }

        private static CoreUtilsSettings CreateOrLoad() {
            //try load
            InternalEditorUtility.LoadSerializedFileAndForget(s_FilePath);

            //else create
            if (s_Instance == null) {
                CoreUtilsSettings created = CreateInstance<CoreUtilsSettings>();
                created.hideFlags = HideFlags.HideAndDontSave;
            }

            Debug.Assert(s_Instance != null);
            return s_Instance;
        }

        private static void SetBoolShared(ref bool b, bool value) {
            if (b == value) {
                return;
            }

            b = value;
            Save();
        }

        private static void Save() {
            if (s_Instance == null) {
                UnityEngine.Debug.Log("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            string folderPath = Path.GetDirectoryName(s_FilePath);

            if (folderPath != null && !Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            if (Provider.hasCheckoutSupport) {
                Provider.Checkout(s_FilePath, CheckoutMode.Asset);
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { s_Instance }, s_FilePath, true);
        }

        private static GUIStyle FoldoutTitleStyle {
            get {
                if (s_SectionTitleStyle != null) {
                    return s_SectionTitleStyle;
                }

                s_SectionTitleStyle = new GUIStyle(GUI.skin.GetStyle("Foldout")) { fontStyle = FontStyle.Bold, fontSize = 14, alignment = TextAnchor.MiddleLeft };
                return s_SectionTitleStyle;
            }
        }
        private static GUIStyle s_SectionTitleStyle;

        private static readonly List<PreferenceGUI> s_RegisteredTools = new List<PreferenceGUI>();
        private static Vector2 s_ScrollPos;

        public static void Register(string sectionName, Action onGUI) {
            int removedCount = s_RegisteredTools.RemoveAll(t => t.SectionName == sectionName);
            if (removedCount > 0) {
                //TODO: Warn about collisions.
            }
            s_RegisteredTools.Add(new PreferenceGUI(sectionName, onGUI));
            s_RegisteredTools.Sort((t1, t2) => string.Compare(t1.SectionName, t2.SectionName, StringComparison.Ordinal));
        }

        private static void PreferencesGUI() {
            s_ScrollPos = EditorGUILayout.BeginScrollView(s_ScrollPos);
            s_RegisteredTools.ForEach(gui => gui.OnGUI());
            GUILayout.Label("\n\nNOTE: To add more, use 'CoreUtilsSettings.Register()'.");
            EditorGUILayout.EndScrollView();
        }

        private class PreferenceGUI {
            public string SectionName { get; private set; }
            private Action OnPreferencesGUI { get; set; }

            private string EditorPrefKey {
                get { return "CoreUtilSettings." + SectionName; }
            }
            private bool m_FoldOut;

            public PreferenceGUI(string sectionName, Action onGUI) {
                SectionName = sectionName;
                OnPreferencesGUI = onGUI;
                m_FoldOut = EditorPrefs.GetBool(EditorPrefKey, true);
            }

            public void OnGUI() {
                GUILayout.Space(10);
                bool newFoldout = GUILayout.Toggle(m_FoldOut, SectionName, FoldoutTitleStyle);

                if (m_FoldOut != newFoldout) {
                    m_FoldOut = newFoldout;
                    EditorPrefs.SetBool(EditorPrefKey, m_FoldOut);
                }

                if (m_FoldOut) {
                    OnPreferencesGUI();
                }
            }
        }
    }
}
