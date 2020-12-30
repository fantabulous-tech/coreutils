using CoreUtils.GameVariables;
using UnityEngine;

namespace CoreUtils {
    public class SetGameEventBoolWhenEnabled : MonoBehaviour {
        [SerializeField] private GameVariableBool m_Bool;
        [SerializeField] private bool m_Value;
        [SerializeField] private float m_CheckRate = -1;

        private float m_NextCheck;

        private void OnEnable() {
            m_Bool.Value = m_Value;
            m_NextCheck = Time.time + m_CheckRate;
        }

        private void Update() {
            if (m_CheckRate < 0 || m_NextCheck > Time.time) {
                return;
            }

            m_NextCheck = Time.time + m_CheckRate;

            if (m_Bool.Value != m_Value) {
                m_Bool.Value = m_Value;
            }
        }
    }
}