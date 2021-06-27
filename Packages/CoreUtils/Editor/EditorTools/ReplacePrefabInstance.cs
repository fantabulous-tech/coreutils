using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public static class ReplacePrefabInstance {
        [MenuItem("Tools/CoreUtils/Replace Prefab Instance", true, (int) MenuOrder.GameObject)]
        public static bool ValidateReplacePrefabInstanceMenuItem() {
            if (Selection.objects.Length == Selection.gameObjects.Length) {
                GameObject prefab = GetPrefabOnly(Selection.gameObjects);
                GameObject[] sceneObjects = GetSceneObjectsOnly(Selection.gameObjects);

                if (prefab != null && sceneObjects.Length > 0 && sceneObjects.Length == Selection.gameObjects.Length - 1) {
                    return true;
                }
            }

            return false;
        }

        [MenuItem("Tools/CoreUtils/Replace Prefab Instance %&r", false, (int) MenuOrder.GameObject)]
        public static void ReplacePrefabInstanceMenuItem() {
            if (Selection.objects.Length == Selection.gameObjects.Length) {
                GameObject prefab = GetPrefabOnly(Selection.gameObjects);
                GameObject[] sceneObjects = GetSceneObjectsOnly(Selection.gameObjects);

                if (prefab != null && sceneObjects.Length > 0 && sceneObjects.Length == Selection.gameObjects.Length - 1) {
                    List<GameObject> toDestroy = new List<GameObject>();
                    List<Object> newSelection = new List<Object>();

                    newSelection.Add(prefab);

                    Undo.SetCurrentGroupName(string.Format("Replace {0} objects with {1}", sceneObjects.Length, prefab.name));
                    int undoGroupIndex = Undo.GetCurrentGroup();

                    foreach (GameObject go in sceneObjects) {
                        Transform parent = go.transform.parent;
                        GameObject instance = null;
                        if (parent != null) {
                            instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                        } else {
                            instance = PrefabUtility.InstantiatePrefab(prefab, go.scene) as GameObject;
                        }

                        if (instance != null) {
                            instance.transform.localPosition = go.transform.localPosition;
                            instance.transform.localRotation = go.transform.localRotation;
                            instance.transform.localScale = go.transform.localScale;
                            Undo.RegisterCreatedObjectUndo(instance, "Instantiated replacement prefab");
                            toDestroy.Add(go);
                            newSelection.Add(instance);
                        } else {
                            Debug.LogWarningFormat(go, "Failed to Replace {0} with {1}", go.name, prefab.name);
                        }
                    }

                    foreach (GameObject go in toDestroy) {
                        Undo.DestroyObjectImmediate(go);
                    }

                    Undo.CollapseUndoOperations(undoGroupIndex);

                    Selection.objects = newSelection.ToArray();
                }
            }
        }

        //Returns a single prefab if and only if it is the only one in the selection
        private static GameObject GetPrefabOnly(GameObject[] gameObjects) {
            List<GameObject> prefabs = new List<GameObject>();
            foreach (GameObject go in gameObjects) {
                if (go != null) {
                    PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(go);
                    if (prefabType == PrefabAssetType.Model || prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant) {
                        PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(go);
                        if (status == PrefabInstanceStatus.NotAPrefab) {
                            string path = AssetDatabase.GetAssetPath(go);
                            if (!string.IsNullOrEmpty(path)) {
                                prefabs.Add(go);
                            }
                        }
                    }
                }
            }

            // Only return anything if there is one and only one prefab
            if (prefabs.Count == 1) {
                return prefabs[0];
            }

            return null;
        }

        // returns gameobjects that exist in the scene
        private static GameObject[] GetSceneObjectsOnly(GameObject[] gameObjects) {
            List<GameObject> sceneObjects = new List<GameObject>();
            foreach (GameObject go in gameObjects) {
                string path = AssetDatabase.GetAssetPath(go);
                if (string.IsNullOrEmpty(path)) {
                    if (go.scene.IsValid()) {
                        sceneObjects.Add(go);
                    }
                }
            }

            return sceneObjects.ToArray();
        }
    }
}