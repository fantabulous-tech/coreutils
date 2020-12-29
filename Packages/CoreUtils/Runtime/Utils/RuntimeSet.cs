using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreUtils {
    public abstract class RuntimeItem<thisT> : MonoBehaviour where thisT : RuntimeItem<thisT> {
        protected abstract RuntimeSet<thisT> GetRuntimeSet();

        private void OnEnable() {
            GetRuntimeSet().Add(this);
        }

        private void OnDisable() {
            GetRuntimeSet().Remove(this);
        }
    }

    public abstract class RuntimeSet<T> : ScriptableObject where T : RuntimeItem<T> {
        public List<RuntimeItem<T>> Items = new List<RuntimeItem<T>>();

        public event Action<RuntimeItem<T>> OnAdd;
        public event Action<RuntimeItem<T>> OnRemove;

        public void Add(RuntimeItem<T> item) {
            if (!Items.Contains(item)) {
                Items.Add(item);
                OnAdd?.Invoke(item);
            }
        }

        public void Remove(RuntimeItem<T> item) {
            if (Items.Contains(item)) {
                Items.Remove(item);
                OnRemove?.Invoke(item);
            }
        }
    }
}