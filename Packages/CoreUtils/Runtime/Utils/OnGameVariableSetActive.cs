using CoreUtils.GameVariables;
using UnityEngine;

namespace CoreUtils {
    public class OnGameVariableSetActive : MonoBehaviour {
        [SerializeField] private GameVariableBool m_ToggleVariable;

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
            gameObject.SetActive(value);
        }
    }
}