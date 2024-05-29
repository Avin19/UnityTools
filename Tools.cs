using UnityEditor;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEngine.Application;
using static UnityEditor.AssetDatabase;
using UnityEditor.PackageManager;
using UnityEngine;

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
            CreateDirectories("Project", "Scripts", "Materials", "Music", "Perfabs", "Models", "Scenes", "Editor");
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

        [MenuItem("Tools/Setup /Add Necessary Package / Resolve")]
        public static void Resolve()
        {
            Client.Resolve();
        }
        [MenuItem(" Tools/Setup /Add Necessary Package / Update Visual Studio Code ")]
        public static void AddVisualStudioCode()
        {
            Client.Add("com.unity.ide.visualstudio");

        }
        [MenuItem(" Tools/Setup /Add Necessary Package / Update Text Mesh Pro  ")]
        public static void AddTextMeshPro()
        {
            Client.Add("com.unity.ide.visualstudio");
        }
        [MenuItem(" Tools/Setup /Add Necessary Package / Remove Visual Scripting ")]
        public static void RemoveVisualScripting()
        {
            Client.Remove("com.unity.visualscripting");
        }
        [MenuItem(" Tools/Setup /Add Necessary Package / Remove Rider  ")]
        public static void RemoveRider()
        {
            Client.Remove("com.unity.ide.rider");
        }
        [MenuItem(" Tools/Setup /Add Necessary Package / Remove Timeline  ")]
        public static void RemoveTimeline()
        {
            Client.Remove("com.unity.timeline");
        }


    }
}