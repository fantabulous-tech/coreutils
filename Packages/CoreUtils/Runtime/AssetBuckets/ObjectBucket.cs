using System.IO;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "Buckets/Generic Object Bucket", order = 1)]
    public class ObjectBucket : GenericAssetBucket<Object> {

#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            return !Directory.Exists(UnityEditor.AssetDatabase.GetAssetPath(asset)) && base.EDITOR_CanAdd(asset);
        }
#endif

    }
}