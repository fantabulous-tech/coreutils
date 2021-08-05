using CoreUtils.GameVariables;
using TMPro;
using UnityEngine;

namespace CoreUtils.UI {
    public class ValueTextBinding : MonoBehaviour {
        [SerializeField, AutoFillAsset] private BaseGameVariable m_Variable;
        [SerializeField, AutoFill] private TextMeshProUGUI m_Label;
        [SerializeField] private string m_OptionalStringFormat;

        public UnityEventString OnChanged;

        private void OnEnable() {
            m_Variable.GenericEvent += OnVariableChanged;
            OnVariableChanged();
        }

        private void OnDisable() {
            if (m_Variable != null) {
                m_Variable.GenericEvent -= OnVariableChanged;
            }
        }

        private void OnVariableChanged() {
            UpdateText();
            OnChanged.Invoke(m_Variable.ValueString);
        }

        private void OnValidate() {
            UpdateText();
        }

        private void UpdateText() {
            if (!m_Label || !m_Variable) {
                return;
            }

            if (!m_OptionalStringFormat.IsNullOrEmpty()) {
                switch (m_Variable) {
                    case GameVariableInt gameVariableInt:
                        m_Label.text = gameVariableInt.Value.ToString(m_OptionalStringFormat);
                        return;
                    case GameVariableFloat gameVariableFloat:
                        m_Label.text = gameVariableFloat.Value.ToString(m_OptionalStringFormat);
                        return;
                }
            }

            m_Label.text = m_Variable.ValueString;
        }
    }
}