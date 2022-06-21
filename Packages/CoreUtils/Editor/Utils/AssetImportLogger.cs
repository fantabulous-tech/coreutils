using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public class AssetImportLogger : AssetPostprocessor {
        private const string kLogAssetImportsKey = "CoreUtils.LogAssetImports";

        private static int s_LogAssetImports = -1;

        private static bool LogAssetImports {
            get => EditorUtils.GetEditorPrefBool(ref s_LogAssetImports, kLogAssetImportsKey);
            set => EditorUtils.SetEditorPrefBool(ref s_LogAssetImports, value, kLogAssetImportsKey);
        }

        [InitializeOnLoadMethod]
        private static void Init() {
            CoreUtilsSettings.Register("Log Asset Imports", OnGUI);
        }

        private static void OnGUI() {
            LogAssetImports = EditorGUILayout.Toggle(new GUIContent("Log Asset Imports", "This logs each time assets are imported. Helpful for debugging import issues."), LogAssetImports);
        }

        public static void OnPostprocessAllAssets(string[] importedPaths, string[] deletedPaths, string[] movedToPaths, string[] movedFromPaths) {
            if (LogAssetImports) {
                LogChanges(importedPaths, deletedPaths, movedToPaths);
            }
        }

        private static void LogChanges(IReadOnlyCollection<string> importedPaths, IReadOnlyCollection<string> deletedPaths, IReadOnlyCollection<string> movedToPaths) {
            const int kLimit = 5000;
            string summary = $"{nameof(LogAssetImports)} changes: {importedPaths.Count:N0} imported, {deletedPaths.Count:N0} deleted, {movedToPaths.Count:N0} moved";

            void AddSummary(string name, IReadOnlyCollection<string> files) {
                if (files != null && files.Count > 0) {
                    summary += files.Count > kLimit ? $"\n\n{name}: Too many to list! (>{kLimit})" : $"\n\n{name}:\n{AggregateToString(files)}";
                }
            }

            AddSummary("Imported", importedPaths);
            AddSummary("Deleted", deletedPaths);
            AddSummary("Moved", movedToPaths);

            Debug.Log($"{summary}\n");
        }

        private static string AggregateToString(IEnumerable<string> strings) {
            return string.Join("\n", strings);
        }
    }
}
