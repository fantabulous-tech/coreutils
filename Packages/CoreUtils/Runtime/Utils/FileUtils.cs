using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CoreUtils {
    public static class FileUtils {
        public static void WriteAllText(string path, string contents) {
            CreateFoldersFor(path);
            File.WriteAllText(path, contents);
        }

        public static void CreateFoldersFor(string path) {
            if (path.IsNullOrEmpty()) {
                Debug.LogWarning("Can't make a directory for an empty path.");
                return;
            }

            string folder = Path.GetDirectoryName(path);

            if (folder.IsNullOrEmpty() || Directory.Exists(folder)) {
                return;
            }

            Directory.CreateDirectory(folder);
        }

        public static string CleanPath(string path) {
            return path.Replace('\\', '/');
        }

        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, string searchPatterns, SearchOption searchOption = SearchOption.TopDirectoryOnly) {
            if (searchPatterns == null || searchPatterns.Contains("*.*") || searchPatterns == "*") {
                return dir.EnumerateFiles("*", searchOption);
            }

            string[] searchList = searchPatterns.SplitRegex(@"(\s+)?[,;](\s+)?");

            return searchList.SelectMany(s => dir.GetFiles(s, searchOption)).Distinct(new FileInfoComparer());
        }

        private class FileInfoComparer : IEqualityComparer<FileInfo> {
            public bool Equals(FileInfo x, FileInfo y) {
                return x == null && y == null || x != null && y != null && x.FullName.Equals(y.FullName, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(FileInfo file) {
                return file.FullName.GetHashCode();
            }
        }
    }
}