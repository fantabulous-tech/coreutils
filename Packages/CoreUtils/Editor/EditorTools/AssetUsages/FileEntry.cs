using System;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SQLite4Unity3d;
using UnityEditor;

namespace CoreUtils.Editor.AssetUsages {
    [Serializable]
    public class FileEntry {
        [PrimaryKey, UsedImplicitly]
        public Guid Guid { get; set; }

        [UsedImplicitly]
        public string Path { get; set; }

        private string m_DisplayPath;

        public string DisplayPath => m_DisplayPath ?? (m_DisplayPath = Path.ReplaceRegex("^.*Assets/", "", RegexOptions.IgnoreCase));

        public bool Exists => File.Exists(Path);

        public string GuidString => Guid.ToString("N");

        public FileEntry() { }

        public FileEntry(Guid guid, string path) {
            Guid = guid;
            Path = path;
        }

        public FileEntry(Guid guid) {
            Guid = guid;
            Path = AssetDatabase.GUIDToAssetPath(guid.ToString("N"));
        }

        public FileEntry(string path) {
            Guid = new Guid(AssetDatabase.AssetPathToGUID(path));
            Path = path;
        }

        public override string ToString() {
            return $"[File: Guid={Guid}, Path={Path}]";
        }
    }

    public class FileEntryCount : FileEntry {
        [UsedImplicitly]
        public int ReferenceCount { get; set; }
    }
}
