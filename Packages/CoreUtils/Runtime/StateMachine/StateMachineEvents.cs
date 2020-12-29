using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class StateMachineEvents : MonoBehaviour {
        [SerializeField, AutoFill] private StateMachine m_StateMachine;

        public UnityEventGameObject OnStateEntered;
        public UnityEventGameObject OnStateExited;
        public UnityEvent OnFirstStateEntered;
        public UnityEvent OnFirstStateExited;
        public UnityEvent OnLastStateEntered;
        public UnityEvent OnLastStateExited;
 
        private void OnEnable() {
            m_StateMachine.OnStateEntered += OnStateEntered.Invoke;
            m_StateMachine.OnStateExited += OnStateExited.Invoke;
            m_StateMachine.OnFirstStateEntered += OnFirstStateEntered.Invoke;
            m_StateMachine.OnFirstStateExited += OnFirstStateExited.Invoke;
            m_StateMachine.OnLastStateEntered += OnLastStateEntered.Invoke;
            m_StateMachine.OnLastStateExited += OnLastStateExited.Invoke;
        }
 
        private void OnDisable() {
            if (m_StateMachine != null) {
                m_StateMachine.OnStateEntered -= OnStateEntered.Invoke;
                m_StateMachine.OnStateExited -= OnStateExited.Invoke;
                m_StateMachine.OnFirstStateEntered -= OnFirstStateEntered.Invoke;
                m_StateMachine.OnFirstStateExited -= OnFirstStateExited.Invoke;
                m_StateMachine.OnLastStateEntered -= OnLastStateEntered.Invoke;
                m_StateMachine.OnLastStateExited -= OnLastStateExited.Invoke;
            }
        }
    }
}