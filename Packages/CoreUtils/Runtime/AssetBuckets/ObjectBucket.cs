using System.IO;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "CoreUtils/Bucket/Generic Object Bucket", order = (int) MenuOrder.Bucket)]
    public class ObjectBucket : GenericAssetBucket<Object> {

#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            return !Directory.Exists(UnityEditor.AssetDatabase.GetAssetPath(asset)) && base.EDITOR_CanAdd(asset);
        }
#endif

    }
}