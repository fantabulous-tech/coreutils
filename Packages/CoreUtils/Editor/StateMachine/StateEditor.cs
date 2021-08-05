using CoreUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace CoreUtils {
    [CustomEditor(typeof(State), true)]
    public class StateEditor : Editor<State> {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (!Target.StateMachine) {
                return;
            }

            GUILayout.BeginHorizontal();

            GUI.color = Color.green;
            GUI.enabled = Target.StateMachine.CurrentState != Target.gameObject;
            if (GUILayout.Button("Select")) {
                Target.ChangeState(Target.gameObject);
                EditorUtility.SetDirty(Target);
            }

            GUI.enabled = Target.StateMachine.CurrentState != null;
            GUI.color = Colors.LightRed;
            if (GUILayout.Button("Exit")) {
                Undo.RegisterCompleteObjectUndo(Target.transform.parent.transform, "Exit");
                Target.Exit();
                EditorUtility.SetDirty(Target);
            }

            GUILayout.EndHorizontal();
        }
    }
}