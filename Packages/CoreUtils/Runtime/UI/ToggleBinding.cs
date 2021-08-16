using CoreUtils.GameVariables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreUtils.UI {
    public class ToggleBinding : MonoBehaviour {
        [SerializeField, AutoFillAsset] private GameVariableBool m_Variable;
        [SerializeField, AutoFillFromChildren] private Toggle m_Control;
        [SerializeField, AutoFillFromChildren] private TextMeshProUGUI m_Label;

        private bool m_LastValue;

        private void Start() {
            OnVariableChanged(m_Variable.Value);
            m_Variable.Changed += OnVariableChanged;
            if (m_Label) {
                m_Label.text = m_Variable.Name;
            }
        }

        private void OnVariableChanged(bool value) {
            m_LastValue = m_Control.isOn = m_Variable.Value;
        }

        private void Update() {
            if (!m_LastValue == m_Control.isOn) {
                m_LastValue = m_Variable.Value = m_Control.isOn;
            }
        }

        private void OnValidate() {
            if (m_Label && m_Variable) {
                m_Label.text = m_Variable.Name;
            }
        }
    }
}