using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    [CustomEditor(typeof(BaseDragnetConfig), true)]
    public class DragnetConfigEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Open in Asset Dragnet Window")) {
                DragnetWindow.Init(target as BaseDragnetConfig);
            }
            base.OnInspectorGUI();
        }
    }
}