using UnityEngine;

namespace CoreUtils.Editor {
    public static class CustomEditorStyle {
        private static GUIStyle s_ErrorLabel;

        public static GUIStyle ErrorLabel => UnityUtils.GetOrSet(ref s_ErrorLabel, () => new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold, normal = {textColor = Color.red}});
    }
}