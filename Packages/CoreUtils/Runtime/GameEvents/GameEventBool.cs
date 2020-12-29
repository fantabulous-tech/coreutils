using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/Bool", order = (int) MenuOrder.EventBool)]
    public class GameEventBool : BaseGameEvent<GameEventBool, bool> { }
}