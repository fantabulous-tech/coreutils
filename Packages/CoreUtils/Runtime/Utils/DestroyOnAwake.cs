using UnityEngine;

namespace CoreUtils {
    public class DestroyOnAwake : MonoBehaviour {
        private void Awake() {
            UnityUtils.DestroyObject(this);
        }
    }
}