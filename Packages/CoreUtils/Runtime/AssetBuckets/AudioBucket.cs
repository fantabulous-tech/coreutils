using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(menuName = "CoreUtils/Bucket/Audio Bucket", order = (int) MenuOrder.Bucket)]
    public class AudioBucket : GenericAssetBucket<AudioClip> { }
}