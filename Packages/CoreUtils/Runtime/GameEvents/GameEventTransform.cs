using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "CoreUtils/GameEvent/Transform", order = (int) MenuOrder.EventObject)]
    public class GameEventTransform : BaseGameEvent<GameEventTransform, Transform> { }
}