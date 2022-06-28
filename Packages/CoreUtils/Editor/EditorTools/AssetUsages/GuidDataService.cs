#if UNITY_2021_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace CoreUtils.Editor.AssetUsages {
    public static class GuidDataService {
        private static bool s_Init;

        public static event AssetImportTracker.AssetsChangedHandler Updated;

        [InitializeOnLoadMethod]
        private static void InitSearchSystem() {
            // HACK: Because the search system doesn't initialize without the search window open,
            // open and close the search window to initialize it.
            SearchService.Request("ref={aef7d7dc5c9ed5748b3d4aa3d923ab69, @path}");
            Debug.Log("Asset Usages Initialized.");
        }

        public static void Init() {
            if (s_Init || CoreUtilsSettings.DisableAssetGuidDatabase) {
                return;
            }

            s_Init = true;
            AssetImportTracker.DelayedAssetsChanged -= OnAssetsChanged;
            AssetImportTracker.DelayedAssetsChanged += OnAssetsChanged;
        }

        private static void OnAssetsChanged(AssetChanges changes) {
            if (CoreUtilsSettings.DisableAssetGuidDatabase) {
                return;
            }

            // Don't need 'MovedTo' since they are included in the 'Imported' list.
            // changes.MovedTo.ForEach(UpdateFileByPath);
            RaiseUpdated(changes);
        }

        public static void Refresh() {
            Init();
        }

        /// <summary>
        ///     Gets file entries that use the included guids.
        /// </summary>
        /// <param name="guids">Guids to search.</param>
        /// <returns>List of file entries that reference the supplied guids.</returns>
        public static List<FileEntry> LoadUsing(Guid[] guids) {
            if (!guids.Any()) {
                return new List<FileEntry>();
            }

            string[] paths = guids.Select(g => AssetDatabase.GUIDToAssetPath(g.ToString("N"))).ToArray();

            return AssetDatabase.GetDependencies(paths).Where(p => !paths.Contains(p)).OrderBy(p => p).Select(p => {
                Guid g = new Guid(AssetDatabase.AssetPathToGUID(p));
                return new FileEntry(g, p);
            }).ToList();
        }

        /// <summary>
        ///     Gets file entries of the supplied guids.
        /// </summary>
        /// <param name="guids">Guids to search.</param>
        /// <returns>List of file entries of the supplied guids.</returns>
        public static List<FileEntry> LoadFiles(Guid[] guids) {
            if (!guids.Any()) {
                return new List<FileEntry>();
            }

            return guids.Select(g => new FileEntry(g, AssetDatabase.GUIDToAssetPath(g.ToString()))).ToList();
        }

        /// <summary>
        ///     Gets file entries that are used by the included guids.
        /// </summary>
        /// <param name="guids">Guids to search.</param>
        /// <returns>List fo file entries referenced by the supplied guids.</returns>
        public static List<FileEntry> LoadUsedBy(Guid[] guids) {
            if (!guids.Any()) {
                return new List<FileEntry>();
            }

            string[] guidStrings = guids.Select(g => g.ToString("N")).ToArray();
            string[] paths = guidStrings.Select(AssetDatabase.GUIDToAssetPath).ToArray();

            // Unity Search example with guids: ref={000eb4c07b2d92c48a9c229eabcb74fb, 39f060c567dd6644a8661b91f49dfc0e, 27ca862385f96d0458e2fbeeed89abd5, @path}
            string searchText = $"ref={{{guidStrings.AggregateToString()}, @path}}";
            List<SearchItem> results = null;
            ISearchList searchList = SearchService.Request(searchText);

            bool success = RunWithTimeout(() => {
                results = searchList.Fetch().ToList();
            });

            if (!success) {
                Debug.LogWarning("Couldn't find search results. Try opening a search window first. (Ctrl+K)");
                return new List<FileEntry>();
            }

            return results
                .Select(r => r?.ToObject())
                .Select(AssetDatabase.GetAssetPath)
                .Where(p => !p.IsNullOrEmpty() && !paths.Contains(p, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .Select(path => new FileEntry(path))
                .ToList();
        }

        private static bool RunWithTimeout(Action action, float seconds = 3) {
            Task task = Task.Run(action);
            try {
                bool success = task.Wait(TimeSpan.FromSeconds(seconds));
                if (!success) {
                    throw new TimeoutException();
                }

                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public static List<FileEntryCount> GetFileReferences() {
            // TODO: Find a way to do this same search via Unity Search services.
            return new List<FileEntryCount>();
            //             return Connection.Query<FileEntryCount>(@"
            // SELECT FileEntry.*, COUNT(UsageEntry.ResourceGuid) as ReferenceCount
            // FROM FileEntry
            //     JOIN UsageEntry ON FileEntry.Guid = UsageEntry.ResourceGuid
            // GROUP BY FileEntry.Guid
            // ORDER BY ReferenceCount DESC
            // ;").ToList();
        }

        private static void RaiseUpdated(AssetChanges changes = new AssetChanges()) {
            Updated?.Invoke(changes);
        }
    }
}

#endif
