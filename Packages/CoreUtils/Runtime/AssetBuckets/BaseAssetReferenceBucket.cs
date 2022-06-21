using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace CoreUtils.AssetBuckets {

    public abstract class BaseAssetReferenceBucket : BaseBucket, IAssetBucket {
        [SerializeField] private List<Object> m_Sources;
        [SerializeField] private List<LazyAssetReference> m_AssetRefs;
        [SerializeField] private bool m_ManualUpdate;
        [SerializeField] private List<string> m_IgnoredGuids;

        public List<LazyAssetReference> AssetRefs => UnityUtils.GetOrSet(ref m_AssetRefs, () => new List<LazyAssetReference>());

        public List<string> IgnoredGuids {
            get => UnityUtils.GetOrSet(ref m_IgnoredGuids, () => new List<string>());
            set => m_IgnoredGuids = value;
        }

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

        protected virtual bool HasAsset(LazyAssetReference reference, string searchName) {
            return reference.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase);
        }

#if UNITY_EDITOR
        [NonSerialized] private List<string> m_EDITOR_SourcePaths;

        public List<Object> EDITOR_Sources => UnityUtils.GetOrSet(ref m_Sources, () => new List<Object> { null });
        public List<string> EDITOR_SourcePaths => UnityUtils.GetOrSet(ref m_EDITOR_SourcePaths, () => EDITOR_Sources.Where(o => o).Select(AssetDatabase.GetAssetPath).ToList());

        public List<string> EDITOR_Guids => AssetRefs.Select(a => a.Guid).ToList();

        public event Action EDITOR_Updated;
        public event Action<LazyAssetReference> EDITOR_PreItemRemove;
        public event Action<LazyAssetReference> EDITOR_OnItemAdded;

        public abstract void EDITOR_Clear();
        public virtual void EDITOR_Sort() {
            int Sorter(LazyAssetReference a, LazyAssetReference b) {
                return string.CompareOrdinal(a.Guid, b.Guid);
            }

            AssetRefs.Sort(Sorter);
            EDITOR_RaiseUpdated();
        }

        public abstract bool EDITOR_CanAdd(Object asset);

        public abstract string EDITOR_GetAssetUserData(Object asset);

        public bool EDITOR_TryAdd(List<string> guidsToAdd) {
            bool addedItems = false;
            foreach (string guid in guidsToAdd) {
                bool added = false;
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (CanBeType(assetPath, AssetSearchType)) {
                    Object objectToAdd = AssetDatabase.LoadAssetAtPath(assetPath, AssetSearchType);
                    if (objectToAdd != null && EDITOR_CanAdd(objectToAdd)) {
                        added = true;
                        EDITOR_ForceAdd(objectToAdd, guid);
                    }
                }

                if (added) {
                    addedItems = true;
                } else {
                    IgnoredGuids.Add(guid);
                }

                // make sure we dump what we just loaded if it wasn't already open to keep memory usage low
                EditorUtility.UnloadUnusedAssetsImmediate();
            }

            return addedItems;
        }

        public void EDITOR_ForceAdd(Object objectToAdd, string guid) {
            LazyAssetReference newLazyRef = new LazyAssetReference(objectToAdd, EDITOR_GetAssetName(objectToAdd), guid, EDITOR_GetAssetUserData(objectToAdd));
            AssetRefs.Add(newLazyRef);
            EDITOR_OnItemAdded?.Invoke(newLazyRef);
        }

        public abstract bool EDITOR_IsMissingOrInvalid(string path);
        public abstract string EDITOR_GetAssetName(Object asset);

        public int EDITOR_Remove(List<string> guids) {
            return AssetRefs.RemoveAll(a => {
                bool shouldRemove = guids.Contains(a.Guid);

                if (shouldRemove) {
                    EDITOR_PreItemRemove?.Invoke(a);
                }
                return shouldRemove;
            });
        }

        public bool EDITOR_IsValidDirectory(string path) {
            return path != null && EDITOR_SourcePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        public void EDITOR_SourcesUpdated() {
            m_EDITOR_SourcePaths = null;
        }

        public void EDITOR_RaiseUpdated() {
            EDITOR_Updated?.Invoke();
        }

        public bool EDITOR_UpdateAssets(string[] updatedFiles) {
            bool bucketUpdated = false;

            if (updatedFiles != null) {
                List<string> guidsToRemove = new List<string>();
                foreach (string updatedFile in updatedFiles) {
                    if (EDITOR_IsValidDirectory(updatedFile)) {
                        string guid = AssetDatabase.AssetPathToGUID(updatedFile);

                        bool isValid = false;

                        if (CanBeType(updatedFile, AssetSearchType)) {
                            Object objectToAdd = AssetDatabase.LoadAssetAtPath(updatedFile, AssetSearchType);

                            // if it's not in the ignored list, we'll have already added it
                            if (objectToAdd != null && EDITOR_CanAdd(objectToAdd)) {
                                isValid = true;

                                if (IgnoredGuids.Remove(guid)) {
                                    bucketUpdated = true;
                                    EDITOR_ForceAdd(objectToAdd, guid);
                                }
                            }
                        }

                        if (!isValid) {
                            guidsToRemove.Add(guid);
                            IgnoredGuids.Add(guid);
                        }

                        // make sure we dump what we just loaded if it wasn't already open to keep memory usage low
                        EditorUtility.UnloadUnusedAssetsImmediate();
                    }
                }

                if (guidsToRemove.Count != 0) {
                    if (EDITOR_Remove(guidsToRemove) != 0) {
                        bucketUpdated = true;
                    }
                }
            }

            // update any asset names that may have changed
            foreach(LazyAssetReference assetReference in AssetRefs) {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetReference.Guid);
                string name = Path.GetFileNameWithoutExtension(assetPath);
                if (assetReference.Name != name) {
                    assetReference.Name = name;
                    bucketUpdated = true;
                }
            }

            return bucketUpdated;
        }

        protected static bool CanBeType(string path, Type testType) {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);

            if (assetType == null) {
                return false;
            }

            if (assetType == testType || assetType.IsSubclassOf(testType)) {
                return true;
            }

            return assetType == typeof(GameObject) && typeof(Component).IsAssignableFrom(testType);
        }
