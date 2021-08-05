using System.Linq;
using CoreUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CustomEditor(typeof(SceneBucket))]
    public class SceneBucketEditor : GenericBucketEditor<SceneBucket, string> {
        public override void OnInspectorGUI() {
            DrawPropertiesExcluding(serializedObject, "m_Items");

            GUILayout.Label("Included Scenes:", EditorStyles.boldLabel);

            foreach (string item in Target.Items) {
                GUILayout.Label(item);
            }

            if (GUILayout.Button("Update Build Scenes")) {
                Target.ForceUpdateBuildScenes();
                EditorUtility.SetDirty(Target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        [InitializeOnLoadMethod]
        public static void OnLoad() {
            AssetImportTracker.AssetsChanged += OnAssetsChanged;
        }

        private static void OnAssetsChanged(AssetChanges changes) {
            AssetDatabase.FindAssets("t:SceneBucket").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<SceneBucket>).ForEach(b => b.OnValidate());
        }
    }
}