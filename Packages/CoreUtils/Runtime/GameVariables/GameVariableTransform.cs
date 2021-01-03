using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Transform")]
    public class GameVariableTransform : BaseGameVariable<GameVariableTransform, Transform> {
        protected override Transform Parse(string stringValue) {
            throw new System.NotImplementedException();
        }
    }
}