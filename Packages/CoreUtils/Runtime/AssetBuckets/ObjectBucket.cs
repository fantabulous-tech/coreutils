using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "CoreUtils/Bucket/Generic Object Bucket", order = (int)MenuOrder.Bucket)]
    public class ObjectBucket : GenericAssetBucket<Object> {
#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            return asset && !Directory.Exists(AssetDatabase.GetAssetPath(asset)) && base.EDITOR_CanAdd(asset);
        }
#endif
    }
}
