using UnityEngine;

namespace CoreUtils {
    public class ForceCursorLock : MonoBehaviour {
        [SerializeField] private KeyCode m_Key = KeyCode.CapsLock;
        [SerializeField] private Behaviour[] m_EnableWhenLockedList;

        private bool m_Locked = true;
        private CursorLockMode LockState => m_Locked ? CursorLockMode.Locked : CursorLockMode.None;

        private void LateUpdate() {
            if (Input.GetKeyUp(m_Key)) {
                m_Locked = !m_Locked;
                Cursor.lockState = LockState;
                Cursor.visible = !m_Locked;
                m_EnableWhenLockedList.ForEach(b => b.enabled = m_Locked);
            }
        }
    }
}