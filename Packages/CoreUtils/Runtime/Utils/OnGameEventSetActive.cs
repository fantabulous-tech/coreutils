using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils {
    public class OnGameEventSetActive : MonoBehaviour {
        [SerializeField] private BaseGameEvent m_ShowCommand;
        [SerializeField] private BaseGameEvent m_HideCommand;

        private void Awake() {
            if (m_ShowCommand != null) {
                m_ShowCommand.GenericEvent += OnShow;
            } else {
                Debug.LogWarningFormat(this, "m_ShowCommand not set.");
            }
            if (m_HideCommand != null) {
                m_HideCommand.GenericEvent += OnHide;
            } else {
                Debug.LogWarningFormat(this, "m_HideCommand not set.");
            }
        }

        private void OnDestroy() {
            if (m_ShowCommand != null) {
                m_ShowCommand.GenericEvent -= OnShow;
            }
            if (m_HideCommand != null) {
                m_HideCommand.GenericEvent -= OnHide;
            }
        }

        private void OnShow() {
            gameObject.SetActive(true);
        }

        private void OnHide() {
            gameObject.SetActive(false);
        }
    }
}