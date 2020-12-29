using UnityEngine;
using CoreUtils.GameVariables;

namespace CoreUtils.UI {
    public class AntialiasingOptionBinding : MonoBehaviour {
        [SerializeField] private GameVariableBool m_GameVariable;
        [SerializeField] private GameVariableInt m_QualityLevel;

        private static int AntiAliasingAmount => QualitySettings.GetQualityLevel() > 2 ? 4 : 2;

        private void Start() {
            UpdateSetting(m_GameVariable.Value);
            m_GameVariable.Changed += OnChanged;
            m_QualityLevel.Changed += OnQualityChanged;
        }

        private static void OnQualityChanged(int obj) {
            UpdateSetting(QualitySettings.antiAliasing > 0);
        }

        private static void OnChanged(bool value) {
            UpdateSetting(value);
        }

        private static void UpdateSetting(bool on) {
            QualitySettings.antiAliasing = on ? AntiAliasingAmount : 0;
        }
    }
}