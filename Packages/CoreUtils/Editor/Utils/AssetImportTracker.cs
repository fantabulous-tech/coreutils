using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public class AssetImportTracker : AssetPostprocessor {
        public static event FileEventHandler SceneSaved;
        public static event FileEventHandler AssetImported;
        public static event FilesEventHandler AssetsImported;
        public static event FilesEventHandler AssetsDeleted;
        public static event FilesEventHandler AssetsMoved;
        public static event AssetsChangedHandler AssetsChanged;
        public static event AssetsChangedHandler DelayedAssetsChanged;

        public delegate void FileEventHandler(string filePath);

        public delegate void FilesEventHandler(string[] filePaths);

        public delegate void AssetsChangedHandler(AssetChanges changes);

        private static AssetChanges s_LastChanges;

        public static void OnPostprocessAllAssets(string[] importedPaths, string[] deletedPaths, string[] movedToPaths, string[] movedFromPaths) {
            if (SceneSaved != null) {
                importedPaths.Where(p => p.EndsWith(".unity")).Distinct().ForEach(p => SceneSaved(p));
            }

            if (importedPaths.Length > 0 && AssetImported != null) {
                importedPaths.Distinct().ForEach(p => AssetImported(p));
            }

            if (deletedPaths.Length > 0) {
                AssetsDeleted?.Invoke(deletedPaths);
            }

            if (importedPaths.Length > 0) {
                AssetsImported?.Invoke(importedPaths);
            }

            if (movedToPaths.Length > 0) {
                AssetsMoved?.Invoke(movedToPaths);
            }

            if (AssetsChanged == null && DelayedAssetsChanged == null) {
                return;
            }

            AssetChanges changes = new AssetChanges(importedPaths, deletedPaths, movedToPaths, movedFromPaths);

            AssetsChanged?.Invoke(changes);

            if (DelayedAssetsChanged != null) {
                EditorApplication.delayCall -= OnDelayCall;
                s_LastChanges.Merge(changes);

                // If there is a script in the list, then notify immediately as the callback won't trigger next frame.
                if (s_LastChanges.HasScript) {
                    OnDelayCall();
                } else {
                    EditorApplication.delayCall += OnDelayCall;
                }
            }
        }

        private static void OnDelayCall() {
            if (s_LastChanges.IsValid) {
                DelayedAssetsChanged?.Invoke(s_LastChanges);
            }

            s_LastChanges = new AssetChanges();
        }
    }

    public struct AssetChanges {
        public bool IsValid;
        public string[] Imported;
        public string[] Deleted;
        public string[] MovedFrom;
        public string[] MovedTo;

        public AssetChanges(string[] imported, string[] deleted, string[] movedTo, string[] movedFrom) {
            Imported = imported;
            Deleted = deleted;
            MovedTo = movedTo;
            MovedFrom = movedFrom;
            IsValid = true;
        }

        public bool HasScript => Imported.Any(IsScript) || Deleted.Any(IsScript) || MovedTo.Any(IsScript);

        private static bool IsScript(string path) {
            return path.EndsWith(".cs");
        }

        public void Merge(AssetChanges other) {
            Imported = Merge(Imported, other.Imported);
            Deleted = Merge(Deleted, other.Deleted);
            MovedFrom = Merge(MovedFrom, other.MovedFrom);
            MovedTo = Merge(MovedTo, other.MovedTo);
            IsValid = true;
        }

        private static string[] Merge(string[] a, string[] b) {
            if (a == null && b == null) {
                return new string[0];
            }

            if (b == null) {
                return a;
            }

            if (a == null) {
                return b;
            }

            return a.IsEqual(b) ? a : a.Union(b).ToArray();
        }
    }
}
