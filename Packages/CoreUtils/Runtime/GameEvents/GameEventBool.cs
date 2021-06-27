using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "CoreUtils/GameEvent/Bool", order = (int) MenuOrder.EventBool)]
    public class GameEventBool : BaseGameEvent<GameEventBool, bool> { }
}