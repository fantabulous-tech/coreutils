using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/Int", order = (int) MenuOrder.VariableInt)]
    public class GameVariableInt : BaseGameVariable<GameVariableInt, int> {
        protected override int Parse(string stringValue) {
            return int.TryParse(stringValue, out int result) ? result : 0;
        }
    }
}