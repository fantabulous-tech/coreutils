using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreUtils.Editor.AssetUsages;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    [CustomPropertyDrawer(typeof(AutoFillAssetAttribute))]
    public class AutoFillAssetDrawer : AttributeDrawer<AutoFillAssetAttribute> {
        private GUIContent[] m_Labels;
        private AssetListItem[] m_AssetListItems;
        private bool m_Init;
        private Type m_PropertyType;
        private string m_SearchString;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            const int kDropDownWidth = 18;
            const int kMargin = 4;

            if (!m_Init) {
                FindAssets(property);
            }

            Color originalColor = GUI.color;
            EditorGUI.BeginProperty(position, label, property);

            Rect objectFieldRect = new Rect(position.x, position.y, position.width - kDropDownWidth - kMargin, position.height);
            Rect dropDownRect = new Rect(position.x + objectFieldRect.width + kMargin, position.y, kDropDownWidth, position.height);
            EditorGUI.PropertyField(objectFieldRect, property, label);

            if (GUI.Button(dropDownRect, new GUIContent(EditorGUIUtility.IconContent("d_icon dropdown")))) {
                int selectedIndex = -1;
                if (property.objectReferenceValue && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out string guid, out long _)) {
                    selectedIndex = m_AssetListItems.IndexOf(r => r.Guid == guid);
                }
                SearchablePopup.ShowAsPopup(dropDownRect, m_Labels, selectedIndex, i => OnNewSelection(i, property), m_SearchString, s => m_SearchString = s);
            }

            EditorGUI.EndProperty();
            GUI.color = originalColor;
        }

        private void OnNewSelection(int newIndex, SerializedProperty property) {
            if (newIndex >= 0 && newIndex < m_AssetListItems.Length) {
                property.objectReferenceValue = EditorUtils.GetAssetFromGuid(m_AssetListItems[newIndex].Guid, m_PropertyType);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void FindAssets(SerializedProperty property) {
            m_Init = true;
            int selectedIndex = -1;
            m_PropertyType = EditorUtils.GetPropertyType(property);
            bool isComponent = m_PropertyType.IsSubclassOf(typeof(Component));
            string assetFilter = GetAssetFilter(m_PropertyType, Attribute.SearchFilter, isComponent);
            IEnumerable<string> assetGuids = GetAssetGuids(isComponent, assetFilter);
            IEnumerable<AssetListItem> assetResults = assetGuids.Select(g => new AssetListItem(g)).OrderBy(r => r.Name);

            if (Attribute.CanBeNull) {
                assetResults = assetResults.Prepend(new AssetListItem(null));
            }

            m_AssetListItems = assetResults.ToArray();
            m_Labels = m_AssetListItems.Select(r => r.Label).ToArray();

            if (property.objectReferenceValue && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out string guid, out long _)) {
                selectedIndex = m_AssetListItems.IndexOf(r => r.Guid == guid);

                // If we have an existing object with a correct index, then we are done here.
                if (selectedIndex >= 0) {
                    return;
                }
            }

            // 1 = not found if we allow null since we've already prepended the 'None' option above.
            bool assetsFound = m_AssetListItems.Length > (Attribute.CanBeNull ? 1 : 0);

            // If we have no reference value, we have some assets, and we aren't allowed to be null, then let's set the object reference value.
            if (!property.objectReferenceValue && assetsFound && !Attribute.CanBeNull) {
                // If we have a default name available, try to get that name's index.
                if (!Attribute.DefaultName.IsNullOrEmpty()) {
                    selectedIndex = m_Labels.IndexOf(l => l.text.Equals(Attribute.DefaultName, StringComparison.OrdinalIgnoreCase));
                }

                // Use either the default name index or 0 if no default name was found.
                selectedIndex = Mathf.Max(selectedIndex, 0);

                // Set the object reference to that found guid (first or default name).
                property.objectReferenceValue = EditorUtils.GetAssetFromGuid(m_AssetListItems[selectedIndex].Guid, m_PropertyType);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private IEnumerable<string> GetAssetGuids(bool isComponent, string assetFilter) {
            // If we aren't searching for a component, just run FindAssets() normally.
            if (!isComponent) {
                return AssetDatabase.FindAssets(assetFilter, Attribute.SearchFolder.IsNullOrEmpty() ? new string[0] : new[] {Attribute.SearchFolder});
            }

            // If this is a component, then FindAssets() won't return anything directly.
            // We have to check the prefab game objects and get components from them directly.

            IEnumerable<string> assetGuids = null;

            // If we don't have any custom search options, then we can use GuiDataService as a fast search across the whole repo.
            if (Attribute.SearchFilter.IsNullOrEmpty() && Attribute.SearchFolder.IsNullOrEmpty()) {
                // Try to get the component's script GUID.
                string scriptGuid = AssetDatabase.FindAssets("t:Script " + m_PropertyType.Name)
                                                 .FirstOrDefault(g => GetAssetName(g).Equals(m_PropertyType.Name, StringComparison.OrdinalIgnoreCase));

                // If we have the component GUID, then get the Guid results.
                if (!scriptGuid.IsNullOrEmpty()) {
                    assetGuids = GuidDataService.LoadUsedBy(new[] {new Guid(scriptGuid)}).Select(entry => entry.GuidString);
                }
            }

            // If we don't have results from the GuidDataService search, then we'll need to search normally.
            if (assetGuids == null) {
                assetGuids = AssetDatabase.FindAssets(assetFilter, Attribute.SearchFolder.IsNullOrEmpty() ? new string[0] : new[] {Attribute.SearchFolder});
            }

            // Finally, we need to only return objects that have the matching component type at the top level.
            return assetGuids.Where(g => EditorUtils.GetAssetFromGuid(g, m_PropertyType));
        }

        private static string GetAssetName(string guid) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return Path.GetFileNameWithoutExtension(path);
        }

        private static string GetAssetFilter(Type propertyType, string customFilter, bool isComponent) {
            string assetFilter = "t:" + (isComponent ? "GameObject" : propertyType.Name);

            if (!string.IsNullOrEmpty(customFilter)) {
                if (assetFilter.Contains("t:")) {
                    assetFilter = customFilter;
                } else {
                    assetFilter += " " + customFilter;
                }
            }

            return assetFilter;
        }

        private class AssetListItem {
            public string Guid { get; }
            public string Name { get; }
            public GUIContent Label { get; }

            public AssetListItem(string guid) {
                Guid = guid;

                if (guid.IsNullOrEmpty()) {
                    Name = "None";
                    Label = new GUIContent(Name);
                } else {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Name = Path.GetFileNameWithoutExtension(path);
                    Label = new GUIContent(Name, path);
                }
            }
        }
    }
}