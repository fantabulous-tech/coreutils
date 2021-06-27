using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "CoreUtils/GameEvent/GameObject", order = (int) MenuOrder.EventObject)]
    public class GameEventGameObject : BaseGameEvent<GameEventGameObject, GameObject> { }
}