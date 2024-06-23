using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class ScriptDownloaderEditor : EditorWindow
{
    [MenuItem("Tools/Download Template Scripts")]
    public static void ShowWindow()
    {
        GetWindow<ScriptDownloaderEditor>("Download Template Scripts");
    }

    private async void OnGUI()
    {
        if (GUILayout.Button("Download Scripts"))
        {
            await GettingTemplateScripts();
        }
        if (GUILayout.Button("Add Necessary Packages"))
        {
            await AddRemoveNecessaryPackages();
        }
        if (GUILayout.Button("Resolve Packages"))
        {
            Resolve();
        }
    }

    public static async Task GettingTemplateScripts()
    {
        string folderPath = Application.dataPath;

        string[] fileUrls = {
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/.gitignore",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/CustomScriptsTemplate.cs",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewScript.cs.txt",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewEnum.cs.Txt",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewScriptableObject.cs.txt",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewClass.cs.txt"
        };
        string[] fileNames = {
            ".gitignore",
            "CustomScriptsTemplate.cs",
            "NewScript.cs.txt",
            "NewEnum.cs.txt",
            "NewScriptableObject.cs.txt",
            "NewClass.cs.txt"
        };

        for (int i = 0; i < fileUrls.Length; i++)
        {
            string fullPath = Path.Combine(folderPath, "Project/Editor/Template", fileNames[i]);

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            EditorUtility.DisplayProgressBar("Downloading Scripts", $"Downloading {fileNames[i]}...", (float)i / fileUrls.Length);

            await DownloadFileAsync(fileUrls[i], fullPath);
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("All scripts downloaded successfully.");
    }

    private static async Task DownloadFileAsync(string url, string filePath)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send a GET request to the specified URL
                HttpResponseMessage response = await client.GetAsync(url);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a byte array
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                // Write the byte array to the specified file
                await File.WriteAllBytesAsync(filePath, fileBytes);

                Debug.Log($"Downloaded and saved file to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while downloading {url}: {ex.Message}");
            }
        }
    }

    [MenuItem("Tools/Setup/Resolve Packages")]
    public static void Resolve()
    {
        Client.Resolve();
        Debug.Log("Packages resolved.");
    }

    [MenuItem("Tools/Setup/Add Necessary Packages")]
    public static async Task AddRemoveNecessaryPackages()
    {
        string[] packagesToAdd = { "com.unity.ide.visualstudio", "com.unity.textmeshpro", "com.unity.inputsystem" };
        string[] packagesToRemove = { "com.unity.visualscripting", "com.unity.ide.rider", "com.unity.timeline" };

        await AddPackages(packagesToAdd);
        await RemovePackages(packagesToRemove);

        Resolve();
    }

    private static async Task AddPackages(string[] packages)
    {
        foreach (string package in packages)
        {
            AddRequest request = Client.Add(package);
            while (!request.IsCompleted)
                await Task.Yield();

            if (request.Status == StatusCode.Success)
            {
                Debug.Log($"Successfully added package: {package}");
            }
            else if (request.Status >= StatusCode.Failure)
            {
                Debug.LogError($"Failed to add package: {package}, Error: {request.Error.message}");
            }
        }
    }

    private static async Task RemovePackages(string[] packages)
    {
        foreach (string package in packages)
        {
            RemoveRequest request = Client.Remove(package);
            while (!request.IsCompleted)
                await Task.Yield();

            if (request.Status == StatusCode.Success)
            {
                Debug.Log($"Successfully removed package: {package}");
            }
            else if (request.Status >= StatusCode.Failure)
            {
                Debug.LogError($"Failed to remove package: {package}, Error: {request.Error.message}");
            }
        }
    }
}
