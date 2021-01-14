using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    [CustomEditor(typeof(TriggerSound))]
    public class TriggerSoundEditor : Editor<TriggerSound> {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (Application.isPlaying && GUILayout.Button("Test")) {
                Target.PlaySound();
            }
        }
    }
}