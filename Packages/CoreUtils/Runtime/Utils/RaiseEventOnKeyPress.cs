using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils {
    public class RaiseEventOnKeyPress : MonoBehaviour {
        [SerializeField] private bool m_Shift;
        [SerializeField] private bool m_Control;
        [SerializeField] private bool m_Command;
        [SerializeField] private KeyCode m_Key = KeyCode.Escape;
        [SerializeField] private GameEvent m_Event;

        private bool ShiftCheck => !m_Shift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private bool ControlCheck => !m_Control || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        private bool CommandCheck => !m_Command || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

        private void Update() {
            if (ShiftCheck && ControlCheck && CommandCheck && Input.GetKeyDown(m_Key)) {
                m_Event.Raise();
            }
        }
    }
}