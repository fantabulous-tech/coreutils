using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils {
    /// <summary>
    ///     Be aware this will not prevent a non singleton constructor
    ///     such as `T myT = new T();`
    ///     To prevent that, add `protected T () {}` to your singleton class.<br />
    ///     NOTE: If there is a prefab with the same name as T,
    ///     it will be used instead of an empty GameObject.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T> {
        private static T s_Instance;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object s_Lock = new object();

        public static bool Exists => AppTracker.IsPlaying && !AppTracker.IsQuitting && s_Instance;

        public static T Instance {
            get {
                if (!AppTracker.IsPlaying) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                if (AppTracker.IsQuitting) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (s_Lock) {
                    if (s_Instance == null) {
                        s_Instance = (T) FindObjectOfType(typeof(T));

#if DEBUG
                        Object[] duplicates = FindObjectsOfType(typeof(T));
                        if (duplicates.Length > 1) {
                            LogDuplicateSingleton(duplicates);
                            return s_Instance;
                        }
#endif

                        if (s_Instance == null) {
                            // Support a prefab in resources that has the same name as the singleton being created.
                            GameObject singletonPrefab = Resources.Load<GameObject>(typeof(T).Name);
                            GameObject singleton = singletonPrefab ? Instantiate(singletonPrefab) : new GameObject(typeof(T).Name);
                            singleton.name = typeof(T).Name + " (singleton)";
                            s_Instance = singleton.GetOrAddComponent<T>();
                            DontDestroyOnLoad(s_Instance.gameObject);
                        } else {
                            //Debug.Log("[Singleton] Using instance already created: " + s_Instance.name);
                        }
                    }

                    return s_Instance;
                }
            }
        }

        public virtual void OnEnable() {
            if (s_Instance == null) {
                s_Instance = (T)this;
            } else if (s_Instance != this) {
                LogDuplicateSingleton(s_Instance, this);
            }
        }

        public virtual void OnDisable() {
            if (s_Instance == this) {
                s_Instance = null;
            }
        }

        private static void LogDuplicateSingleton(params Object[] allSingletons) {
            for (int i = 0; i < allSingletons.Length; i++) {
                Object singleton = allSingletons[i];
                if (i == 0) {
                    Debug.LogError($"[Singleton] Something went really wrong. Duplicate {typeof(T).Name} singleton found. Original singleton: {singleton.name}", singleton);
                } else {
                    Debug.LogError($"[Singleton] Duplicate singleton: {singleton.name}", singleton);
                }
            }
        }
    }
}