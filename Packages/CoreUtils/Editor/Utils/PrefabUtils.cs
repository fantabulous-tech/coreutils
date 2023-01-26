using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public static class PrefabUtils {
        [MenuItem("Tools/CoreUtils/Reset Child Transform Overrides", true, (int)MenuOrder.GameObject)]
        public static bool CanResetTransformOverrides() {
            return Selection.gameObjects.Any(EditorUtils.IsInstance);
        }

        [MenuItem("Tools/CoreUtils/Reset Child Transform Overrides", false, (int)MenuOrder.GameObject)]
        private static void ResetTransformOverrides() {
            foreach (Transform activeTransform in Selection.transforms) {
                foreach (Transform t in activeTransform.GetComponentsInChildren<Transform>()) {
                    if (!t) {
                        continue;
                    }

                    try {
                        PrefabUtility.RevertObjectOverride(t, InteractionMode.UserAction);
                    }
                    catch (Exception e) {
                        Debug.Log($"Couldn't revert {t.name} because {e.Message}", t);
                    }
                }
            }
        }
    }
}
