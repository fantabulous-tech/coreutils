using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SQLite4Unity3d;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetUsages {
    public static class GuidDataService {
        private const string kDatabasePath = @"Library/GuidRefs.db";
        private const string kLastScanKey = "AssetUsage.LastScan";

        private static bool s_Init;
        private static SQLiteConnection s_Connection;
        private static DateTime s_LastScan;

        public static event AssetImportTracker.AssetsChangedHandler Updated;

        private static SQLiteConnection Connection => UnityUtils.GetOrSet(ref s_Connection, () => new SQLiteConnection(kDatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create));

        private enum RefreshSteps {
            [ProgressStep("Gathering Files")] GatherFiles,
            [ProgressStep("Scanning Files for References", 20)] ScanFiles,
            [ProgressStep("Removing Old Files")] RemoveOldFiles
        }

        [InitializeOnLoadMethod]
        public static void AutoInit() {
            if (Application.isPlaying) {
                return;
            }

            EditorApplication.delayCall += Init;
        }

        public static void Init() {
            if (s_Init || CoreUtilsSettings.DisableAssetGuidDatabase) {
                return;
            }

            s_Init = true;

            AssetImportTracker.DelayedAssetsChanged -= OnAssetsChanged;
            AssetImportTracker.DelayedAssetsChanged += OnAssetsChanged;

            // Test for database file.
            if (!File.Exists(kDatabasePath)) {
                Refresh();
                return;
            }

            // Test for database setup.
            List<SQLiteConnection.ColumnInfo> result = Connection.GetTableInfo(nameof(UsageEntry));

            if (result == null || result.Count == 0) {
                Refresh();
                return;
            }

            // Test for daily refresh.
            string lastScan = EditorPrefs.GetString(kLastScanKey);

            if (!CoreUtilsSettings.DisableWeeklyScans) {
                if (DateTime.TryParse(lastScan, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastScanTime)) {
                    s_LastScan = lastScanTime;
                } else {
                    s_LastScan = DateTime.MinValue;
                }

                if (s_LastScan < DateTime.Today - TimeSpan.FromDays(7)) {
                    Refresh();
                }
            }

            AssetImportTracker.DelayedAssetsChanged -= OnAssetsChanged;
            AssetImportTracker.DelayedAssetsChanged += OnAssetsChanged;
        }

        private static void OnAssetsChanged(AssetChanges changes) {
            if (CoreUtilsSettings.DisableAssetGuidDatabase) {
                return;
            }

            Connection.BeginTransaction();

            changes.Deleted.ForEach(RemoveFileByPath);
            changes.MovedFrom.ForEach(RemoveFileByPath);
            changes.Imported.ForEach(UpdateFileByPath);
            changes.MovedTo.ForEach(UpdateFileByPath);

            Connection.Commit();

            // Don't need 'MovedTo' since they are included in the 'Imported' list.
            // changes.MovedTo.ForEach(UpdateFileByPath);
            RaiseUpdated(changes);
        }

        public static void Refresh() {
            Init();
            Connection.DropTable<FileEntry>();
            Connection.DropTable<UsageEntry>();

            Debug.Log($"<color=#cc00ff>AssetUsages</color> : Refreshing DB. (Last Refresh: {s_LastScan})");

            ProgressBarEnum<RefreshSteps> mainProgress = new ProgressBarEnum<RefreshSteps>("Refreshing Guid Reference Database", true);

            try {
                Connection.CreateTable<FileEntry>();
                Connection.CreateTable<UsageEntry>();
                Connection.CreateIndex(nameof(UsageEntry), "UserGuid");
                Connection.CreateIndex(nameof(UsageEntry), "ResourceGuid");
            }
            catch (DllNotFoundException) {
                s_Connection = null;
                Debug.LogError("<color=#cc00ff>AssetUsages</color> : DB refresh failed. SQLite DLL not found. Please restart Unity.");
                return;
            }

            mainProgress.StartStep(RefreshSteps.GatherFiles);
            DateTime startTime = DateTime.Now;
            Connection.BeginTransaction();

            try {
                //ScanFilesManually(mainProgress);
                ScanAssetDatabase(mainProgress);
            }
            catch (ProgressBar.UserCancelledException) {
                Debug.LogWarning("Scan incomplete due to cancellation.");
            }

            Connection.Commit();
            s_LastScan = startTime;
            EditorPrefs.SetString(kLastScanKey, s_LastScan.ToString(CultureInfo.InvariantCulture));
            mainProgress.Done();

            RaiseUpdated();
        }

        private static void ScanAssetDatabase(ProgressBarEnum<RefreshSteps> mainProgress) {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            Dictionary<Guid, FileEntry> fileLookup = Connection.Table<FileEntry>().ToDictionary(f => f.Guid);
            ProgressBarCounted scanProgress = new ProgressBarCounted(mainProgress.StartStep(RefreshSteps.ScanFiles), allAssetPaths.Length);

            for (int i = 0; i < allAssetPaths.Length; i++) {
                string path = allAssetPaths[i];

                if (!IsValidFile(path)) {
                    continue;
                }

                scanProgress.StartStep(i, "Scanning " + Path.GetFileNameWithoutExtension(path));
                Guid fileGuid = new Guid(AssetDatabase.AssetPathToGUID(path));
                fileLookup.Remove(fileGuid);
                UpdateAssetInGuidDatabase(path, fileGuid, false);
            }

            ProgressBarCounted deleteProgress = new ProgressBarCounted(mainProgress.StartStep(RefreshSteps.RemoveOldFiles), fileLookup.Count);
            int step = 0;

            foreach (FileEntry file in fileLookup.Values) {
                deleteProgress.StartStep(step, "Deleting " + file.DisplayPath);
                RemoveFileByGuid(file.Guid, file.Path);
                step++;
            }
        }

        private static bool IsValidFile(string path) {
            return File.Exists(path) && !path.Contains(":/");
        }

        private static void UpdateFileByPath(string path) {
            string guid = AssetDatabase.AssetPathToGUID(path);
            try {
                if (!guid.IsNullOrEmpty()) {
                    UpdateAssetInGuidDatabase(path, new Guid(guid), true);
                }
            }
            catch (DllNotFoundException) {
                Debug.LogError("Unable to update paths. SQLite DLL was not found. Please restart Unity.");
            }
            catch (Exception e) {
                Debug.LogError($"Unable to update path '{path}' (guid = {guid}) because {e.Message}");
            }
        }

        private static void UpdateAssetInGuidDatabase(string path, Guid fileGuid, bool removeOldRefs) {
            if (removeOldRefs) {
                RemoveFileRefsByGuid(fileGuid);
            }

            Connection.InsertOrReplace(new FileEntry(fileGuid, path));
            AddAssetDatabaseRefs(path, fileGuid);
        }

        private static void AddAssetDatabaseRefs(string refFile, Guid refGuid) {
            string[] files = AssetDatabase.GetDependencies(refFile, false);
            IEnumerable<UsageEntry> entries = files.Select(f => new UsageEntry(refGuid, new Guid(AssetDatabase.AssetPathToGUID(f))));
            Connection.InsertAll(entries, "OR REPLACE");
        }

        private static void RemoveFileByPath(string path) {
            Connection.Query<FileEntry>($@"
DELETE
FROM FileEntry
WHERE FileEntry.Path = ""{path}""
;

DELETE
FROM UsageEntry
WHERE EXISTS
(
	SELECT *
	FROM FileEntry
	WHERE (UsageEntry.ResourceGuid = FileEntry.Guid OR UsageEntry.UserGuid = FileEntry.Guid) AND FileEntry.Path = ""{path}""
)
;");
        }

        private static void RemoveFileRefsByGuid(Guid guid) {
            Connection.Query<FileEntry>($@"
DELETE
FROM UsageEntry
WHERE UsageEntry.UserGuid = ""{guid}""
;");
            Connection.Delete<FileEntry>(guid);
        }

        private static void RemoveFileByGuid(Guid guid, string path) {
            Connection.Query<FileEntry>($@"
DELETE
FROM UsageEntry
WHERE UsageEntry.ResourceGuid = ""{guid}"" OR UsageEntry.UserGuid = ""{guid}""
;");
            Connection.Delete<FileEntry>(guid);
        }

        public static List<FileEntry> LoadUsing(Guid[] guids) {
            if (!guids.Any()) {
                return new List<FileEntry>();
            }

            string search = guids.AggregateToString(" OR ", g => "UsageEntry.UserGuid = \"" + g + "\"");

            return Connection.Query<FileEntry>($@"
SELECT FileEntry.*
FROM FileEntry
JOIN UsageEntry ON UsageEntry.ResourceGuid = FileEntry.Guid
WHERE {search}
GROUP BY FileEntry.Path
ORDER BY FileEntry.Path ASC
;").Where(f => !guids.Contains(f.Guid)).ToList();
        }

        public static List<FileEntry> LoadFiles(Guid[] guids) {
            if (!guids.Any()) {
                return new List<FileEntry>();
            }

            string search = guids.AggregateToString(" OR ", g => "Guid = \"" + g + "\"");

            return Connection.Query<FileEntry>($@"
SELECT FileEntry.*
FROM FileEntry
WHERE {search}
GROUP BY FileEntry.Path
ORDER BY FileEntry.Path ASC
;").ToList();
        }

        public static List<FileEntry> LoadUsedBy(Guid[] guids) {
            if (!guids.Any()) {
                return new List<FileEntry>();
            }

            if (guids.Length > 900) {
                int index = 0;
                List<FileEntry> collectedResults = new List<FileEntry>();
                while (index < guids.Length) {
                    collectedResults.AddRange(LoadUsedBy(guids.Skip(index).Take(900).ToArray()));
                    index += 900;
                }
                return collectedResults; //.Where(f => !guids.Contains(f.Guid)).ToList();
            }

            string search = guids.AggregateToString(" OR ", g => "UsageEntry.ResourceGuid = \"" + g + "\"");

            return Connection.Query<FileEntry>($@"
SELECT FileEntry.*
FROM FileEntry
JOIN UsageEntry ON UsageEntry.UserGuid = FileEntry.Guid
WHERE {search}
GROUP BY FileEntry.Path
ORDER BY FileEntry.Path ASC
;").Where(f => !guids.Contains(f.Guid)).ToList();
        }

        public static List<FileEntryCount> GetFileReferences() {
            return Connection.Query<FileEntryCount>($@"
SELECT FileEntry.*, COUNT(UsageEntry.ResourceGuid) as ReferenceCount
FROM FileEntry
    JOIN UsageEntry ON FileEntry.Guid = UsageEntry.ResourceGuid
GROUP BY FileEntry.Guid
ORDER BY ReferenceCount DESC
;").ToList();
        }

        private static void RaiseUpdated(AssetChanges changes = new AssetChanges()) {
            Updated?.Invoke(changes);
        }
    }
}
