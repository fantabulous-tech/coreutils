using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "CoreUtils/GameEvent/String", order = (int) MenuOrder.EventString)]
    public class GameEventString : BaseGameEvent<GameEventString, string> { }
}