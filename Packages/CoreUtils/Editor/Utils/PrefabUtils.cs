using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public static class PrefabUtils {
        [MenuItem("Tools/Reset Transform Overrides")]
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