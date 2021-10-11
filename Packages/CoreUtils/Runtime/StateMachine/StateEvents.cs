using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class StateEvents : MonoBehaviour {
        [SerializeField, AutoFill] private State m_State;

        public UnityEventBool StateActiveEvent;
        public UnityEvent OnStateEntered;
        public UnityEvent OnStateExited;

        private bool m_Init;

        private void Awake() {
            Init();
        }

        public void Init() {
            if (m_Init) {
                return;
            }

            m_Init = true;
            m_State.OnEntered += RaiseOnStateEntered;
            m_State.OnExited += RaiseOnStateExited;
        }

        private void RaiseOnStateEntered() {
            OnStateEntered.Invoke();
            StateActiveEvent.Invoke(true);
        }

        private void RaiseOnStateExited() {
            OnStateExited.Invoke();
            StateActiveEvent.Invoke(false);
        }

        private void OnDestroy() {
            if (m_State != null) {
                m_State.OnEntered -= OnStateEntered.Invoke;
                m_State.OnExited -= OnStateExited.Invoke;
            }
        }
    }
}
