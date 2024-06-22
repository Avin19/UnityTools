using UnityEditor;


public static class CustomScriptsTemplets
{
    private static string Path = "Assets/Project/Editor/Template/";
    [MenuItem("Assets/Create/Code/MonoBehaviour", priority = 40)]
    public static void CreateMonoBehaviourMenuItem()
    {
        string templatePath = "NewScript.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path + templatePath, "NewScript.cs");
    }
    [MenuItem("Assets/Create/Code/Enum", priority = 41)]
    public static void CreateEnumMenuItem()
    {
        string templatePath = "NewEnum.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path + templatePath, "NewEnum.cs");
    }
    [MenuItem("Assets/Create/Code/ScriptableObject", priority = 42)]
    public static void CreateScriptableObjectMenuItem()
    {
        string templatePath = "NewScriptableObject.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path + templatePath, "SONewScriptableObject.cs");
    }
    [MenuItem("Assets/Create/Code/C#Class", priority = 43)]
    public static void CreateCShapeClassMenuItem()
    {
        string templatePath = "NewClass.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path + templatePath, "NewClass.cs");

    }
}
