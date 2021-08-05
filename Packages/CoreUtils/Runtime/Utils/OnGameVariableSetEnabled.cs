using CoreUtils.GameVariables;
using UnityEngine;

namespace CoreUtils {
    public class OnGameVariableSetEnabled : MonoBehaviour {
        [SerializeField, AutoFillAsset] private GameVariableBool m_ToggleVariable;
        [SerializeField] private Behaviour m_Component;
        [SerializeField] private bool m_Invert;

        private void Awake() {
            if (m_ToggleVariable != null) {
                m_ToggleVariable.Changed += OnChange;
                OnChange(m_ToggleVariable.Value);
            }
        }

        private void OnDestroy() {
            if (m_ToggleVariable != null) {
                m_ToggleVariable.Changed -= OnChange;
            }
        }

        private void OnChange(bool value) {
            m_Component.enabled = m_Invert ? !value : value;
        }
    }
}