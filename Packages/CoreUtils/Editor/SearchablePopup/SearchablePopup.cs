using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public class SearchablePopup {

        private string m_SearchString = "";
        private string[] m_SearchBits;
        private Vector2 m_ScrollPos;
        private List<TreeItem> m_Items;
        private TreeItem m_SelectedItem;
        private int m_TotalItemCount;
        private int m_FilteredItemCount;
        private float m_TotalItemWidth;
        private float m_FilteredItemWidth;

        private static GUIStyle s_ToolbarSearchStyle;
        private static GUIStyle s_ToolbarCancelSearchStyle;
        private static GUIStyle s_ToolbarCancelEmptySearchStyle;

        private static GUIStyle s_LabelStyle;
        private static GUIStyle s_MixedLabelStyle;

        private static GUIStyle s_BackgroundStyleEven;
        private static GUIStyle s_BackgroundStyleOdd;

        private static GUIStyle s_CreateItemButtonStyle;

        private Rect m_NextItemRect;
        private int m_NextRowIndex;

        private const float kIndent = 16;
        private const string kSearchFieldName = "SearchablePopupSearchField";

        private Func<EditorWindow> m_GetEditorWindow;

        private bool m_ExactMatch;
        private GUIContent m_CreateItemContent;

        private event SearchChangedEventHandler SearchChanged;
        private event CreateItemEventHandler CreateItem;

        public void SetEditorWindow(EditorWindow window) {
            m_GetEditorWindow = () => window;
        }

        public void SetEditorWindow(Func<EditorWindow> windowCallback) {
            m_GetEditorWindow = windowCallback;
        }

        public EditorWindow EditorWindow {
            get {
                if (m_GetEditorWindow != null) {
                    return m_GetEditorWindow();
                }
                return null;
            }
        }

        // wrapper to make our popup content work with Unity's Popup
        private class SearchablePopupContent : PopupWindowContent {
            private readonly Rect m_ButtonRect;
            private SearchablePopup m_Searchable;

            public SearchablePopupContent(Rect buttonRect, SearchablePopup searchable) {
                m_ButtonRect = buttonRect;
                m_Searchable = searchable;
            }

            public override void OnGUI(Rect rect) {
                m_Searchable.OnGUI(rect);

                // Force Repaint
                editorWindow.Repaint();
            }

            public override Vector2 GetWindowSize() {
                Vector2 size = m_Searchable.GetWindowSize(true);
                size.x = Mathf.Max(size.x, m_ButtonRect.width);
                return size;
            }
        }

        public class TreeItem {

            private bool m_PassedFilter = true;

            private TreeItem m_Parent;
            private List<TreeItem> m_Children;

            public bool PassedFilter {
                get => m_PassedFilter;
                set {
                    m_PassedFilter = value;
                    if (value) {
                        if (m_Parent != null) {
                            m_Parent.PassedFilter = true;
                        }
                    }
                }
            }

            public GUIContent Content { get; }

            public int Depth { get; private set; }

            public object UserData { get; }
            public Action<TreeItem> OnSelected { get; }
            public string SearchText { get; }
            public bool Ticked { get; set; } = false;
            public bool PartiallyTicked { get; set; } = false;

            // content:    The content (usually text) to display in the search window
            //		 	   If this contains '/' characters then it will be split up to make a tree structure.
            // searchText: The text to apply the search filter to, usually this will be the same as content.text
            // userData:   user data that can be utilised for contenxt specufuc actions in the delegate callback
            // onSelected: delegate to call when the end user selects an item
            public TreeItem(GUIContent content, string searchText, object userData = null, Action<TreeItem> onSelected = null) {
                Content = content;
                if (!string.IsNullOrEmpty(searchText)) {
                    SearchText = searchText.ToLower();
                }
                UserData = userData;
                m_Children = null;
                Depth = 0;
                OnSelected = onSelected;
            }

            public void AddChild(TreeItem child) {
                if (m_Children == null) {
                    m_Children = new List<TreeItem>();
                }
                m_Children.Add(child);
                child.m_Parent = this;
                child.Depth = Depth + 1;
            }

            public void Visit(Action<TreeItem> action) {
                action(this);
                if (m_Children != null) {
                    foreach (TreeItem child in m_Children) {
                        child.Visit(action);
                    }
                }
            }
        }

        private static GUIStyle ToolbarSearchStyle {
            get {
                if (s_ToolbarSearchStyle == null) {
                    s_ToolbarSearchStyle = new GUIStyle("ToolBarSeachTextField");
                    ////Try and deal with Unity fixing its spelling mistake
                    //if (s_ToolbarSearchStyle == null) {
                    //	s_ToolbarSearchStyle = new GUIStyle("ToolBarSearchTextField");
                    //}
                }
                return s_ToolbarSearchStyle;
            }
        }

        private static GUIStyle ToolbarCancelSearchStyle {
            get {
                if (s_ToolbarCancelSearchStyle == null) {
                    s_ToolbarCancelSearchStyle = new GUIStyle("ToolBarSeachCancelButton");
                    ////Try and deal with Unity fixing its spelling mistake
                    //if (s_ToolbarCancelSearchStyle == null) {
                    //	s_ToolbarCancelSearchStyle = new GUIStyle("ToolBarSearchCancelButton");
                    //}
                }
                return s_ToolbarCancelSearchStyle;
            }
        }

        private static GUIStyle ToolbarCancelEmptySearchStyle {
            get {
                if (s_ToolbarCancelEmptySearchStyle == null) {
                    s_ToolbarCancelEmptySearchStyle = new GUIStyle("ToolBarSeachCancelButtonEmpty");
                    ////Try and deal with Unity fixing its spelling mistake
                    //if (s_ToolbarCancelEmptySearchStyle == null) {
                    //	s_ToolbarCancelEmptySearchStyle = new GUIStyle("ToolBarSeachCancelButtonEmpty");
                    //}
                }
                return s_ToolbarCancelEmptySearchStyle;
            }
        }

        private static GUIStyle LabelStyle {
            get {
                if (s_LabelStyle == null) {
                    s_LabelStyle = new GUIStyle("MenuItem");
                }
                return s_LabelStyle;
            }
        }

        private static GUIStyle MixedLabelStyle {
            get {
                if (s_MixedLabelStyle == null) {
                    s_MixedLabelStyle = new GUIStyle("MenuItemMixed");
                }
                return s_MixedLabelStyle;
            }
        }

        private GUIStyle BackgroundStyleEven {
            get {
                if (s_BackgroundStyleEven == null) {
                    s_BackgroundStyleEven = new GUIStyle(GUI.skin.GetStyle("CN EntryBackEven"));
                }
                return s_BackgroundStyleEven;
            }
        }

        private GUIStyle BackgroundStyleOdd {
            get {
                if (s_BackgroundStyleOdd == null) {
                    s_BackgroundStyleOdd = new GUIStyle(GUI.skin.GetStyle("CN EntryBackOdd"));
                }
                return s_BackgroundStyleOdd;
            }
        }

        private GUIStyle CreateItemButtonStyle {
            get {
                if (s_CreateItemButtonStyle == null) {
                    s_CreateItemButtonStyle = new GUIStyle(GUI.skin.GetStyle("CapsuleButton"));
                }
                return s_CreateItemButtonStyle;
            }
        }

        public static void EnumPropertyField(SerializedProperty property) {
            EnumPropertyField(property, new GUIContent(property.displayName, property.tooltip));
        }

        public static void EnumPropertyField(SerializedProperty property, GUIContent label) {
            if (property.propertyType == SerializedPropertyType.Enum) {
                EditorGUILayout.PrefixLabel(label, EditorStyles.popup);
                string[] names = property.enumDisplayNames;
                GUIContent valueContent;
                if (property.enumValueIndex >= 0 && property.enumValueIndex < names.Length) {
                    // index is a valid 
                    valueContent = new GUIContent(names[property.enumValueIndex]);
                } else {
                    valueContent = GUIContent.none;
                }
                Rect buttonRect = GUILayoutUtility.GetRect(valueContent, EditorStyles.popup);
                if (GUI.Button(buttonRect, valueContent, EditorStyles.popup)) {
                    GUIContent[] labels = new GUIContent[names.Length];
                    for (int i = 0; i < names.Length; i++) {
                        labels[i] = new GUIContent(names[i]);
                    }
                    ShowAsPopup(buttonRect, labels, property.enumValueIndex, selectedIndex => SelectEnumPropertyValue(property, selectedIndex));
                }
            } else {
                EditorGUILayout.PrefixLabel(label, EditorStyles.helpBox);
                EditorGUILayout.HelpBox(string.Format("{0} is not an enum field", property.propertyPath), MessageType.Error);
            }
        }

        private static void SelectEnumPropertyValue(SerializedProperty property, int selectedIndex) {
            property.enumValueIndex = selectedIndex;
            property.serializedObject.ApplyModifiedProperties();
        }

        public delegate void SelectedIndexEventHandler(int selectedIndex);

        public delegate void SearchChangedEventHandler(string search);

        public delegate void CreateItemEventHandler(string name);

        public static void ShowAsPopup(Rect buttonPosition, GUIContent[] labels, int selectedIndex, SelectedIndexEventHandler onSelection, string search = "", SearchChangedEventHandler onSearchChanged = null, CreateItemEventHandler onCreateItem = null) {
            List<TreeItem> roots = new List<TreeItem>();
            TreeItem selectedItem = null;
            Dictionary<string, TreeItem> itemMap = new Dictionary<string, TreeItem>();
            List<TreeItem> items = new List<TreeItem>();

            //Create all the actual leaf items
            for (int i = 0; i < labels.Length; i++) {
                GUIContent label = labels[i];
                string text = label.text;

                if (!string.IsNullOrEmpty(text)) {
                    string[] bits = text.Split('/');

                    GUIContent leafContent = new GUIContent(bits[bits.Length - 1], label.image, label.tooltip);

                    // We store the index into the labels array in userData
                    TreeItem item = new TreeItem(leafContent, text, i, userSelectedItem => {
                        if (onSelection != null) {
                            onSelection((int) userSelectedItem.UserData);
                        }
                    });

                    if (!itemMap.ContainsKey(text)) {
                        items.Add(item);
                        if (!itemMap.ContainsKey(text)) {
                            itemMap.Add(text, item);
                        }
                        if (i == selectedIndex) {
                            selectedItem = item;
                        }
                    } else {
                        Debug.LogErrorFormat("Duplicate Key \"{0}\" in label \"{1}\"", text, label);
                    }
                }
            }

            // Create any intermediate parent items
            foreach (TreeItem item in items) {
                int index = (int) item.UserData;

                string originalText = labels[index].text;
                if (!string.IsNullOrEmpty(originalText)) {
                    string[] bits = originalText.Split('/');
                    TreeItem prev = null;
                    TreeItem parent = null;
                    for (int i = 1; i < bits.Length; i++) {
                        prev = parent;
                        string key = string.Join("/", bits.Take(i).ToArray());
                        if (!itemMap.TryGetValue(key, out parent)) {
                            parent = new TreeItem(new GUIContent(bits[i - 1]), null);
                            if (prev != null) {
                                prev.AddChild(parent);
                            } else {
                                roots.Add(parent);
                            }
                            itemMap.Add(key, parent);
                        }
                    }
                    // Add the actual item to the parent (if we even found a parent)
                    if (parent != null) {
                        parent.AddChild(item);
                    } else {
                        roots.Add(item);
                    }
                }
            }

            ShowAsPopup(buttonPosition, roots.ToArray(), selectedItem, search, onSearchChanged, onCreateItem);
        }

        public static void ShowAsPopup(Rect buttonPosition, TreeItem[] rootItems, TreeItem selectedItem, string search = "", SearchChangedEventHandler onSearchChanged = null, CreateItemEventHandler onCreateItem = null) {
            SearchablePopup searchable = new SearchablePopup();

            searchable.m_Items = new List<TreeItem>(rootItems);
            searchable.m_SelectedItem = selectedItem;
            searchable.m_SearchString = search ?? "";
            if (onSearchChanged != null) {
                searchable.SearchChanged += onSearchChanged;
            }

            if (onCreateItem != null) {
                searchable.CreateItem += onCreateItem;
            }

            searchable.ApplyFilter();

            if (Event.current != null) {
                // If the event is not null, then we're inside OnGUI so can use the default Unity popup
                SearchablePopupContent content = new SearchablePopupContent(buttonPosition, searchable);
                searchable.SetEditorWindow(() => content.editorWindow);
                PopupWindow.Show(buttonPosition, content);
            } else {
                // Otherwise, the OnGUI version will throw an exception so use our own Editor window
                SearchablePopupEditorWindow.ShowAsPopup(buttonPosition, searchable);
            }
        }

        public Vector2 GetWindowSize(bool windowResizeable) {
            float itemWidth;
            int itemCount;
            if (windowResizeable) {
                itemWidth = m_FilteredItemWidth;
                itemCount = m_FilteredItemCount;
            } else {
                itemWidth = m_TotalItemWidth;
                itemCount = m_TotalItemCount;
            }

            float width = Mathf.Max(200, itemWidth + 16); // +16 to account for possible vertical scroll bar
            float height = Mathf.Max(200, itemCount*EditorGUIUtility.singleLineHeight + 16);

            return new Vector2(width, height);
        }

        public void OnGUI(Rect rect) {
            float toolbarHeight = EditorStyles.toolbar.CalcHeight(GUIContent.none, rect.width);
            Rect toolBarRect = rect;
            toolBarRect.height = toolbarHeight;
            GUI.Box(toolBarRect, GUIContent.none, EditorStyles.toolbar);
            //GUI.Toolbar(toolBarRect, -1, new GUIContent[] {});

            Rect searchRect = toolBarRect;
            // inset slightly
            searchRect.xMin += 2;
            searchRect.xMax -= 2;
            searchRect.y += 2;

            searchRect.height = ToolbarSearchStyle.CalcHeight(GUIContent.none, searchRect.width);

            GUIStyle cancelStyle = ToolbarCancelEmptySearchStyle;
            if (!string.IsNullOrEmpty(m_SearchString)) {
                cancelStyle = ToolbarCancelSearchStyle;
            }

            Rect cancelRect = searchRect;

            float minWidth, maxWidth;
            cancelStyle.CalcMinMaxWidth(GUIContent.none, out minWidth, out maxWidth);
            // remove the cancel button off the right of the search rect
            searchRect.xMax -= minWidth;
            cancelRect.xMin = cancelRect.xMax - minWidth;

            bool refilter = false;

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(kSearchFieldName);
            m_SearchString = GUI.TextField(searchRect, m_SearchString, ToolbarSearchStyle);
            if (EditorGUI.EndChangeCheck()) {
                refilter = true;
            }
            if (GUI.Button(cancelRect, GUIContent.none, cancelStyle)) {
                m_SearchString = "";
                refilter = true;
            }

            if (refilter) {
                ApplyFilter();
            }

            Rect scrollRect = rect;
            scrollRect.yMin += toolbarHeight;

            Rect treeRect = new Rect(0, 0, m_FilteredItemWidth, EditorGUIUtility.singleLineHeight*m_FilteredItemCount);

            m_ScrollPos = GUI.BeginScrollView(scrollRect, m_ScrollPos, treeRect, false, false);

            m_NextItemRect = new Rect(0, 0, Mathf.Max(scrollRect.width, m_FilteredItemWidth), EditorGUIUtility.singleLineHeight);
            m_NextRowIndex = 0;

            if (CreateItem != null && !string.IsNullOrEmpty(m_SearchString) && !m_ExactMatch) {
                Rect createButtonRect = m_NextItemRect;
                //inset
                createButtonRect.yMin += 1;
                createButtonRect.xMin += 2;
                createButtonRect.xMax -= 2;
                createButtonRect.yMax -= 1;

                if (GUI.Button(createButtonRect, m_CreateItemContent, CreateItemButtonStyle)) {
                    CreateItem(m_SearchString);
                    EditorWindow.Close();
                }

                m_NextItemRect.y += EditorGUIUtility.singleLineHeight;
                m_NextRowIndex += 1;
            }

            VisitItems(
                item => {
                    if (item.PassedFilter) {
                        DrawItem(item);
                    }
                });

            GUI.EndScrollView();

            if (GUI.GetNameOfFocusedControl() == string.Empty) {
                GUI.FocusControl(kSearchFieldName);
            }
        }

        private void DrawItem(TreeItem item) {
            if (Event.current.type == EventType.Repaint) {
                GUIStyle rowStyle = m_NextRowIndex%2 == 0 ? BackgroundStyleEven : BackgroundStyleOdd;
                bool selected = false;
                if (item == m_SelectedItem) {
                    //rowStyle = LabelStyle;
                    selected = true;
                }

                rowStyle.Draw(m_NextItemRect, false, false, selected, selected);
            }

            GUIContent label = item.Content;

            Rect labelRect = m_NextItemRect;
            labelRect.xMin += item.Depth*kIndent;
            GUI.enabled = item.OnSelected != null;
            if (Event.current.type == EventType.Repaint) {
                GUIStyle style = LabelStyle;
                if (item.PartiallyTicked) {
                    style = MixedLabelStyle;
                }
                bool showTick = item.Ticked | item.PartiallyTicked;

                //GUI.Toggle(labelRect, showTick, GUIContent.none, style);
                style.Draw(labelRect, label, labelRect.Contains(Event.current.mousePosition), showTick, showTick, false);
                // Fake label to display tooltip
                EditorGUI.LabelField(labelRect, new GUIContent("", label.tooltip), GUIStyle.none);
            }
            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition)) {
                //if (GUI.Button(labelRect, label, LabelStyle)) {
                m_SelectedItem = item;
                if (m_SelectedItem.OnSelected != null) {
                    m_SelectedItem.OnSelected(item);
                }
                EditorWindow.Close();
            }
            GUI.enabled = true;
            m_NextItemRect.y += EditorGUIUtility.singleLineHeight;
            m_NextRowIndex += 1;
        }

        private void ApplyFilter() {
            string searchLower = m_SearchString.ToLower();
            m_SearchBits = searchLower.Split(new[] {' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            //reset all the filters;
            VisitItems(item => item.PassedFilter = false);

            //Apply filters
            VisitItems(item => item.PassedFilter = Filter(item));

            m_ExactMatch = false;

            //Count visible
            m_FilteredItemCount = 0;
            m_TotalItemCount = 0;
            m_FilteredItemWidth = 0;
            m_TotalItemWidth = 0;
            VisitItems(item => {
                float min, max;
                LabelStyle.CalcMinMaxWidth(item.Content, out min, out max);
                float itemWidth = item.Depth*kIndent + max;
                if (item.PassedFilter) {
                    m_FilteredItemWidth = Mathf.Max(m_FilteredItemWidth, itemWidth);
                    m_FilteredItemCount++;
                }
                m_TotalItemWidth = Mathf.Max(m_TotalItemWidth, itemWidth);
                m_TotalItemCount++;

                if (item.SearchText == searchLower) {
                    m_ExactMatch = true;
                }
            });

            if (CreateItem != null && !string.IsNullOrEmpty(m_SearchString) && !m_ExactMatch) {
                // need an extra row for the createItem Button
                m_CreateItemContent = new GUIContent(string.Format("Create \"{0}\"", m_SearchString));
                m_TotalItemCount++;
            }

            if (EditorWindow != null) {
                EditorWindow.Repaint();
            }

            if (SearchChanged != null) {
                SearchChanged(m_SearchString);
            }
        }

        private void VisitItems(Action<TreeItem> action) {
            foreach (TreeItem item in m_Items) {
                item.Visit(action);
            }
        }

        private bool Filter(TreeItem item) {
            string itemText = item.SearchText;
            if (string.IsNullOrEmpty(itemText)) {
                return false;
            }

            foreach (string bit in m_SearchBits) {
                if (!itemText.Contains(bit)) {
                    return false;
                }
            }
            // We found ALL of the search bits!
            return true;
        }

    }
}