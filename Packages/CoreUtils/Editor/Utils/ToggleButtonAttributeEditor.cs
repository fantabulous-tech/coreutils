using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    [CustomPropertyDrawer(typeof(ToggleButtonAttribute))]
    public class ToggleButtonAttributeEditor : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            property.boolValue = GUI.Toggle(position, property.boolValue, label, GUI.skin.button);
        }
    }
}