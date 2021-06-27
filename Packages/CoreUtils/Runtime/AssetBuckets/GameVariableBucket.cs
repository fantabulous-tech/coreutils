using CoreUtils.GameVariables;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    [CreateAssetMenu(fileName = "Game Variable Bucket", menuName = "CoreUtils/Bucket/Game Variable Bucket", order = (int) MenuOrder.Bucket)]
    public class GameVariableBucket : GenericAssetBucket<BaseGameVariable> { }
}