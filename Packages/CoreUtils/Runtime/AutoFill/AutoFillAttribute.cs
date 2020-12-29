using UnityEngine;

namespace CoreUtils {
    public class AutoFillAttribute : PropertyAttribute { }
    public class AutoFillFromParentAttribute : AutoFillAttribute { }
    public class AutoFillFromChildrenAttribute : AutoFillAttribute { }
}