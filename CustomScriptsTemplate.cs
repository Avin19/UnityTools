using UnityEditor;


public static class CustomScriptsTemplets
{
    [MenuItem("Assets/Create/Code/MonoBehaviour", priority = 40)]
    public static void CreateMonoBehaviourMenuItem()
    {
        string templatePath = "Assets/Editor/Templates/MonoBehaviour.cs.txt"
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewScript.cs");
    }
}