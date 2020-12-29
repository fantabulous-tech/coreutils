using CoreUtils;
using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils.UI {
    public class QuitEventHandler : MonoBehaviour {
        [SerializeField] private BaseGameEvent m_QuitEvent;

        private void Start() {
            m_QuitEvent.GenericEvent += UnityUtils.Quit;
        }
    }
}