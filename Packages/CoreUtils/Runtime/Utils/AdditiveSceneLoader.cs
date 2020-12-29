using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreUtils {
    public class AdditiveSceneLoader : MonoBehaviour {
#if UNITY_EDITOR
        [SerializeField] private Object m_Scene;
#endif
        [SerializeField, ReadOnly] private string m_SceneName;

        protected void Awake() {
            if (!m_SceneName.IsNullOrEmpty() && !SceneManager.GetSceneByName(m_SceneName.Split('/').LastOrDefault()).IsValid()) {
                SceneManager.LoadSceneAsync(m_SceneName, LoadSceneMode.Additive);
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            m_SceneName = m_Scene ? AssetDatabase.GetAssetPath(m_Scene).Replace("Assets/", "").Replace(".unity", "") : null;
        }
#endif
    }
}