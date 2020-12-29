using UnityEngine;

namespace CoreUtils {
    public class DestroyAfterTime : MonoBehaviour {
        [SerializeField] private float m_Time = 1;

        // Start is called before the first frame update
        private void Start() {
            UnityUtils.DestroyObject(this, m_Time);
        }
    }
}