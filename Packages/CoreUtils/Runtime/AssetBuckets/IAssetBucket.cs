using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreUtils.AssetBuckets {
    public interface IAssetBucket {
        bool ManualUpdate { get; }

#if UNITY_EDITOR
        List<Object> EDITOR_Sources { get; }

        bool EDITOR_IsValidDirectory(string path);

        bool EDITOR_IsMissingOrInvalid(string path);
#endif // UNITY_EDITOR
    }
}
