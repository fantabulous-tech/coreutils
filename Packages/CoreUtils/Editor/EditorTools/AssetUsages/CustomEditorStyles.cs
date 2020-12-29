using UnityEngine;

namespace CoreUtils.Editor.AssetUsages {
	public static class CustomEditorStyles {
		private static GUIStyle s_ButtonLeft;

		public static GUIStyle ButtonLeft => UnityUtils.GetOrSet(ref s_ButtonLeft, () => new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleLeft});
	}
}