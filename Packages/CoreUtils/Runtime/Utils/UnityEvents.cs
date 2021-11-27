using UnityEngine;
using UnityEngine.Events;

namespace CoreUtils {
    public class UnityEvents : MonoBehaviour {
        public UnityEvent OnEnableEvent;
        public UnityEvent OnDisableEvent;
        public UnityEventBool OnEnableChangedEvent;

        private void OnEnable() {
            OnEnableEvent.Invoke();
            OnEnableChangedEvent.Invoke(true);
        }

        private void OnDisable() {
            OnDisableEvent.Invoke();
            OnEnableChangedEvent.Invoke(false);
        }
    }
}
