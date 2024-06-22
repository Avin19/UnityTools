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

        [MenuItem("Tools/Setup/Resolve Packages")]
        public static void Resolve()
        {
            Client.Resolve();
        }
        [MenuItem(" Tools/Setup/Add Necessary Package  ")]
        public static void AddRemoveNecessaryPackages()
        {
            string[] apackages = { "com.unity.ide.visualstudio", "com.unity.textmeshpro", "com.unity.inputsystem" };
            string[] rpackages = { "com.unity.visualscripting", "com.unity.ide.rider", "com.unity.timeline" };
            Client.AddAndRemove(apackages, rpackages);
            Resolve();
        }

        [MenuItem("Tools/Setup/Getting Up All Script")]
        public static async Task GettingAllScripts()
        {
            string folderPath = GetCurrentDirectory();
            string fileUrl = "https://raw.githubusercontent.com/Avin19/UnityTools/main/.gitignore";
            string filePath = Combine(folderPath, ".gitignore");
            await DownloadFileAsync(fileUrl, filePath);
        }
        static async Task DownloadFileAsync(string url, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                // Send a GET request to the specified URL
                HttpResponseMessage response = await client.GetAsync(url);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a byte array
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                // Write the byte array to the specified file
                await File.WriteAllBytesAsync(filePath, fileBytes);
            }

        }
    }
}