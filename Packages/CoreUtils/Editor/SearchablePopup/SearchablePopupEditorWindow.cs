using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
	public class SearchablePopupEditorWindow : EditorWindow {
		public static void ShowAsPopup(Rect buttonRect, SearchablePopup content) {
			SearchablePopupEditorWindow window = CreateInstance<SearchablePopupEditorWindow>();
			window.Init(buttonRect, content);
		}

		private SearchablePopup m_Content;
		private Rect m_ActivatorRect;
		private Vector2 m_LastWantedSize;

		private void Init(Rect buttonRect, SearchablePopup content) {
			m_Content = content;
			content.SetEditorWindow(this);
			m_LastWantedSize = content.GetWindowSize(false);
			m_LastWantedSize.x = Mathf.Max(m_LastWantedSize.x, buttonRect.width);

			Vector2 screenPos = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.y));
			m_ActivatorRect = buttonRect;
			m_ActivatorRect.x = screenPos.x;
			m_ActivatorRect.y = screenPos.y;

			ShowAsDropDown(m_ActivatorRect, m_LastWantedSize);
			if (Event.current != null) {
				// We're inside OnGUI stuff, Bail out immediately
				GUIUtility.ExitGUI();
			}
		}

		protected void OnGUI() {
			Rect rect = new Rect(0f, 0f, position.width, position.height);
			if (m_Content != null) {
				m_Content.OnGUI(rect);
				GUI.Label(rect, GUIContent.none, "grey_border");
			} else {
				Close();
			}
		}
	}
}