using UnityEngine;

namespace CoreUtils {
    public class AutoFillAttribute : PropertyAttribute { }
    public class AutoFillFromSceneAttribute : AutoFillAttribute { }
    public class AutoFillFromParentAttribute : AutoFillAttribute { }
    public class AutoFillFromChildrenAttribute : AutoFillAttribute { }
}