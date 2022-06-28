using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor {
    partial class AssetResaver {
        private class Resaver {
            private string m_CurrentAssetPath;
            private bool m_Logging;
            private IEnumerator<Object> m_Enumerator;

            private HashSet<string> m_IgnoreNamespaces = new HashSet<string> { "UnityEngine", "UnityEditor", "UnityEngine.UI", };

            private Dictionary<Type, bool> m_HasCustomInspectorCache = new Dictionary<Type, bool>();
            private Dictionary<Type, bool> m_HasOnValidateCache = new Dictionary<Type, bool>();
            private Dictionary<Type, bool> m_HasPropertyDrawerersCache = new Dictionary<Type, bool>();
            private float m_AssetCount;
            private float m_Progress;
            private string m_ProgressMessage;
            private bool m_Cancel;

            public bool IsRunning {
                get { return m_Enumerator != null; }
            }
            public float Progress {
                get { return m_Progress; }
            }
            public string ProgressMessage {
                get { return m_ProgressMessage; }
            }

            public void ResaveAll() {
                if (EditorUtility.DisplayDialog("Asset Resaver",
                        "Do you want to attempt to resave EVERY asset in the project?  It will take a long time.", "Ok",
                        "Nononononono, get me out of here")) {
                    string[] paths = AssetDatabase.GetAllAssetPaths();
                    m_Enumerator = Resave(paths);
                    //EditorApplication.update += Update;
                }
            }

            public void ResaveSelection() {
                HashSet<string> paths = new HashSet<string>();
                foreach (Object o in Selection.objects) {
                    string path = AssetDatabase.GetAssetPath(o);
                    if (!string.IsNullOrEmpty(path)) {
                        paths.Add(path);
                    }
                }
                m_Enumerator = Resave(paths.ToArray());
                //EditorApplication.update += Update;
            }

            public void Update() {
                if (IsRunning) {
                    try {
                        if (!m_Enumerator.MoveNext()) {
                            // We've reached the end of the list
                            Cleanup();
                        }
                    }
                    catch (ProgressBar.UserCancelledException) {
                        Debug.Log("User Cancelled");
                        Cleanup();
                    }
                    catch {
                        // Something went wrong, bail out
                        Cleanup();
                        throw;
                    }
                }
            }

            private void Cleanup() {
                if (m_Enumerator != null) {
                    m_Enumerator.Dispose();
                    m_Enumerator = null;
                }
            }

            private IEnumerator<Object> Resave(string[] paths) {
                Object[] originalSelection = Selection.objects;

                m_Cancel = false;
                EditorApplication.LockReloadAssemblies();
                try {
                    SetPerforceEnabled(false);
                    try {
                        Application.logMessageReceived += OnLogMessage;
                        try {
                            try {
                                int groupCount = 0;
                                int groupSize = 1000;
                                m_AssetCount = paths.Length;
                                for (int i = 0; i < paths.Length; i++) {
                                    m_CurrentAssetPath = paths[i];
                                    UpdateProgress(i, m_CurrentAssetPath);
                                    if (FilterPath(m_CurrentAssetPath)) {
                                        //Debug.LogFormat("Loading from \"{0}\"",m_CurrentAssetPath);

                                        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(m_CurrentAssetPath);
                                        if (assets != null && assets.Length > 0) {
                                            foreach (Object asset in assets) {
                                                if (asset != null) {
                                                    if (NeedsInspector(asset, m_CurrentAssetPath)) {
                                                        Selection.activeObject = asset;
                                                        UpdateProgress(i, asset.FullName());
                                                        yield return null;
                                                        Selection.activeObject = null;

                                                        if (m_Cancel) {
                                                            yield break;
                                                        }
                                                    }
                                                    EditorUtility.SetDirty(asset);
                                                }
                                            }
                                        }
                                        if (groupCount++ >= groupSize) {
                                            AssetDatabase.SaveAssets();
                                            Resources.UnloadUnusedAssets();
                                            groupCount = 0;
                                        }
                                    }
                                }
                            }
                            finally {
                                UpdateProgress(paths.Length, m_CurrentAssetPath);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh(ImportAssetOptions.Default);
                            }
                        }
                        finally {
                            Application.logMessageReceived -= OnLogMessage;
                        }
                    }
                    finally {
                        SetPerforceEnabled(true);
                    }
                }
                finally {
                    EditorApplication.UnlockReloadAssemblies();
                }

                Selection.objects = originalSelection;

                EditorUtility.DisplayDialog("All done",
                    "All done, you might want to do a \"Revert Unchanged Files\" in P4V.",
                    "OK");
            }

            private void UpdateProgress(int i, string message) {
                m_Progress = i/m_AssetCount;
                m_ProgressMessage = message;
            }

            private bool NeedsInspector(Object asset, string currentAssetPath) {
                if (asset is Material) {
                    return true;
                }

                if (asset is GameObject) {
                    GameObject go = asset as GameObject;
                    if (go != null) {
                        Component[] components = go.GetComponents(typeof(Component));
                        foreach (Component component in components) {
                            if (HasInspectorOrOnValidate(component)) {
                                return true;
                            }
                        }
                    }
                } else if (asset is ScriptableObject) {
                    if (HasInspectorOrOnValidate(asset)) {
                        return true;
                    }
                }

                return false;
            }

            private bool HasInspectorOrOnValidate(Object o) { //Custom Editors can change the component inside OnGUI()
                if (HasCustomInspector(o)) {
                    return true;
                }

                // Components with an OnValidate() method can change their own contents when inspected
                if (HasOnValidateMethod(o)) {
                    return true;
                }

                // if (HasPropertyDrawers(o)) {
                //     return true;
                // }

                return false;
            }

            private bool HasCustomInspector(Object o) {
                bool hasCustomInspector = false;
                if (o != null) {
                    Type type = o.GetType();
                    if (!m_HasCustomInspectorCache.TryGetValue(type, out hasCustomInspector)) {
                        if (!m_IgnoreNamespaces.Contains(type.Namespace)) {
                            hasCustomInspector = ActiveEditorTracker.HasCustomEditor(o);
                            //XDebug.ConditionalLog(hasCustomInspector, "Has Custom Editor {0}", o.GetType().FullName);
                        }

                        m_HasCustomInspectorCache.Add(type, hasCustomInspector);
                    }
                }
                return hasCustomInspector;
            }

            private bool HasOnValidateMethod(Object o) {
                bool hasOnValidate = false;
                if (o != null) {
                    Type type = o.GetType();
                    if (!m_HasOnValidateCache.TryGetValue(type, out hasOnValidate)) {
                        MethodInfo method = type.GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        hasOnValidate = method != null;
                        //XDebug.ConditionalLog(hasOnValidate, "Has OnValidate() {0}", o.GetType().FullName);
                        m_HasOnValidateCache.Add(type, hasOnValidate);
                    }
                }
                return hasOnValidate;
            }

            // private bool HasPropertyDrawers(Object o) {
            //     bool hasPropertyDrawerers = false;
            //     if (o != null) {
            //         Type type = o.GetType();
            //         if (!m_HasPropertyDrawerersCache.TryGetValue(type, out hasPropertyDrawerers)) {
            //             SerializedObject so = new SerializedObject(o);
            //             so.Update();
            //             SerializedProperty p = so.GetIterator();
            //             while (p.Next(true)) {
            //                 if (HasPropertyDrawers(p)) {
            //                     hasPropertyDrawerers = true;
            //                     break;
            //                 }
            //             }
            //             m_HasPropertyDrawerersCache.Add(type, hasPropertyDrawerers);
            //         }
            //     }
            //     return hasPropertyDrawerers;
            // }

            // private MethodInfo m_GetHandlerMethod;
            // private PropertyInfo m_HasDrawerProperty;

            // private bool HasPropertyDrawers(SerializedProperty p) {
            //     if (m_GetHandlerMethod == null) {
            //         Assembly editorAssembly = UnityEditor.Compilation.Assembly typeof(ScriptCompiler).Assembly;
            //         Type utilityType = editorAssembly.GetType("UnityEditor.ScriptAttributeUtility");
            //         Debug.Assert(utilityType != null, "Failed to find UnityEditor.ScriptAttributeUtility");
            //         m_GetHandlerMethod = utilityType.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic);
            //     }
            //
            //     if (m_HasDrawerProperty == null) {
            //         Assembly editorAssembly = typeof(ScriptCompiler).Assembly;
            //         Type handlerType = editorAssembly.GetType("UnityEditor.PropertyHandler");
            //         Debug.Assert(handlerType != null, "Failed to find UnityEditor.PropertyHandler");
            //         m_HasDrawerProperty = handlerType.GetProperty("hasPropertyDrawer", BindingFlags.Instance | BindingFlags.Public);
            //     }
            //
            //     Debug.Assert(m_GetHandlerMethod != null, "Failed to find ScriptAttributeUtility.GetHandler");
            //     Debug.Assert(m_HasDrawerProperty != null, "Failed to find PropertyHandler.GetHandler");
            //     if (m_GetHandlerMethod == null || m_HasDrawerProperty == null) {
            //         return false;
            //     }
            //     object handler = m_GetHandlerMethod.Invoke(null, new object[] { p });
            //
            //     if (handler == null) {
            //         return false;
            //     }
            //
            //     object result = m_HasDrawerProperty.GetValue(handler, null);
            //
            //     if (result == null) {
            //         return false;
            //     }
            //
            //     return (bool)result;
            // }

            private void SetPerforceEnabled(bool enabled) {
                if (enabled) {
                    Debug.Log("Enabling Perforce");
#if UNITY_2021_1_OR_NEWER
                    VersionControlSettings.mode = "Perforce";
#else
                    EditorSettings.externalVersionControl = "Perforce";
#endif
                    Task connectToPerforce = Provider.UpdateSettings();
                    connectToPerforce.Wait();

                    ReconcilePerforceOfflineWork();
                } else {
                    // Disable Perforce;
                    Debug.Log("Disabling Perforce");
#if UNITY_2021_1_OR_NEWER
                    VersionControlSettings.mode = "Visible Meta Files";
#else
                    EditorSettings.externalVersionControl = "Visible Meta Files";
#endif
                    Task updateSettings = Provider.UpdateSettings();
                    updateSettings.Wait();
                }
            }

            private static void ReconcilePerforceOfflineWork() {
                // Reconcile offline work to figure out what went down during bake
                string directoryFullPath = Application.dataPath;
                Process process = new Process();
                process.StartInfo.FileName = "p4";
                string arguments = "";
                arguments += "-c " + EditorUserSettings.GetConfigValue("vcPerforceWorkspace");
                arguments += " -u " + EditorUserSettings.GetConfigValue("vcPerforceUsername");
                arguments += " -p " + EditorUserSettings.GetConfigValue("vcPerforceServer");
                arguments += " reconcile " + directoryFullPath + "/...";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                StringBuilder standardOutput = new StringBuilder();
                StringBuilder standardError = new StringBuilder();
                process.OutputDataReceived += (sender, args) => standardOutput.AppendLine(args.Data);
                process.ErrorDataReceived += (sender, args) => standardError.AppendLine(args.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();

                if (standardError.ToString().Trim() != "") {
                    Debug.LogError("Resaver Reconcile Changes Failed: " + standardError);
                }
                if (standardOutput.ToString().Trim() != "") {
                    Debug.Log(standardOutput.ToString());
                }
            }

            private bool FilterPath(string path) {
                if (path.StartsWith("Assets")) {
                    string extension = Path.GetExtension(path).ToLower();
                    if (extension == ".unity") {
                        // ignore scene files (Maybe we shouldn't? but that would be a lot more work to load and resave the scene).
                        return false;
                    }
                    // ignore script changes. There *might* be changes to their .meta files (which we'll ignore),
                    // but the scripts themselves aren't going to change.
                    if (extension == ".cs") {
                        return false;
                    }
                    if (extension == ".dll") {
                        return false;
                    }
                    if (extension == ".shader") {
                        return false;
                    }
                    if (extension == ".cginc") {
                        return false;
                    }
                } else {
                    // ignore files that are outside of the Assets folder structure, these will probably be Unity built in Dlls
                    return false;
                }
                return true;
            }

            private void OnLogMessage(string condition, string stacktrace, LogType type) {
                if (!m_Logging) {
                    m_Logging = true;
                    try {
                        Debug.LogFormat("Unexpected log message while processing \"{0}\"", m_CurrentAssetPath);
                    }
                    finally {
                        m_Logging = false;
                    }
                }
            }

            public void Cancel() {
                if (m_Enumerator != null) {
                    m_Cancel = true;
                    m_Enumerator.MoveNext();
                    m_Enumerator = null;
                }
            }
        }
    }
}
