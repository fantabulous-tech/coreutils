using CoreUtils.GameVariables;
using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class OnGameVariableToggle : MonoBehaviour {
        [SerializeField, AutoFillAsset] private GameVariableBool m_ToggleVariable;

        public UnityEvent OnTrue;
        public UnityEvent OnFalse;
        public UnityEventBool OnChanged;

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
            if (value) {
                OnTrue.Invoke();
            } else {
                OnFalse.Invoke();
            }

            OnChanged.Invoke(value);
        }
    }
}