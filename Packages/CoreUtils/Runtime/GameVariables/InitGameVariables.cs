using CoreUtils.AssetBuckets;
using UnityEngine;

namespace CoreUtils.GameVariables {
    public class InitGameVariables : MonoBehaviour {
        [SerializeField, AutoFillAsset(DefaultName = "Game Variable Bucket")]
        private GameVariableBucket m_GameVariableBucket;

        private void Awake() {
            foreach (BaseGameVariable gameVariable in m_GameVariableBucket.Items) {
                if (gameVariable != null) {
                    gameVariable.TryInit();
                }
            }
        }
    }
}