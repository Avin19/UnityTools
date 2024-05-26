using UnityEditor;


public static class CustomScriptsTemplets
{
    [MenuItem("Assets/Create/Code/MonoBehaviour", priority = 40)]
    public static void CreateMonoBehaviourMenuItem()
    {
        string templatePath = "Assets/Editor/Templates/NewScripts.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewScript.cs");
    }
    [MenuItem("Assets/Create/Code/Enum", priority = 41)]
    public static void CreateEnumMenuItem()
    {
        string templatePath = "Assets/Editor/Template/NewEnum.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewEnum.cs");
    }
    [MenuItem("Assets/Create/Code/ScriptableObject", priority = 42)]
    public static void CreateScriptableObjectMenuItem()
    {
        string templatePath = "Assets/Editor/Template/NewScriptableObject.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "SONewScriptableObject.cs");
    }
}