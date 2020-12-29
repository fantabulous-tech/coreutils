using CoreUtils.GameEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreUtils.UI {
    public class ButtonBinding : MonoBehaviour {
        [SerializeField] private BaseGameEvent m_GameEvent;
        [SerializeField] private Button m_Button;
        [SerializeField] private TextMeshProUGUI m_Label;
        [SerializeField] private bool OverrideName;

        private bool m_LastValue;

        private void Start() {
            m_Button.onClick.AddListener(OnClick);
            if (m_Label && m_GameEvent && !OverrideName) {
                m_Label.text = m_GameEvent.Name;
            }
        }

        private void OnClick() {
            if (m_GameEvent != null) {
                m_GameEvent.Raise();
            }
        }

        private void Reset() {
            m_Button = GetComponentInChildren<Button>();
            m_Label = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnValidate() {
            if (m_Label && m_GameEvent && !OverrideName) {
                m_Label.text = m_GameEvent.Name;
            }
        }
    }
}