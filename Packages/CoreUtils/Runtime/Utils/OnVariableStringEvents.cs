using CoreUtils.GameVariables;
using UnityEngine;

namespace CoreUtils {
    public class OnVariableStringEvents : MonoBehaviour {
        [SerializeField, AutoFillAsset] private GameVariableString m_StringVariable;

        public UnityEventString OnEvent;

        private void OnEnable() {
            if (m_StringVariable == null) {
                Debug.LogWarning($"No variable assigned to {name}.", this);
                return;
            }

            OnEvent.Invoke(m_StringVariable.Value);
            m_StringVariable.Changed += OnEvent.Invoke;
        }

        private void OnDisable() {
            if (m_StringVariable != null) {
                m_StringVariable.Changed -= OnEvent.Invoke;
            }
        }
    }
}