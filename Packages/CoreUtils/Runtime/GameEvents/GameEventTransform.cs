using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/Transform", order = (int) MenuOrder.EventTransform)]
    public class GameEventTransform : BaseGameEvent<GameEventTransform, Transform> { }
}