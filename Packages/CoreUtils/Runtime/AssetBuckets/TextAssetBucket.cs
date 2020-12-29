using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "Buckets/Text Asset Bucket", order = 1)]
    public class TextAssetBucket : GenericAssetBucket<TextAsset> {
#if UNITY_EDITOR
        public override bool EDITOR_CanAdd(Object asset) {
            return !Directory.Exists(AssetDatabase.GetAssetPath(asset)) && base.EDITOR_CanAdd(asset);
        }
#endif
    }
}