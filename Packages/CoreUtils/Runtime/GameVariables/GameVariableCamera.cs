using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "GameVariable/Camera")]
    public class GameVariableCamera : BaseGameVariable<GameVariableCamera, Camera> {
        protected override Camera Parse(string stringValue) {
            throw new System.NotImplementedException();
        }
    }
}