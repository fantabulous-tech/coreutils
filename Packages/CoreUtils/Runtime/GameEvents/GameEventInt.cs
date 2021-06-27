using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "CoreUtils/GameEvent/Int", order = (int) MenuOrder.EventBool)]
    public class GameEventInt : BaseGameEvent<GameEventInt, int> { }
}