using CoreUtils.GameVariables;
using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class OnFloatEventToBool : MonoBehaviour {
        [SerializeField] private GameVariableFloat m_Float;
        [SerializeField, Range(0, 1)] private float m_MidPoint = 0.5f;

        public UnityEvent OnTrue;
        public UnityEvent OnFalse;

        private void OnEnable() {
            if (m_Float == null) {
                Debug.LogWarningFormat(this, $"No bool assigned to {name}");
                return;
            }

            m_Float.Changed += OnFloatChanged;
            OnFloatChanged(m_Float.Value);
        }

        private void OnDisable() {
            if (m_Float != null) {
                m_Float.Changed -= OnFloatChanged;
            }
        }

        private void OnFloatChanged(float value) {
            if (value >= m_MidPoint) {
                OnTrue.Invoke();
            } else {
                OnFalse.Invoke();
            }
        }
    }
}