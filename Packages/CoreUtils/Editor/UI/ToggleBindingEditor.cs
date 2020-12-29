using UnityEditor;

namespace CoreUtils.UI {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in ToggleBinding to be expandable in the Inspector.
    [CustomEditor(typeof(ToggleBinding))]
    public class ToggleBindingEditor : UnityEditor.Editor { }
}