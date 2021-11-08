using System;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreUtils.GameVariables {
    public class Storage : MonoBehaviour {
        [SerializeField] private string m_FileName = "Saves/Game01";
        [SerializeField] private BaseGameVariable[] m_SavedVariables;

        [Serializable]
        private class StorageData {
            public VariableData[] variables;
        }

        [Serializable]
        private class VariableData {
            public string name;
            public string value;

            public VariableData(BaseGameVariable variable) {
                name = variable.Name;
                value = variable.ValueString;
            }

            public void Load(BaseGameVariable variable) {
                variable.ValueString = value;
            }
        }

        private bool m_Dirty;

        private string FilePath => string.Format("{0}/{1}.json", Application.dataPath, m_FileName);

        private void Start() {
            Load();
            Subscribe();
        }

        private void Update() {
            if (m_Dirty) {
                m_Dirty = false;
                Save();
            }
        }

        private void Load() {
            if (File.Exists(FilePath)) {
                StorageData data = JsonUtility.FromJson<StorageData>(File.ReadAllText(FilePath));

                if (data == null || data.variables == null) {
                    Debug.LogWarningFormat(this, "Saved data found but not loaded. Format change?");
                    return;
                }

                if (data.variables.Length != m_SavedVariables.Length) {
                    Debug.LogErrorFormat(this, "Saved data and variable counts don't match!");
                    return;
                }

                for (int i = 0; i < data.variables.Length; i++) {
                    BaseGameVariable variable = m_SavedVariables[i];
                    VariableData variableData = data.variables[i];
                    variableData.Load(variable);
                }
            }
        }

        private void Subscribe() {
            for (int i = 0; i < m_SavedVariables.Length; i++) {
                BaseGameVariable gameVariable = m_SavedVariables[i];
                gameVariable.GenericEvent += OnVariableChanged;
            }
        }

        private void Save() {
            if (m_SavedVariables == null || m_SavedVariables.Length == 0) {
                Debug.LogWarningFormat(this, "No variables found. Save skipped.");
                return;
            }

            StorageData data = new StorageData {variables = m_SavedVariables.Select(gv => new VariableData(gv)).ToArray()};
            string saveText = JsonUtility.ToJson(data, true);
            FileUtils.WriteAllText(FilePath, saveText);

#if UNITY_EDITOR
            string assetPath = FilePath.ReplaceRegex("^.*/Assets/", "Assets/");

            if (assetPath.StartsWith("Assets/")) {
                AssetDatabase.ImportAsset(assetPath);
            }
#endif
        }

        private void OnVariableChanged() {
            m_Dirty = true;
        }
    }
}
