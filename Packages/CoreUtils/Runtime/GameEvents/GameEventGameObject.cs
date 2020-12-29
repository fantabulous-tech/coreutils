using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "GameEvent/GameObject", order = (int) MenuOrder.EventGameObject)]
    public class GameEventGameObject : BaseGameEvent<GameEventGameObject, GameObject> { }
}