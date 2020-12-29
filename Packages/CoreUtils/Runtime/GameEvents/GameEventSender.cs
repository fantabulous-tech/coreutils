using JetBrains.Annotations;
using UnityEngine;

namespace CoreUtils.GameEvents {
    public class GameEventSender : MonoBehaviour {
        [SerializeField] private BaseGameEvent m_Send;
        [SerializeField] private SendOn m_On;

        private enum SendOn {
            Manual,
            Awake,
            Enable,
            Disable,
            Start,
            Destroy
        }

        private void Awake() {
            if (m_On == SendOn.Awake) {
                Send();
            }
        }

        private void Start() {
            if (m_On == SendOn.Start) {
                Send();
            }
        }

        private void OnEnable() {
            if (m_On == SendOn.Enable) {
                Send();
            }
        }

        private void OnDisable() {
            if (m_On == SendOn.Disable) {
                Send();
            }
        }

        private void OnDestroy() {
            if (m_On == SendOn.Destroy) {
                Send();
            }
        }

        [UsedImplicitly]
        public void Send() {
            if (m_Send != null) {
                m_Send.Raise();
            }
        }
    }
}