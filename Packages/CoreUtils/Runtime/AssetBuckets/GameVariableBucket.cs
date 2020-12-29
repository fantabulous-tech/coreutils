using CoreUtils.GameVariables;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(fileName = "Game Variable Bucket", menuName = "Buckets/Game Variable Bucket")]
    public class GameVariableBucket : GenericAssetBucket<BaseGameVariable> { }
}