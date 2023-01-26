using System.Globalization;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/Int", order = (int)MenuOrder.VariableInt)]
    public class GameVariableInt : BaseGameVariable<GameVariableInt, int> {

        public override string ValueString {
            get => GetValue().ToString(CultureInfo.InvariantCulture);
            set => Value = Parse(value);
        }

        protected override int Parse(string stringValue) {
            return int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : 0;
        }
    }
}
