using System.Linq;
using UnityEditor;

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
        private static double s_LastChangesTime;

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (SceneSaved != null) {
                importedAssets.Where(p => p.EndsWith(".unity")).Distinct().ForEach(p => SceneSaved(p));
            }

            if (importedAssets.Length > 0 && AssetImported != null) {
                importedAssets.Distinct().ForEach(p => AssetImported(p));
            }

            if (deletedAssets.Length > 0 && AssetsDeleted != null) {
                AssetsDeleted(deletedAssets);
            }

            if (importedAssets.Length > 0 && AssetsImported != null) {
                AssetsImported(importedAssets);
            }

            if (movedAssets.Length > 0 && AssetsMoved != null) {
                AssetsMoved(movedAssets);
            }

            if (AssetsChanged == null && DelayedAssetsChanged == null) {
                return;
            }

            AssetChanges changes = new AssetChanges(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);

            if (AssetsChanged != null) {
                AssetsChanged(changes);
            }

            if (DelayedAssetsChanged != null) {
                s_LastChanges.Merge(changes);

                if (EditorApplication.timeSinceStartup > s_LastChangesTime) {
                    EditorApplication.delayCall += OnDelayCall;
                    s_LastChangesTime = EditorApplication.timeSinceStartup + 1;
                }
            }
        }

        private static void OnDelayCall() {
            DelayedAssetsChanged?.Invoke(s_LastChanges);
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

        public void Merge(AssetChanges other) {
            Imported = Merge(Imported, other.Imported);
            Deleted = Merge(Deleted, other.Deleted);
            MovedFrom = Merge(MovedFrom, other.MovedFrom);
            MovedTo = Merge(MovedTo, other.MovedTo);
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