using UnityEditor;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEngine.Application;
using static UnityEditor.AssetDatabase;
using UnityEditor.PackageManager;
using UnityEngine;

namespace avinash
{
    public static class Tools
    {
        [MenuItem("Tools/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
           CreateDirectories("_Project" , "Scripts", "Materials", "Music", "Perfabs" , "Models","Scenes");
            Refresh();

        }
        public static void CreateDirectories(string root , params string[] dir)
        {
            string fullPath = Combine(dataPath, root);
            foreach(string newDir in dir)
            {
                CreateDirectory(Combine(fullPath,newDir));
            }
        }
        [MenuItem(" Tools/Setup /Add Necessary Package")]
        static void Add()
        {
            Client.Add("com.unity.ide.visualstudio");
        }
    }
}