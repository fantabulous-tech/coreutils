using CoreUtils.GameVariables;
using TMPro;
using UnityEngine;

namespace CoreUtils.UI {
    public class ValueTextBinding : MonoBehaviour {
        [SerializeField] private BaseGameVariable m_RangeVariable;
        [SerializeField] private TextMeshProUGUI m_Label;

        public UnityEventString OnChanged;

        private void OnEnable() {
            m_RangeVariable.GenericEvent += OnVariableChanged;
            OnVariableChanged();
        }

        private void OnDisable() {
            if (m_RangeVariable != null) {
                m_RangeVariable.GenericEvent -= OnVariableChanged;
            }
        }

        private void OnVariableChanged() {
            if (m_Label != null) {
                m_Label.text = m_RangeVariable.ValueString;
            }
            OnChanged.Invoke(m_RangeVariable.ValueString);
        }

        private void OnValidate() {
            if (m_Label && m_RangeVariable) {
                m_Label.text = m_RangeVariable.ValueString;
            }
        }
    }
}