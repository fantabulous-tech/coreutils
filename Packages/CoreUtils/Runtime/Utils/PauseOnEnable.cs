using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreUtils {
    public class PauseOnEnable : MonoBehaviour {
        private static readonly List<PauseOnEnable> s_ActivePauses = new List<PauseOnEnable>();

        [SerializeField] private float m_PauseSpeed = 0.0001f;

        private void OnEnable() {
            if (!s_ActivePauses.Contains(this)) {
                s_ActivePauses.Add(this);
                UpdatePause();
            } else {
                Debug.LogError("PauseOnEnable tried to add a pause that already exists.", this);
            }
        }

        private void OnDisable() {
            if (s_ActivePauses.Contains(this)) {
                s_ActivePauses.Remove(this);
                UpdatePause();
            } else {
                Debug.LogError("PauseOnEnable tried to remove a pause that didn't exist.", this);
            }
        }

        private void UpdatePause() {
            PauseOnEnable lastPause = s_ActivePauses.LastOrDefault();
            Time.timeScale = lastPause ? lastPause.m_PauseSpeed : 1;
        }
    }
}