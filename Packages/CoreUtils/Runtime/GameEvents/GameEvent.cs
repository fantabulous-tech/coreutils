using UnityEngine;

namespace CoreUtils.GameEvents {
    [CreateAssetMenu(menuName = "CoreUtils/GameEvent/Generic", order = (int) MenuOrder.EventGeneric)]
    public class GameEvent : BaseGameEvent {
        protected override void RaiseDefault() {
            RaiseGeneric();
        }
    }
}