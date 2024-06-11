using UnityEditor;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEngine.Application;
using static UnityEditor.AssetDatabase;
using UnityEditor.PackageManager;


namespace avinash
{
    /// <summary>
    ///  This is Editor Tool to  Create Default Folder Structure and Install Necessary packages 
    /// </summary>
    public static class Tools
    {
        [MenuItem("Tools/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            CreateDirectories("Project", "Scripts", "Materials", "Music", "Perfabs", "Models", "Texture", "Editor");
            Refresh();

        }
        public static void CreateDirectories(string root, params string[] dir)
        {
            string fullPath = Combine(dataPath, root);
            foreach (string newDir in dir)
            {
                CreateDirectory(Combine(fullPath, newDir));
            }
        }

        [MenuItem("Tools/Setup / Resolve Packages")]
        public static void Resolve()
        {
            Client.Resolve();
        }
        [MenuItem(" Tools/Setup /Add Necessary Package  ")]
        public static void AddRemoveNecessaryPackages()
        {
            string[] apackages = { "com.unity.ide.visualstudio", "com.unity.textmeshpro", "com.unity.inputsystem" };
            string[] rpackages = { "com.unity.visualscripting", "com.unity.ide.rider", "com.unity.timeline" };
            Client.AddAndRemove(apackages, rpackages);

            Resolve();
        }
        [MenuItem(" Tools/Setup /Visual Studio Package  ")]
        public static void AddVisualStudioCode()
        {
            Client.Add("com.unity.ide.visualstudio");
            Resolve();
            AddTextMeshPro();


        }
        public static void AddTextMeshPro()
        {
            Client.Add("com.unity.textmeshpro");
        }
        [MenuItem(" Tools/Setup /Remove Unecessary Package")]
        public static void RemoveVisualScripting()
        {
            Client.Remove("com.unity.visualscripting");
            Resolve();
            RemoveRider();
        }

        public static void RemoveRider()
        {
            Client.Remove("com.unity.ide.rider");
            Resolve();
            RemoveTimeline();
        }
        public static void RemoveTimeline()
        {
            Client.Remove("com.unity.timeline");
            Resolve();
        }


    }
}