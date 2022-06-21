using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "CoreUtils/Bucket/Prefab Reference Bucket", order = (int)MenuOrder.Bucket)]
    public class PrefabReferenceBucket : GenericAssetReferenceBucket<GameObject> {
#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            if (!asset) {
                return false;
            }

            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(asset);
            bool rightType = prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant;
            return rightType && base.EDITOR_CanAdd(asset);
        }
#endif
    }
}
