using UnityEngine;

namespace CoreUtils {
    /// <summary>
    ///     Update the Inspector UI with a drop-down of compatible assets based on the provided filters and automatically
    ///     assigns the first one.
    /// </summary>
    public class AutoFillAssetAttribute : PropertyAttribute {
        /// <summary>
        ///     If true, the asset can be unassigned and won't be assigned by default. The filtered drop-down will still be
        ///     available.
        /// </summary>
        public bool CanBeNull;

        /// <summary>
        ///     The optional name of the asset to assign by default, if found.<br />
        ///     If not defined, then the first asset in the list will be assigned if CanBeNull is false.
        /// </summary>
        public string DefaultName;

        /// <summary>
        ///     The filter to apply to the asset search.<br />
        ///     This asset's type ('t:') is auto-added to the search unless specified here.
        /// </summary>
        public string SearchFilter;

        /// <summary>
        ///     The folder path to use when searching for assets.
        /// </summary>
        public string SearchFolder;
    }
}