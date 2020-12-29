using System;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    public class PrefabInstanceTracker : MonoBehaviour {
        private string m_Name;
        
        public string Name {
            get => m_Name ?? name;
            set => m_Name = value;
        }

        public event Action<string> Destroyed;

        private void OnDestroy() {
            Destroyed?.Invoke(m_Name);
        }
    }
}