using CoreUtils.GameVariables;
using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class OnVariableBoolEvents : MonoBehaviour {
        [SerializeField] private GameVariableBool m_Bool;

        public UnityEvent OnTrue;
        public UnityEvent OnFalse;
        public UnityEventBool OnVariableChanged;

        private void Awake() {
            if (m_Bool == null) {
                Debug.LogWarningFormat(this, $"No bool assigned to {name}");
                return;
            }

            OnBoolChanged(m_Bool.Value);
            m_Bool.Changed += OnBoolChanged;
        }

        private void OnDestroy() {
            if (m_Bool != null) {
                m_Bool.Changed -= OnBoolChanged;
            }
        }

        private void OnBoolChanged(bool value) {
            if (value) {
                OnTrue.Invoke();
            } else {
                OnFalse.Invoke();
            }

            OnVariableChanged.Invoke(value);
        }
    }
}