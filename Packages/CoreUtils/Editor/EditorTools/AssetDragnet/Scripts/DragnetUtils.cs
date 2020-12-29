using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    public static class DragnetUtils {
        private static GUIStyle s_ErrorLabel;
        private static GUIStyle s_BoldLabel;
        private static GUIStyle s_BoldFoldout;
        private static GUIStyle s_GrayFoldout;
        // @TJM Warning Fix // private static GUIStyle s_IconButton;
        private static GUIStyle s_SmallIconButton;

        public static GUIStyle ErrorLabel =>
            s_ErrorLabel ?? (s_ErrorLabel =
                                 new GUIStyle(GUI.skin.GetStyle("label")) {fontStyle = FontStyle.Bold, normal = {textColor = Color.red}});

        public static GUIStyle BoldLabel => s_BoldLabel ?? (s_BoldLabel = new GUIStyle(GUI.skin.GetStyle("label")) {fontStyle = FontStyle.Bold});

        public static GUIStyle BoldFoldout => s_BoldFoldout ?? (s_BoldFoldout = new GUIStyle(GUI.skin.GetStyle("foldout")) {fontStyle = FontStyle.Bold});

        public static GUIStyle GrayFoldout {
            get {
                if (s_GrayFoldout != null) {
                    return s_GrayFoldout;
                }
                s_GrayFoldout = new GUIStyle(EditorStyles.foldout);
                s_GrayFoldout.active.textColor = Color.gray;
                return s_GrayFoldout;
            }
        }

        public static GUIStyle IconButton => SmallIconButton;

        //return s_IconButton ?? (s_IconButton =
        //				 new GUIStyle(GUI.skin.GetStyle("button")) {fixedWidth = 30, fixedHeight = 25, margin = new RectOffset()});
        public static GUIStyle SmallIconButton =>
            s_SmallIconButton ?? (s_SmallIconButton =
                                      new GUIStyle(GUI.skin.GetStyle("button")) {fixedWidth = 25, fixedHeight = 22, margin = new RectOffset(), padding = new RectOffset(2, 2, 3, 3)});

        public static string Multiply(this string source, int multiplier) {
            return Enumerable.Range(1, multiplier)
                             .Aggregate(new StringBuilder(multiplier*source.Length), (sb, n) => sb.Append(source)).ToString();
        }

        public static void CreateFoldersFor(string path) {
            if (path.IsNullOrEmpty()) {
                Debug.LogWarning("Can't make a directory for an empty path.");
                return;
            }

            string folder = Path.GetDirectoryName(path);

            if (folder.IsNullOrEmpty() || Directory.Exists(folder)) {
                return;
            }

            Directory.CreateDirectory(folder);
        }

        public static void DeleteEmptyFolders() {
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

            DeleteEmptyFolders(directories, AssetDatabase.DeleteAsset);
        }

        public static void DeleteEmptyFolders(HashSet<DragnetChange> changes, Func<string, bool> deleteAsset) {
            string[] oldDirectories = changes.Select(c => Path.GetDirectoryName(c.FullSourcePath)).Distinct()
                                             .OrderByDescending(p => p.Length).ToArray();

            DeleteEmptyFolders(oldDirectories, deleteAsset);
        }

        private static void DeleteEmptyFolders(string[] descendingDirectories, Func<string, bool> deleteAsset) {
            if (EditorUtility.DisplayCancelableProgressBar("Deleting Empty Folders", "Scanning...", 0.02f)) {
                EditorUtility.ClearProgressBar();
                return;
            }

            for (int i = 0; i < descendingDirectories.Length; i++) {
                string directory = descendingDirectories[i];

                if (EditorUtility.DisplayCancelableProgressBar("Deleting Empty Folders", "Checking " + directory,
                                                               i*1f/descendingDirectories.Length)) {
                    break;
                }

                if (directory.IsNullOrEmpty() || !Directory.Exists(directory) ||
                    Directory.GetFileSystemEntries(directory).Length != 0) {
                    continue;
                }

                Debug.Log("Deleting " + directory);
                deleteAsset(directory);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}