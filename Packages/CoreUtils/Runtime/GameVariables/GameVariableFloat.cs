using System.Globalization;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/Float", order = (int)MenuOrder.VariableFloat)]
    public class GameVariableFloat : BaseGameVariable<GameVariableFloat, float> {

        public override string ValueString {
            get => GetValue().ToString(CultureInfo.InvariantCulture);
            set => Value = Parse(value);
        }

        protected override float Parse(string stringValue) {
            return float.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        protected override bool Equals(float a, float b) {
            return a.Approximately(b);
        }
    }
}
