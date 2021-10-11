using CoreUtils.GameVariables;
using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class OnVariableFloatEvents : MonoBehaviour {
        [SerializeField] private GameVariableFloat m_Float;

        public UnityEventFloat OnVariableChanged;

        private void Awake() {
            if (m_Float == null) {
                Debug.LogWarningFormat(this, $"No float assigned to {name}");
                return;
            }

            OnVariableChanged.Invoke(m_Float.Value);
            m_Float.Changed += OnVariableChanged.Invoke;
        }

        private void OnDestroy() {
            if (m_Float != null) {
                m_Float.Changed -= OnVariableChanged.Invoke;
            }
        }
    }
}