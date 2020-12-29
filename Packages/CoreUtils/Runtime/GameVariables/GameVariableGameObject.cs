using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/GameObject")]
    public class GameVariableGameObject : BaseGameVariable<GameVariableGameObject, GameObject> {
        protected override GameObject Parse(string stringValue) {
            throw new System.NotImplementedException();
        }
    }
}