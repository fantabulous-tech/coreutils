using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/String", order = (int) MenuOrder.VariableString)]
    public class GameVariableString : BaseGameVariable<GameVariableString, string> {
        protected override string Parse(string stringValue) {
            return stringValue;
        }
    }
}