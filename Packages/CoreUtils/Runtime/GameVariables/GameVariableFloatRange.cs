using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/FloatRange", order = (int) MenuOrder.VariableFloatRange)]
    public class GameVariableFloatRange : GameVariableFloat {
        [SerializeField] private float m_MinValue;
        [SerializeField] private float m_MaxValue = 1;

        public float Progress {
            get => (Value - m_MinValue)/(m_MaxValue - m_MinValue);
            set => Value = Mathf.Clamp(value*(m_MaxValue - m_MinValue) + m_MinValue, m_MinValue, m_MaxValue);
        }

        protected override void SetValue(float value) {
            base.SetValue(Mathf.Clamp(value, m_MinValue, m_MaxValue));
        }

        protected override void OnValidate() {
            base.OnValidate();
            m_InitialValue = Mathf.Clamp(m_InitialValue, m_MinValue, m_MaxValue);
            Value = Mathf.Clamp(Value, m_MinValue, m_MaxValue);
            m_MinValue = Mathf.Min(m_MinValue, m_MaxValue);
        }
    }
}