using CoreUtils.AssetBuckets;
using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [DefaultExecutionOrder(-1500)]
    public class SaveLoadVariables : Singleton<SaveLoadVariables> {
        [SerializeField, AutoFillAsset] private GameVariableBucket m_GameVariablesToSave;
        [SerializeField, AutoFillAsset(CanBeNull = true)] private GameEvent m_ResetProgressEvent;

        public void Awake() {
            if (m_ResetProgressEvent) {
                m_ResetProgressEvent.GenericEvent += OnResetProgress;
            }

            LoadAll();
            SubscribeAll();
        }

        public void OnDestroy() {
            if (m_ResetProgressEvent) {
                m_ResetProgressEvent.GenericEvent -= OnResetProgress;
            }
        }

        private void OnResetProgress() {
            PlayerPrefs.DeleteAll();
            LoadAll();
        }

        public static void LoadAll() {
            Instance.m_GameVariablesToSave.Items.ForEach(Load);
        }

        public static void SubscribeAll() {
            Instance.m_GameVariablesToSave.Items.ForEach(Subscribe);
        }

        private static void Load(BaseGameVariable variable) {
            if (!variable) {
                return;
            }

            variable.TryInit();

            string value = PlayerPrefs.GetString(variable.name);

            if (!value.IsNullOrEmpty()) {
                Debug.Log($"Loading {variable} to {value}", variable);
                variable.ValueString = PlayerPrefs.GetString(variable.name);
            }
        }

        private static void Subscribe(BaseGameVariable variable) {
            if (!variable) {
                return;
            }

            variable.GenericEvent += () => Save(variable);
        }

        private static void Save(BaseGameVariable variable) {
            Debug.Log($"Saving {variable} to '{variable.ValueString}'");
            PlayerPrefs.SetString(variable.name, variable.ValueString);
            PlayerPrefs.Save();
        }
    }
}