using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    [UsedImplicitly]
    public static class CreateEditorScriptContextMenu {
        [MenuItem("Assets/Create/C# Editor Script", true)]
        private static bool ValidateCreateEditorScript() {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Path.GetExtension(path) != ".cs") {
                return false;
            }
            string editorPath = GetEditorPath(path);
            return !File.Exists(editorPath);
        }

        [MenuItem("Assets/Create/C# Editor Script", false, 82)]
        private static void CreateEditorScript() {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string className = Path.GetFileNameWithoutExtension(path);
            string classEditorName = className + "Editor";
            string editorPath = GetEditorPath(path);

            if (!EditorUtility.DisplayDialog("Confirm script creation", string.Format("Create editor script \"{0}\" for class \"{1}\"?", editorPath, className), "OK", "Cancel")) {
                return;
            }

            UnityUtils.CreateFoldersFor(editorPath);

            // Try and extract namespace from existing script
            TextReader reader = new StreamReader(path);
            string textLine = reader.ReadLine();
            string namespaceLine = null;
            while (textLine != null) {
                if (textLine.Contains("namespace")) {
                    namespaceLine = textLine;
                    break;
                }

                textLine = reader.ReadLine();
            }

            reader.Close();

            if (namespaceLine == null) {
                namespaceLine = "namespace CoreUtils.Editor {";
            }

            // Write editor script
            TextWriter writer = new StreamWriter(editorPath);

            writer.Write(@"using CoreUtils;
using CoreUtils.Editor;
using UnityEditor;

{0}
	[CustomEditor(typeof({1}))]
	public class {2} : Editor<{1}> {{
		public override void OnInspectorGUI() {{
			base.OnInspectorGUI();
		}}
	}}
}}", namespaceLine, className, classEditorName);
            writer.Close();

            AssetDatabase.ImportAsset(editorPath);
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(editorPath));
        }

        private static string GetEditorPath(string scriptPath) {
            string dir = Path.GetDirectoryName(scriptPath).Replace('\\', '/');
            return dir + (dir.Contains("/Editor") ? "/" : "/Editor/") + Path.GetFileNameWithoutExtension(scriptPath) + "Editor.cs";
        }
    }
}