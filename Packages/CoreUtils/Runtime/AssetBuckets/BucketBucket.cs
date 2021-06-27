using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(fileName = "Buckets", menuName = "CoreUtils/Bucket/Bucket of Buckets", order = (int) MenuOrder.Bucket)]
    public class BucketBucket : GenericAssetBucket<BaseBucket> { }
}