using CoreUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace CoreUtils {
    [CustomEditor(typeof(StateMachine), true)]
    public class StateMachineEditor : Editor<StateMachine> {
        private const int kColumnMax = 200;
        private const float kSpacer = 3;

        public override void OnInspectorGUI() {
            //if no states are found:
            if (Target.transform.childCount == 0) {
                DrawNotification("Add child Gameobjects for this State Machine to control.", Color.yellow);
                return;
            }

            serializedObject.Update();
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
            DrawStateChangeButtons();
        }

        private void DrawStateChangeButtons() {
            if (Target.transform.childCount == 0) {
                return;
            }
            Color currentColor = GUI.color;
            float width = EditorGUIUtility.currentViewWidth - 23;
            GameObject currentState = Target.CurrentState;

            GUILayout.BeginHorizontal();
            int columns = (int) width/kColumnMax;
            int itemCount = 0;
            float buttonWidth = (width - kSpacer*(columns - 1))/columns;

            for (int i = -1; i < Target.transform.childCount; i++) {
                if (i < 0) {
                    GUI.color = currentState == null ? Color.green : Colors.LightRed;
                    if (GUILayout.Button($"{i}. Exit", GUILayout.Width(buttonWidth))) {
                        Target.Exit();
                    }
                } else {
                    GameObject current = Target.transform.GetChild(i).gameObject;
                    GUI.color = current == currentState ? Color.green : Color.white;

                    if (GUILayout.Button($"{i}. {current.name}", GUILayout.Width(buttonWidth))) {
                        Target.ChangeState(current);
                    }
                }

                itemCount++;
                if (itemCount >= columns) {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    itemCount = 0;
                }
            }
            GUILayout.EndHorizontal();

            float navButtonWidth = (width - kSpacer)/2;
            GUI.color = Colors.DeepSkyBlue;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous", GUILayout.Width(navButtonWidth))) {
                Undo.RegisterCompleteObjectUndo(Target.transform, "Previous");
                if (Target.CurrentState == Target.FirstState) {
                    Target.Exit();
                } else if (Target.CurrentState == null && Target.transform.childCount > 0) {
                    Target.ChangeState(Target.transform.GetChild(Target.transform.childCount - 1).gameObject);
                } else {
                    Target.Previous();
                }
            }
            if (GUILayout.Button("Next", GUILayout.Width(navButtonWidth))) {
                Undo.RegisterCompleteObjectUndo(Target.transform, "Next");
                if (Target.CurrentState == Target.LastState) {
                    Target.Exit();
                } else {
                    Target.Next();
                }
            }
            GUILayout.EndHorizontal();
            GUI.color = currentColor;
        }

        private void DrawNotification(string message, Color color) {
            Color currentColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.HelpBox(message, MessageType.Warning);
            GUI.color = currentColor;
        }
    }
}