using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils {
    public class RaiseGameEventOnInput : MonoBehaviour {
        [SerializeField] private GameEvent m_Event;
        [SerializeField] private string m_InputName = "Menu";

        private void Update() {
            if (Input.GetButtonUp(m_InputName)) {
                m_Event.Raise();
            }
        }
    }
}