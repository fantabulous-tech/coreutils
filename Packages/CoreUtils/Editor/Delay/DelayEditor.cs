using UnityEditor;
using UnityEngine;

namespace CoreUtils {
    [CustomEditor(typeof(Delay))]
    public class DelayEditor : UnityEditor.Editor {
        private Delay m_Target;

        private Delay Target => m_Target ? m_Target : m_Target = (Delay) target;

        private void OnEnable() {
            Target.DelayEventsChanged += DelayEventsChanged;
        }

        private void OnDisable() {
            Target.DelayEventsChanged -= DelayEventsChanged;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.LabelField("Delay List:", EditorStyles.boldLabel);

                foreach (DelaySequence delay in Target.DelaySequences) {
                    if (delay.Context) {
                        EditorGUILayout.ObjectField(delay.Name, delay.Context, typeof(Object), true);
                    } else {
                        EditorGUILayout.LabelField(delay.Name);
                    }
                }
            }
        }

        private void DelayEventsChanged() {
            Repaint();
        }
    }
}