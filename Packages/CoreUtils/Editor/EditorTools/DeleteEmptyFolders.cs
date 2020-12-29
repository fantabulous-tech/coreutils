using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public static class DeleteEmptyFolders {
        [MenuItem("Tools/Delete Empty Folders")]
        public static void Go() {
            string[] directories = Directory.GetDirectories("Assets", "*.*", SearchOption.AllDirectories);

            if (EditorUtility.DisplayCancelableProgressBar("Deleting Empty Folders", "Sorting...", 0.01f)) {
                return;
            }

            Array.Sort(directories, (d1, d2) => {
                int result = d2.Length.CompareTo(d1.Length);
                if (result == 0) {
                    result = string.Compare(d1, d2, StringComparison.OrdinalIgnoreCase);
                }
                return result;
            });

            if (EditorUtility.DisplayCancelableProgressBar("Deleting Empty Folders", "Scanning...", 0.02f)) {
                EditorUtility.ClearProgressBar();
                return;
            }

            for (int i = 0; i < directories.Length; i++) {
                string directory = directories[i];
                float progress = i*1f/directories.Length;

                if (EditorUtility.DisplayCancelableProgressBar("Deleting Empty Folders", $"Checking {directory}", progress)) {
                    break;
                }

                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory) ||
                    Directory.GetFileSystemEntries(directory).Length != 0) {
                    continue;
                }

                Debug.Log($"Deleting {directory}");
                AssetDatabase.DeleteAsset(directory);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}