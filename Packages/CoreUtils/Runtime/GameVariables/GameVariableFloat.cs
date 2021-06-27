using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/Float", order = (int) MenuOrder.VariableFloat)]
    public class GameVariableFloat : BaseGameVariable<GameVariableFloat, float> {
        protected override float Parse(string stringValue) {
            return float.Parse(stringValue);
        }

        protected override bool Equals(float a, float b) {
            return a.Approximately(b);
        }
    }
}