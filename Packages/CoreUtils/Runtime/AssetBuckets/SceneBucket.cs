using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "CoreUtils/Bucket/Scene Bucket", order = (int) MenuOrder.Bucket)]
    public class SceneBucket : GenericBucket<string> {
        public override string[] ItemNames => m_Items.Select(i => i.ReplaceRegex(@"^.*[/\\]([^/\\]+)\.unity$", "$1", RegexOptions.IgnoreCase)).ToArray();
        
        

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private string[] m_MainPaths;
        [SerializeField] private Object[] m_MainScenes;
        [SerializeField] private Object m_SceneFolder;
        [SerializeField] private bool m_AutoUpdateBuildScenes;

        public void OnValidate() {
            ValidateMainScenes();
            ValidateSceneList();
            UpdateBuildScenes(false);
        }

        private void UpdateBuildScenes(bool force) {
            if (!m_AutoUpdateBuildScenes && !force) {
                return;
            }

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(Items.Select(path => new EditorBuildSettingsScene(path, true)));

            for (int i = m_MainPaths.Length - 1; i >= 0; i--) {
                string path = m_MainPaths[i];
                if (!path.IsNullOrEmpty()) {
                    scenes.Insert(0, new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public void ForceUpdateBuildScenes() {
            ValidateMainScenes();
            ValidateSceneList();
            UpdateBuildScenes(true);
        }

        private void ValidateMainScenes() {
            if (m_MainScenes == null) {
                return;
            }

            m_MainPaths = new string[m_MainScenes.Length];

            for (int i = 0; i < m_MainScenes.Length; i++) {
                Object scene = m_MainScenes[i];
                string path = scene ? AssetDatabase.GetAssetPath(scene) : null;
                bool isScene = !path.IsNullOrEmpty() && path.EndsWith(".unity");
                m_MainScenes[i] = isScene ? scene : null;
                m_MainPaths[i] = isScene ? path : null;
            }
        }

        private void ValidateSceneList() {
            if (!m_SceneFolder) {
                return;
            }

            string path = AssetDatabase.GetAssetPath(m_SceneFolder);

            if (!Directory.Exists(path)) {
                Debug.LogWarningFormat(this, m_SceneFolder + " is not a folder.");
                m_SceneFolder = null;
                return;
            }

            m_Items = Directory.GetFiles(path, "*.unity").Select(p => p.Replace("\\", "/")).ToArray();
        }

        public override bool Has(string itemName) {
            return Items.Any(item => item != null && (item.Equals(itemName, StringComparison.OrdinalIgnoreCase) || item.EndsWith($"/{itemName}.unity", StringComparison.OrdinalIgnoreCase)));
        }
#endif
    }
}