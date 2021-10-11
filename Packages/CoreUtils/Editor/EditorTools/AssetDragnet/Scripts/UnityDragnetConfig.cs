using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    [CreateAssetMenu(menuName = "CoreUtils/AssetDragnet/UnityDragnetConfig", order = (int) MenuOrder.Config)]
    public class UnityDragnetConfig : BaseDragnetConfig {
        public string AssetFilter = "t:Prefab";

        protected override string RootPath => "Assets";

        protected override string MoveAsset(string source, string destination) {
            return AssetDatabase.MoveAsset(source, destination);
        }

        protected override IEnumerable<string> GetUnfilteredAssetPaths() {
            int trimIndex = RootPath.Length + 1;

            IEnumerable<string> allAssetPaths = AssetFilter.IsNullOrEmpty()
                                                    ? AssetDatabase.GetAllAssetPaths()
                                                    : AssetDatabase.FindAssets(AssetFilter).Select(AssetDatabase.GUIDToAssetPath);

            return allAssetPaths.Where(p => p.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase) && File.Exists(p)).Select(p => p.Substring(trimIndex));
        }

        protected override void TopConfigGUI() {
            string newAssetFilter = EditorGUILayout.DelayedTextField("Project Search", AssetFilter);

            if (AssetFilter == newAssetFilter) {
                return;
            }

            Undo.RecordObject(this, "Asset Search Change");
            AssetFilter = newAssetFilter;
            RefreshAssetPaths();
        }

        protected override void PostFolderCreation() {
            // Force import of all new folders.
            EditorUtility.DisplayCancelableProgressBar("Asset Dragnet", "Loading new directories...", -1);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.StartAssetEditing();
        }

        protected override void PostFileMove() {
            EditorUtility.DisplayCancelableProgressBar("Asset Dragnet", "Refreshing asset database...", 0.95f);
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        protected override bool DeleteAsset(string path) {
            return AssetDatabase.DeleteAsset(path);
        }
    }

    public enum Direction {
        Top,
        Up,
        Down,
        Bottom
    }
}
