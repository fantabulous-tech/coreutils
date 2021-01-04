using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class StateEvents : MonoBehaviour {
        [SerializeField, AutoFill] private State m_State;

        public UnityEvent OnStateEntered;
        public UnityEvent OnStateExited;

        private void OnEnable() {
            m_State.OnEntered += OnStateEntered.Invoke;
            m_State.OnExited += OnStateExited.Invoke;
        }

        private void OnDisable() {
            if (m_State != null) {
                m_State.OnEntered -= OnStateEntered.Invoke;
                m_State.OnExited -= OnStateExited.Invoke;
            }
        }
    }
}