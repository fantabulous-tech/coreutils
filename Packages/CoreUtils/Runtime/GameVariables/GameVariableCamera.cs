using System;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/Camera", order = (int)MenuOrder.VariableObject)]
    public class GameVariableCamera : BaseGameVariable<GameVariableCamera, Camera> {
        protected override Camera Parse(string stringValue) {
            throw new NotImplementedException();
        }
    }
}
