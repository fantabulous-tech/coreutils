using CoreUtils.GameVariables;
using UnityEngine;

namespace CoreUtils {
    public class SetBoolOnEnable : MonoBehaviour {
        [SerializeField] private GameVariableBool m_Bool;

        private void OnEnable() {
            m_Bool.Value = true;
        }

        private void OnDisable() {
            m_Bool.Value = false;
        }
    }
}