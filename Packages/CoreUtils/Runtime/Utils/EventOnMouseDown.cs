using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CoreUtils {
    public class EventOnMouseDown : MonoBehaviour {
        [SerializeField] private bool m_Global = true;

        public UnityEvent MouseDown;
        public UnityEvent MouseUp;

        private bool m_MouseWasDown;

        private void OnMouseUp() {
            if (!m_Global) {
                MouseUp.Invoke();
            }
        }

        private void OnMouseDown() {
            if (!m_Global) {
                MouseDown.Invoke();
            }
        }

        private void Update() {
            if (!m_Global) {
                return;
            }

            bool mouseDown = !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0);

            if (mouseDown && !m_MouseWasDown) {
                m_MouseWasDown = true;
                MouseDown.Invoke();
            }

            if (!mouseDown && m_MouseWasDown) {
                m_MouseWasDown = false;
                MouseUp.Invoke();
            }
        }
    }
}