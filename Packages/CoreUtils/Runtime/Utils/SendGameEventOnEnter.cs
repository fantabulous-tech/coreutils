using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils {
    public class SendGameEventOnEnter : MonoBehaviour {
        [SerializeField] private GameEvent m_Send;

        private void OnTriggerEnter(Collider other) {
            if (m_Send != null) {
                m_Send.Raise();
            }
        }
    }
}