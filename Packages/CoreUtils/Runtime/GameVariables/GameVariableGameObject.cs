using System;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/GameVariable/GameObject", order = (int)MenuOrder.VariableObject)]
    public class GameVariableGameObject : BaseGameVariable<GameVariableGameObject, GameObject> {
        protected override GameObject Parse(string stringValue) {
            throw new NotImplementedException();
        }
    }
}
