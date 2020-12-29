using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    public class DragnetWindow : EditorWindow {
        public Texture IconAdd;
        public Texture IconDelete;
        public Texture IconTop;
        public Texture IconUp;
        public Texture IconDown;
        public Texture IconBottom;
        public Texture IconCollapseAll;
        public Texture IconExpandAll;

        private static DragnetWindow s_Instance;
        private BaseDragnetConfig m_SelectedConfig;

        private BaseDragnetConfig[] m_Configs;
        private string[] m_ConfigNames;
        private int m_ConfigIndex;

        public static DragnetWindow Instance {
            get {
                if (s_Instance) {
                    return s_Instance;
                }

                Init();
                return s_Instance;
            }
        }

        public BaseDragnetConfig Config {
            get => m_SelectedConfig ? m_SelectedConfig : m_SelectedConfig = CreateInstance<UnityDragnetConfig>();
            set {
                if (m_SelectedConfig != null) {
                    m_SelectedConfig.AddRuleEvent -= OnRuleChange;
                    m_SelectedConfig.RemoveRuleEvent -= OnRuleChange;
                    m_SelectedConfig.MoveRuleEvent -= OnRuleMoved;
                }

                m_SelectedConfig = value;

                if (m_SelectedConfig != null) {
                    m_SelectedConfig.AddRuleEvent += OnRuleChange;
                    m_SelectedConfig.RemoveRuleEvent += OnRuleChange;
                    m_SelectedConfig.MoveRuleEvent += OnRuleMoved;
                }
            }
        }

        private void OnRuleMoved(DragnetRule rule, Direction direction) {
            Repaint();
        }

        private void OnRuleChange(DragnetRule rule) {
            Repaint();
        }

        [MenuItem("Window/Asset Dragnet")]
        private static void Init() {
            Init(null);
        }

        public static void Init(BaseDragnetConfig config) {
            if (!s_Instance) {
                s_Instance = (DragnetWindow) GetWindow(typeof(DragnetWindow), false, "Asset Dragnet");
            }

            s_Instance.LoadConfigs(config);
        }

        private void LoadConfigs(BaseDragnetConfig defaultConfig) {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(BaseDragnetConfig).Name);
            m_Configs = guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<BaseDragnetConfig>)
                             .ToArray();
            m_ConfigNames = s_Instance.m_Configs.Select(c => c.name).ToArray();
            m_SelectedConfig = defaultConfig && m_Configs.Contains(defaultConfig)
                                   ? defaultConfig
                                   : s_Instance.m_Configs.FirstOrDefault();
            m_ConfigIndex = m_SelectedConfig ? m_Configs.IndexOf(m_SelectedConfig) : -1;
            Show();
        }

        private void OnGUI() {
            int lastIndex = EditorGUILayout.Popup("Configs", m_ConfigIndex, m_ConfigNames);
            if (m_ConfigIndex != lastIndex) {
                m_ConfigIndex = lastIndex;

                if (m_ConfigIndex > 0) {
                    Config = m_Configs[m_ConfigIndex];
                }
            }

            if (Config) {
                Config.OnConfigGUI(this);
            }
        }
    }
}