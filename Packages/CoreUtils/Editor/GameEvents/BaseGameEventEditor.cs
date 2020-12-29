using CoreUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.GameEvents {
    [CustomEditor(typeof(BaseGameEvent), true), CanEditMultipleObjects]
    public class BaseGameEventEditor : Editor<BaseGameEvent> {

        private void OnEnable() {
            if (Application.isPlaying) {
                Targets.ForEach(t => t.GenericEvent += OnEvent);
            }
        }

        private void OnDisable() {
            if (Application.isPlaying) {
                Targets.ForEach(t => t.GenericEvent -= OnEvent);
            }
        }

        private void OnEvent() {
            Repaint();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (Application.isPlaying) {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying || targets.Length != 1)) {
                    EditorUtils.OptionalUnserializedPropertyFieldGUILayout(target, "Value", "Current Value");
                }
            }

            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying)) {
                if (GUILayout.Button("Raise")) {
                    Targets.ForEach(t => t.Raise());
                }
            }
        }
    }
}