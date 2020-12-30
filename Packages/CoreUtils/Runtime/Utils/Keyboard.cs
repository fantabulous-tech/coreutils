using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreUtils {
    public class Keyboard : Singleton<Keyboard> {
        // OnKeyDown
        public static KeyAction OnKeyDown(string name, KeyCode key, Action action) {
            return OnKey(() => name, key, KeyType.Down, action);
        }

        public static KeyAction OnKeyDown(string name, KeyCode[] keys, Action action) {
            return OnKey(() => name, keys, KeyType.Down, action);
        }

        public static KeyAction OnKeyDown(Func<string> nameFunc, KeyCode key, Action action) {
            return OnKey(nameFunc, key, KeyType.Down, action);
        }

        public static KeyAction OnKeyDown(Func<string> nameFunc, KeyCode[] keys, Action action) {
            return OnKey(nameFunc, keys, KeyType.Down, action);
        }

        // OnKeyUp
        public static KeyAction OnKeyUp(string name, KeyCode key, Action action) {
            return OnKey(() => name, key, KeyType.Up, action);
        }

        public static KeyAction OnKeyUp(string name, KeyCode[] keys, Action action) {
            return OnKey(() => name, keys, KeyType.Up, action);
        }

        public static KeyAction OnKeyUp(Func<string> nameFunc, KeyCode key, Action action) {
            return OnKey(nameFunc, key, KeyType.Up, action);
        }

        public static KeyAction OnKeyUp(Func<string> nameFunc, KeyCode[] keys, Action action) {
            return OnKey(nameFunc, keys, KeyType.Up, action);
        }

        // OnKeyHold
        public static KeyAction OnKeyHold(string name, KeyCode key, Action action) {
            return OnKey(() => name, key, KeyType.Hold, action);
        }

        public static KeyAction OnKeyHold(string name, KeyCode[] keys, Action action) {
            return OnKey(() => name, keys, KeyType.Hold, action);
        }

        public static KeyAction OnKeyHold(Func<string> nameFunc, KeyCode key, Action action) {
            return OnKey(nameFunc, key, KeyType.Hold, action);
        }

        public static KeyAction OnKeyHold(Func<string> nameFunc, KeyCode[] keys, Action action) {
            return OnKey(nameFunc, keys, KeyType.Hold, action);
        }

        private static KeyAction OnKey(Func<string> nameFunc, KeyCode key, KeyType type, Action action) {
            return OnKey(nameFunc, new[] {key}, type, action);
        }

        private static KeyAction OnKey(Func<string> nameFunc, KeyCode[] keys, KeyType type, Action action) {
            if (keys.All(k => k == KeyCode.None)) {
                return null;
            }
            KeyAction keyAction = new KeyAction(nameFunc, keys, type, action);
            Actions.Add(keyAction);
            Instance.UpdateKeys();
            return keyAction;
        }

        private static List<KeyAction> Actions => Instance.m_Actions ?? (Instance.m_Actions = new List<KeyAction>());
        private List<KeyAction> m_Actions;

        private string GuiText { get; set; }

        private List<KeyAction> m_KeyGroups;

        private void UpdateKeys() {
            GuiText = Actions.OrderBy(a => a.MainKey).Select(a => a.ToString()).Distinct().AggregateToString("\n");
            m_KeyGroups = Actions.OrderByDescending(k => k.Count).ThenByDescending(g => g.MainKey).ToList();
        }

        public static bool HasAction(KeyAction action) {
            return Actions.Contains(action);
        }

        protected void Update() {
            if (!this) {
                return;
            }
            if (m_KeyGroups == null) {
                UpdateKeys();
            }
            if (m_KeyGroups == null) {
                return;
            }

            string foundKeylabel = null;
            for (int i = 0; i < m_KeyGroups.Count; i++) {
                if (!foundKeylabel.IsNullOrEmpty() && m_KeyGroups[i].KeyLabel != foundKeylabel) {
                    return;
                }
                if (m_KeyGroups[i].Try()) {
                    foundKeylabel = m_KeyGroups[i].KeyLabel;
                }
            }
        }

        private void OnGUI() {
            GUIContent content = new GUIContent(GuiText);
            Vector2 size = GUI.skin.textField.CalcSize(content);
            GUI.enabled = false;
            GUI.TextField(new Rect(10, Screen.height - size.y - 10, size.x, size.y), GuiText);
            GUI.enabled = true;
        }

        public enum KeyType {
            Down,
            Up,
            Hold
        }

        public class KeyAction {
            public readonly string KeyLabel;
            private readonly KeyCode[] m_Keys;
            private readonly KeyCode m_MainKey;
            private readonly Func<string> m_NameFunc;

            public string Name => m_NameFunc();

            public Action Action { get; }
            public KeyType Type { get; }

            public int Count => m_Keys.Length;
            public object MainKey => m_MainKey;

            public KeyAction(Func<string> nameFunc, KeyCode[] keys, KeyType type, Action action) {
                m_NameFunc = nameFunc;
                KeyLabel = keys.AggregateToString("-", CleanKey);
                m_Keys = GetRealKeys(keys);
                m_MainKey = GetMainKey(keys);
                Type = type;
                Action = action;
            }

            private static KeyCode GetMainKey(KeyCode[] keys) {
                KeyCode key = keys.FirstOrDefault(k => !IsModifier(k));
                if (key == KeyCode.None) {
                    key = keys.FirstOrDefault();
                }
                return key;
            }

            private static bool IsModifier(KeyCode key) {
                switch (key) {
                    case KeyCode.LeftShift:
                    case KeyCode.RightShift:
                    case KeyCode.LeftAlt:
                    case KeyCode.RightAlt:
                    case KeyCode.LeftCommand:
                    case KeyCode.RightCommand:
                    case KeyCode.LeftWindows:
                    case KeyCode.RightWindows:
                        return true;
                    default:
                        return false;
                }
            }

            private static KeyCode[] GetRealKeys(KeyCode[] keys) {
                List<KeyCode> realKeys = new List<KeyCode>();

                foreach (KeyCode key in keys) {
                    switch (key) {
                        case KeyCode.Exclaim:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha1);
                            continue;
                        case KeyCode.At:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha2);
                            continue;
                        case KeyCode.Hash:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha3);
                            continue;
                        case KeyCode.Dollar:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha4);
                            continue;
                        case KeyCode.Ampersand:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha5);
                            continue;
                        case KeyCode.Caret:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha6);
                            continue;
                        case KeyCode.Asterisk:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha8);
                            continue;
                        case KeyCode.LeftParen:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha9);
                            continue;
                        case KeyCode.RightParen:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Alpha0);
                            continue;
                        case KeyCode.Plus:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Equals);
                            continue;
                        case KeyCode.Underscore:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Minus);
                            continue;
                        case KeyCode.DoubleQuote:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Quote);
                            continue;
                        case KeyCode.Greater:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Period);
                            continue;
                        case KeyCode.Less:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Comma);
                            continue;
                        case KeyCode.Colon:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Semicolon);
                            continue;
                        case KeyCode.Question:
                            realKeys.Add(KeyCode.LeftShift);
                            realKeys.Add(KeyCode.Slash);
                            continue;
                        default:
                            realKeys.Add(key);
                            continue;
                    }
                }

                return realKeys.ToArray();
            }

            private bool IsPressed() {
                KeyCode first = KeyCode.None;

                for (int i = 0; i < m_Keys.Length; i++) {
                    KeyCode key = m_Keys[i];
                    if (IsKey(key, Type)) {
                        first = key;
                        break;
                    }
                }

                if (first == KeyCode.None) {
                    return false;
                }

                for (int i = 0; i < m_Keys.Length; i++) {
                    KeyCode unknown = m_Keys[i];
                    if (unknown != first && !IsKey(unknown, Input.GetKey)) {
                        return false;
                    }
                }

                return true;
            }

            private static readonly Predicate<KeyCode> s_GetKeyDown = Input.GetKeyDown;
            private static readonly Predicate<KeyCode> s_GetKeyUp = Input.GetKeyUp;
            private static readonly Predicate<KeyCode> s_GetKey = Input.GetKey;

            private static bool IsKey(KeyCode key, KeyType type) {
                switch (type) {
                    case KeyType.Down:
                        return IsKey(key, s_GetKeyDown);
                    case KeyType.Up:
                        return IsKey(key, s_GetKeyUp);
                    case KeyType.Hold:
                        return IsKey(key, s_GetKey);
                    default:
                        Debug.LogError(string.Format("Invalid type found: {0}", type));
                        return false;
                }
            }

            private static bool IsKey(KeyCode key, Predicate<KeyCode> test) {
                switch (key) {
                    case KeyCode.RightShift:
                    case KeyCode.LeftShift:
                        return test(KeyCode.RightShift) || test(KeyCode.LeftShift);
                    case KeyCode.RightControl:
                    case KeyCode.LeftControl:
                        return test(KeyCode.RightControl) || test(KeyCode.LeftControl);
                    case KeyCode.RightAlt:
                    case KeyCode.LeftAlt:
                        return test(KeyCode.RightAlt) || test(KeyCode.LeftAlt);
                    case KeyCode.RightCommand:
                    case KeyCode.LeftCommand:
                        return test(KeyCode.RightCommand) || test(KeyCode.LeftCommand);
                    case KeyCode.RightWindows:
                    case KeyCode.LeftWindows:
                        return test(KeyCode.RightWindows) || test(KeyCode.LeftWindows);
                    default:
                        return test(key);
                }
            }

            public bool Try() {
                if (!IsPressed()) {
                    return false;
                }
                try {
                    Action();
                }
                catch (Exception e) {
                    Debug.LogException(e);
                    return false;
                }
                Instance.UpdateKeys();
                if (Type != KeyType.Hold) {
                    Debug.Log($"{KeyLabel} {Type}: {m_NameFunc()}");
                }
                return true;
            }

            private static string CleanKey(KeyCode key) {
                switch (key) {
                    case KeyCode.UpArrow:
                        return "Up";
                    case KeyCode.DownArrow:
                        return "Down";
                    case KeyCode.RightArrow:
                        return "Right";
                    case KeyCode.LeftArrow:
                        return "Left";
                    case KeyCode.PageUp:
                        return "PgUp";
                    case KeyCode.PageDown:
                        return "PgDn";
                    case KeyCode.Alpha0:
                        return "0";
                    case KeyCode.Alpha1:
                        return "1";
                    case KeyCode.Alpha2:
                        return "2";
                    case KeyCode.Alpha3:
                        return "3";
                    case KeyCode.Alpha4:
                        return "4";
                    case KeyCode.Alpha5:
                        return "5";
                    case KeyCode.Alpha6:
                        return "6";
                    case KeyCode.Alpha7:
                        return "7";
                    case KeyCode.Alpha8:
                        return "8";
                    case KeyCode.Alpha9:
                        return "9";
                    case KeyCode.Exclaim:
                        return "!";
                    case KeyCode.DoubleQuote:
                        return "\"";
                    case KeyCode.Hash:
                        return "#";
                    case KeyCode.Dollar:
                        return "$";
                    case KeyCode.Ampersand:
                        return "&";
                    case KeyCode.Quote:
                        return "'";
                    case KeyCode.LeftParen:
                        return "(";
                    case KeyCode.RightParen:
                        return ")";
                    case KeyCode.Asterisk:
                        return "*";
                    case KeyCode.Plus:
                        return "+";
                    case KeyCode.Comma:
                        return ",";
                    case KeyCode.Period:
                        return ".";
                    case KeyCode.Slash:
                        return "/";
                    case KeyCode.Colon:
                        return ":";
                    case KeyCode.Semicolon:
                        return ";";
                    case KeyCode.Less:
                        return "<";
                    case KeyCode.Equals:
                        return "=";
                    case KeyCode.Greater:
                        return ">";
                    case KeyCode.Question:
                        return "?";
                    case KeyCode.At:
                        return "@";
                    case KeyCode.LeftBracket:
                        return "[";
                    case KeyCode.Backslash:
                        return "\\";
                    case KeyCode.RightBracket:
                        return "]";
                    case KeyCode.Caret:
                        return "^";
                    case KeyCode.Underscore:
                        return "_";
                    case KeyCode.BackQuote:
                        return "`";
                    case KeyCode.RightShift:
                    case KeyCode.LeftShift:
                        return "Shift";
                    case KeyCode.RightControl:
                    case KeyCode.LeftControl:
                        return "Ctrl";
                    case KeyCode.RightAlt:
                    case KeyCode.LeftAlt:
                        return "Alt";
                    case KeyCode.LeftCommand:
                    case KeyCode.RightCommand:
                        return "Cmd";
                    case KeyCode.LeftWindows:
                    case KeyCode.RightWindows:
                        return "Win";
                    case KeyCode.Mouse0:
                        return "RClick";
                    case KeyCode.Mouse1:
                        return "LClick";
                    case KeyCode.Mouse2:
                        return "Middle Click";
                    default:
                        return key.ToString();
                }
            }

            public override string ToString() {
                return KeyLabel + ": " + m_NameFunc();
            }
        }
    }
}