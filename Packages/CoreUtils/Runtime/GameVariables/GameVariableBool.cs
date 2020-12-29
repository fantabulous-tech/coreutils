using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Bool", order = (int) MenuOrder.VariableBool)]
    public class GameVariableBool : BaseGameVariable<GameVariableBool, bool> {
        protected override bool Parse(string stringValue) {
            return stringValue.IsNullOrEmpty() ? m_InitialValue : bool.TryParse(stringValue, out bool result) && result;
        }
    }
}