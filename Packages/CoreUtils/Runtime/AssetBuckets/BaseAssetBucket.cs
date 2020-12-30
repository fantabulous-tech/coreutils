using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.AssetBuckets {
    public abstract class BaseAssetBucket : BaseBucket {
        [SerializeField] private List<Object> m_Sources;
        [SerializeField] private List<AssetReference> m_AssetRefs;
        [SerializeField] private bool m_ManualUpdate;

        protected List<AssetReference> AssetRefs => UnityUtils.GetOrSet(ref m_AssetRefs, () => new List<AssetReference>());

        public abstract Type AssetType { get; }
        public abstract Type AssetSearchType { get; }

        public bool ManualUpdate {
            get => m_ManualUpdate;
            set => m_ManualUpdate = value;
        }

        public override string[] ItemNames => AssetRefs.Select(r => r.Name).ToArray();

        public override bool Has(string itemName) {
            return AssetRefs.Any(r => HasAsset(r, itemName));
        }

        protected virtual bool HasAsset(AssetReference reference, string searchName) {
            return reference.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase);
        }

#if UNITY_EDITOR
        [NonSerialized] private List<string> m_EDITOR_SourcePaths;

        public List<Object> EDITOR_Sources => UnityUtils.GetOrSet(ref m_Sources, () => new List<Object> {null});
        public List<string> EDITOR_SourcePaths => UnityUtils.GetOrSet(ref m_EDITOR_SourcePaths, () => EDITOR_Sources.Where(o => o).Select(UnityEditor.AssetDatabase.GetAssetPath).ToList());
        public List<Object> EDITOR_Objects => AssetRefs.Select(a => a.Asset).ToList();
        public event Action EDITOR_Updated;
        public abstract void EDITOR_Clear();
        public abstract void EDITOR_Sort(Comparison<Object> comparer);
        public abstract bool EDITOR_CanAdd(Object asset);
        public abstract void EDITOR_TryAdd(Object asset);
        public abstract bool EDITOR_IsMissing(string path);
        public abstract string EDITOR_GetAssetName(Object asset);

        public bool EDITOR_IsValidDirectory(string path) {
            return path != null && EDITOR_SourcePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        public void EDITOR_SourcesUpdated() {
            m_EDITOR_SourcePaths = null;
        }

        public void EDITOR_RaiseUpdated() {
            EDITOR_Updated?.Invoke();
        }
#endif
    }

    public abstract class GenericAssetBucket<T> : BaseAssetBucket where T : Object {

        public override Type AssetType => typeof(T);
        public override Type AssetSearchType => typeof(T);

        public T[] Items {
            get {
                try {
                    return AssetRefs.Select(a => a.Asset).Cast<T>().ToArray();
                }
                catch (Exception e) {
                    Debug.LogException(e, this);
                    throw;
                }
            }
        }

        public virtual T Get(string assetName) {
            return (T) AssetRefs.FirstOrDefault(a => a != null && a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.Asset;
        }

        protected virtual string GetName(T asset) {
            return asset.name;
        }

#if UNITY_EDITOR
        public override void EDITOR_Clear() {
            AssetRefs.Clear();
        }

        public override bool EDITOR_IsMissing(string path) {
            // If this is an invalid path, then this path isn't missing.
            if (!EDITOR_IsValidDirectory(path)) {
                return false;
            }

            // If the guid already exists, then this path isn't missing.
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            if (AssetRefs.Any(r => r.Guid != null && r.Guid.Equals(guid))) {
                return false;
            }

            // We now have a valid path and a missing guid, so we should load and test the asset itself.
            // If we can add the asset, then this path is missing.
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            return EDITOR_CanAdd(asset);
        }

        public override string EDITOR_GetAssetName(Object asset) {
            return asset && asset is T typedAsset ? GetName(typedAsset) : $"None ({AssetType.Name})";
        }

        private T EDITOR_GetTypedAsset(Object asset) {
            GameObject go = asset as GameObject;

            T typedAsset;

            if (go != null && typeof(Component).IsAssignableFrom(typeof(T))) {
                // Only get the component if the type is a Component.
                typedAsset = go.GetComponent<T>();
            } else {
                // Otherwise case to the desired type (even when it's a GameObject)
                typedAsset = asset as T;
            }

            return typedAsset;
        }

        public override bool EDITOR_CanAdd(Object asset) {
            return EDITOR_GetTypedAsset(asset);
        }

        public override void EDITOR_TryAdd(Object asset) {
            T typedAsset = EDITOR_GetTypedAsset(asset);

            if (typedAsset && AssetRefs.All(a => a.Asset != typedAsset)) {
                string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                AssetRefs.Add(new AssetReference(typedAsset, EDITOR_GetAssetName(typedAsset), guid));
            }
        }

        public override void EDITOR_Sort(Comparison<Object> comparer) {
            AssetRefs.Sort((a1, a2) => comparer.Invoke(a1.Asset, a2.Asset));
            EDITOR_RaiseUpdated();
        }
#endif
    }

    [Serializable]
    public class AssetReference {
        public string Name;
        public string Guid;
        public Object Asset;

        public AssetReference(Object asset, string name, string guid) {
            Asset = asset;
            Name = name.ToLower();
            Guid = guid;
        }
    }
}