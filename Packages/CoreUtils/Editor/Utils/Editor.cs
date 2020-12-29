using System.Linq;
using UnityEngine;

namespace CoreUtils.Editor {

    public class Editor<T> : UnityEditor.Editor where T : Object {
        private T m_TypedTarget;
        private T[] m_TypedTargets;

        protected T Target => UnityUtils.GetOrSet(ref m_TypedTarget, () => target as T);
        protected T[] Targets => UnityUtils.GetOrSet(ref m_TypedTargets, () => targets.Cast<T>().ToArray());
    }
}