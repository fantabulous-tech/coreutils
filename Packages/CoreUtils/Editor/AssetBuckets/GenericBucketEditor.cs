using CoreUtils.Editor;

namespace CoreUtils.AssetBuckets {
    public class GenericBucketEditor<TBucket, TItem> : Editor<TBucket> where TBucket : GenericBucket<TItem> where TItem : class { }
}