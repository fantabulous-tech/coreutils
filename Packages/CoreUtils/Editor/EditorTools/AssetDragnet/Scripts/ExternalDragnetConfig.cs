using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor.AssetDragnet {
    [CreateAssetMenu(menuName = "AssetDragnet/ExternalDragnetConfig")]
    public class ExternalDragnetConfig : BaseDragnetConfig {
        [SerializeField] private string m_RootFolder;

        protected override string RootPath => m_RootFolder;

        protected override void TopConfigGUI() {
            GUILayout.BeginHorizontal();
            string newRootFolder = EditorGUILayout.DelayedTextField("Root Folder", m_RootFolder);

            if (GUILayout.Button("...", GUILayout.Width(25))) {
                newRootFolder = EditorUtility.OpenFolderPanel("Root Folder", m_RootFolder, "");
                GUI.FocusControl(null);
            }

            if (!newRootFolder.IsNullOrEmpty() && m_RootFolder != newRootFolder) {
                Debug.Log("New search folder: " + newRootFolder);
                Undo.RecordObject(this, "Change Root Search Folder");
                m_RootFolder = newRootFolder.Trim('/');
                RefreshAssetPaths();
            }

            GUILayout.EndHorizontal();
        }

        protected override bool DeleteAsset(string path) {
            try {
                if (Directory.Exists(path)) {
                    Directory.Delete(path);
                } else if (File.Exists(path)) {
                    File.Delete(path);
                }

                return true;
            }
            catch (Exception e) {
                Debug.Log(e);
                return false;
            }
        }

        protected override string MoveAsset(string source, string destination) {
            try {
                File.Move(source, destination);
            }
            catch (Exception e) {
                return e.Message;
            }

            return null;
        }

        protected override IEnumerable<string> GetUnfilteredAssetPaths() {
            if (!Directory.Exists(m_RootFolder)) {
                m_Error = string.Format("'{0}' does not exist.", m_RootFolder);
            }

            int trimIndex = m_RootFolder.IsNullOrEmpty() ? 1 : m_RootFolder.Length + 1;

            return !m_RootFolder.IsNullOrEmpty() && Directory.Exists(m_RootFolder)
                       ? Directory.GetFiles(m_RootFolder, "*.*", SearchOption.AllDirectories).Select(p => p.Substring(trimIndex).Replace('\\', '/'))
                       : new string[0];
        }
    }
}