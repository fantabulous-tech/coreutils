using System;
using CoreUtils.GameEvents;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CoreUtils.UI {
    public class ButtonBinding : MonoBehaviour {
        [SerializeField, AutoFillAsset(CanBeNull = true)] private BaseGameEvent m_GameEvent;
        [SerializeField, AutoFill] private Button m_Button;
        [SerializeField, AutoFillFromChildren] private TextMeshProUGUI m_Label;
        [FormerlySerializedAs("OverrideName"),SerializeField] private bool m_OverrideName;

        public object Data { get; set; }
        
        public string Text {
            get => m_Label.text;
            set {
                if (m_Label) {
                    m_Label.text = value;
                }
            }
        }

        public event Action<ButtonBinding> Clicked; 

        private void Start() {
            m_Button.onClick.AddListener(OnClick);
            if (m_Label && m_GameEvent && !m_OverrideName) {
                m_Label.text = m_GameEvent.Name;
            }
        }

        private void OnClick() {
            if (m_GameEvent != null) {
                m_GameEvent.Raise();
            }

            Clicked?.Invoke(this);
        }

        private void Reset() {
            m_Button = GetComponentInChildren<Button>();
            m_Label = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnValidate() {
            if (m_Label && m_GameEvent && !m_OverrideName) {
                m_Label.text = m_GameEvent.Name;
            }
        }
    }
}