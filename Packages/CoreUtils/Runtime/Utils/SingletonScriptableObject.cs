using System.Linq;
using UnityEngine;

namespace CoreUtils {
    public abstract class SingletonScriptableObject<thisT> : ScriptableObject where thisT : ScriptableObject {
        private static thisT s_Instance;

        protected static thisT Instance {
            get {
                if (!s_Instance) {
                    s_Instance = Resources.FindObjectsOfTypeAll<thisT>().FirstOrDefault() ?? CreateInstance<thisT>();
                }
                return s_Instance;
            }
        }
    }
}