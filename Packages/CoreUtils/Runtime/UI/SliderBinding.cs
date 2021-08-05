using CoreUtils.GameVariables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreUtils.UI {
    public class SliderBinding : MonoBehaviour {
        [SerializeField, AutoFillAsset] private GameVariableFloatRange m_Variable;
        [SerializeField, AutoFill] private Slider m_Control;
        [SerializeField, AutoFillFromChildren] private TextMeshProUGUI m_Label;

        private float m_LastValue;

        private void Start() {
            OnVariableChanged(m_Variable.Progress);
            m_Variable.Changed += OnVariableChanged;
            if (m_Label) {
                m_Label.text = m_Variable.Name;
            }
        }

        private void OnVariableChanged(float value) {
            m_LastValue = m_Control.value = m_Variable.Progress;
        }

        private void Update() {
            if (!m_LastValue.Approximately(m_Control.value)) {
                m_LastValue = m_Variable.Progress = m_Control.value;
            }
        }

        private void OnValidate() {
            if (m_Label && m_Variable) {
                m_Label.text = m_Variable.Name;
            }
        }
    }
}