using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public abstract class AttributeDrawer<T> : PropertyDrawer where T : PropertyAttribute {
        private T m_Attribute;

        protected T Attribute => UnityUtils.GetOrSet(ref m_Attribute, () => (T) attribute);
    }
}