#endif
    }
    public abstract class GenericAssetReferenceBucket<T> : BaseAssetReferenceBucket where T : Object {
        public override Type AssetType => typeof(T);
        public override Type AssetSearchType => typeof(T);

        public T[] Items {
            get {
                try {
                    return AssetRefs.Select(a => a.AssetRef.asset).Cast<T>().ToArray();
                }
                catch (Exception e) {
                    Debug.LogException(e, this);
                    throw;
                }
            }
        }

        public virtual T Get(string assetName) {
            return (T)AssetRefs.FirstOrDefault(a => a != null && a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.AssetRef.asset;
        }

        protected virtual string GetName(T asset) {
            return asset.name;
        }

#if UNITY_EDITOR
        public override void EDITOR_Clear() {
            AssetRefs.Clear();
        }

        public override bool EDITOR_IsMissingOrInvalid(string path) {
            // If this is an invalid path, then this path isn't missing.
            if (!EDITOR_IsValidDirectory(path)) {
                return false;
            }

            // If the guid already exists, then this path isn't missing.
            string guid = AssetDatabase.AssetPathToGUID(path);

            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            bool result = false;

            // If we have this asset, check to see if it has become invalid
            if (AssetRefs.Any(r => r.Guid != null && r.Guid.Equals(guid))) {
                result = !EDITOR_CanAdd(asset);
            } else {
                // we don't have this asset, check if we should add it
                result = EDITOR_CanAdd(asset);
            }

            asset = null;
            // make sure we dump what we just loaded if it wasn't already open to keep memory usage low
            EditorUtility.UnloadUnusedAssetsImmediate();
            return result;
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

        public override string EDITOR_GetAssetUserData(Object asset) {
            return null;
        }
#endif
    }

    [Serializable]
    public class LazyAssetReference {
        public string Name;
        public string Guid;
        public LazyLoadReference<Object> AssetRef;
        public string UserData;

        public LazyAssetReference(Object asset, string name, string guid, string userData) {
            AssetRef = new LazyLoadReference<Object> { asset = asset };
            Name = name;
            Guid = guid;
            UserData = userData;
        }
    }
}
