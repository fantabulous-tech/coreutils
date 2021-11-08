using System;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/Transform", order = (int)MenuOrder.VariableObject)]
    public class GameVariableTransform : BaseGameVariable<GameVariableTransform, Transform> {
        protected override Transform Parse(string stringValue) {
            throw new NotImplementedException();
        }
    }
}
