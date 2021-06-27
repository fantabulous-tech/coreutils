using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public static class PrefabUtils {
        [MenuItem("Tools/CoreUtils/Reset Child Transform Overrides", true, (int) MenuOrder.GameObject)]
        public static bool CanResetTransformOverrides() {
            return Selection.gameObjects.Any(EditorUtils.IsInstance);
        }

        [MenuItem("Tools/CoreUtils/Reset Child Transform Overrides", false, (int) MenuOrder.GameObject)]
        private static void ResetTransformOverrides() {
            foreach (Transform t in Selection.activeTransform.GetComponentsInChildren<Transform>()) {
                if (!t) {
                    continue;
                }
                PrefabUtility.RevertObjectOverride(t, InteractionMode.UserAction);
            }
        }
    }
}