using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "CoreUtils/Bucket/Prefab Bucket", order = (int) MenuOrder.Bucket)]
    public class PrefabBucket : GenericAssetBucket<GameObject> {

#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            UnityEditor.PrefabAssetType prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(asset);
            bool rightType = prefabType == UnityEditor.PrefabAssetType.Regular || prefabType == UnityEditor.PrefabAssetType.Variant;
            return rightType && base.EDITOR_CanAdd(asset);
        }
#endif
    }
}