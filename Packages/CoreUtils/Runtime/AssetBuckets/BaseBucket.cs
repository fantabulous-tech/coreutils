using System;
using System.Linq;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    public abstract class BaseBucket : ScriptableObject {
        [SerializeField] private string m_BucketName;
        public string BucketName => m_BucketName;

        public abstract string[] ItemNames { get; }

        public abstract bool Has(string itemName);

        public virtual void ListOut() {
            ItemNames.ForEach(Debug.Log);
        }
    }

    public abstract class GenericBucket<T> : BaseBucket where T : class {
        [SerializeField] protected T[] m_Items;

        public T[] Items => UnityUtils.GetOrSet(ref m_Items, () => new T[0]);

        public override bool Has(string itemName) {
            return Items.Any(item => item != null && item.ToString().Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }
    }
}