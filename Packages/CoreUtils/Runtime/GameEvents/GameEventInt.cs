using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/Int", order = (int) MenuOrder.EventBool)]
    public class GameEventInt : BaseGameEvent<GameEventInt, int> { }
}