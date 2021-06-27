namespace CoreUtils {
    public enum MenuOrder {
        Bucket = -1001,
        EventGeneric = -1000,
        VariableBool,
        VariableFloat,
        VariableFloatRange,
        VariableInt,
        VariableString,
        EventBool = -950,
        EventString,
        Command = 0,
        Config,
        EventObject,
        VariableObject,
        EditorScript = 82,
        LightingSettings = 200,
        GameObject = 2000,
        Window = 3000,
        Usages = 5000,
    }
}