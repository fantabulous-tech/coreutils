using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace CoreUtils {
    public class DisableInBuild : MonoBehaviour {
        private void Start() {
            Debug.LogError("DisableInBuild ran...? This should never happen.", this);
        }

#if UNITY_EDITOR
        [PostProcessScene]
        public static void OnPostprocessScene() {
            FindObjectsOfType<DisableInBuild>().ForEach(dib => {
                if (dib.enabled) {
                    dib.gameObject.SetActive(false);
                }
            });
        }
#endif
    }
}
