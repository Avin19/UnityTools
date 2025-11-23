// ScriptDownloaderEditor.cs
// Place this file inside an Editor folder: Assets/Editor/ScriptDownloaderEditor.cs

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public class ScriptDownloaderEditor : EditorWindow
{
    private bool createScripts = true;
    private bool createSprite = true;
    private bool createMaterials = true;
    private bool createMusic = true;
    private bool createPrefabs = true;
    private bool createModels = true;
    private bool createTextures = true;
    private bool createEditor = true;
    private bool downloadGitIgnore = true;

    private string readmeContent =
@"# Unity Project Setup

## Description
This project is a Unity3D-based game setup tool designed to streamline development by automating the creation of folders, downloading essential scripts, and managing Unity packages.

## Features
- Auto-create project structure (Scripts, Materials, Prefabs, etc.).
- Download essential Unity C# scripts from a remote repository.
- Automatically download a `.gitignore` file for Unity projects.
- Manage Unity package dependencies (add/remove packages).
- Generate a `README.md` file with basic project information.

## Gameplay
Provide a brief explanation of the game mechanics.

## PlantUML Diagrams
### Class Diagram
![Class Diagram](include.png)

## Screenshots
<!-- ![Screenshot 2](screenshots/screenshot2.png) -->

## Development
This project is developed using Unity3D and C#. Contributions are welcome, including bug fixes, feature enhancements, and optimizations.

## Credits
This game remake is created by Developer Name.

## Feedback
If you have any feedback, suggestions, or bug reports, please open an issue on GitHub or contact us directly.

Prepare for liftoff and enjoy your journey to the International Space Station! 🚀";

    // Google Mobile Ads package URL & filename
    private const string GoogleMobileAdsUrl = "https://github.com/googleads/googleads-mobile-unity/releases/download/v10.6.0/GoogleMobileAds-v10.6.0.unitypackage";
    private const string GoogleMobileAdsFileName = "GoogleMobileAds-v10.6.0.unitypackage";

    // AdManager raw file URL for the exact commit the user provided
    private const string AdManagerRawUrl = "https://raw.githubusercontent.com/Avin19/UnityTools/6f925b8bad0e80425a9efd64a695a1057ca01bdc/Ads/AdManager.cs";

    [MenuItem("Tools/Setup/Script Downloader")]
    public static void ShowWindow()
    {
        GetWindow<ScriptDownloaderEditor>("Script Downloader");
    }

    private void OnGUI()
    {
        GUILayout.Label("Folder Setup", EditorStyles.boldLabel);
        createScripts = EditorGUILayout.Toggle("Scripts", createScripts);
        createSprite = EditorGUILayout.Toggle("Sprite", createSprite);
        createMaterials = EditorGUILayout.Toggle("Materials", createMaterials);
        createMusic = EditorGUILayout.Toggle("Music", createMusic);
        createPrefabs = EditorGUILayout.Toggle("Prefabs", createPrefabs);
        createModels = EditorGUILayout.Toggle("Models", createModels);
        createTextures = EditorGUILayout.Toggle("Textures", createTextures);
        createEditor = EditorGUILayout.Toggle("Editor", createEditor);

        if (GUILayout.Button("Create Folders"))
        {
            CreateSelectedFolders();
        }

        GUILayout.Space(8);
        GUILayout.Label("UI Package Management", EditorStyles.boldLabel);
        if (GUILayout.Button("Download & Install UnityPackage (Custom URL)"))
        {
            _ = DownloadAndInstallPackage(); // previously provided generic package downloader
        }

        GUILayout.Space(6);
        GUILayout.Label("Advertisement Legacy", EditorStyles.boldLabel);
        if (GUILayout.Button("Download & Install Advertisement Legacy"))
        {
            _ = DownloadAndInstallGoogleMobileAdsPackage();
        }

        GUILayout.Space(6);
        GUILayout.Label("Ad Scripts", EditorStyles.boldLabel);
        if (GUILayout.Button("Download AdManager.cs from GitHub (specific commit)"))
        {
            _ = DownloadAdManagerScript();
        }

        GUILayout.Space(6);
        GUILayout.Label("README Setup", EditorStyles.boldLabel);
        readmeContent = EditorGUILayout.TextArea(readmeContent, GUILayout.Height(200));

        if (GUILayout.Button("Create README"))
        {
            CreateReadmeFile();
        }

        GUILayout.Space(6);
        GUILayout.Label("Download Scripts", EditorStyles.boldLabel);
        if (GUILayout.Button("Download Template Scripts"))
        {
            _ = GettingTemplateScripts(); // Fire and forget async call
        }

        GUILayout.Space(6);
        GUILayout.Label("Download .gitignore", EditorStyles.boldLabel);
        downloadGitIgnore = EditorGUILayout.Toggle("Download .gitignore", downloadGitIgnore);

        if (GUILayout.Button("Download .gitignore"))
        {
            _ = GettingGitIgnore(); // Fire and forget async call
        }

        GUILayout.Space(6);
        GUILayout.Label("Package Management", EditorStyles.boldLabel);
        if (GUILayout.Button("Add/Remove Necessary Packages"))
        {
            _ = AddRemoveNecessaryPackages(); // Fire and forget async call
        }

        GUILayout.Space(8);
        GUILayout.Label("PLANTUml Diagram generator", EditorStyles.boldLabel);
        GUILayout.Label("puml-gen Scripts PlantUml -dir --ignore Private,Protected -createAssociation -allInOne", EditorStyles.miniLabel);
    }

    private void CreateSelectedFolders()
    {
        string projectPath = Application.dataPath;
        string[] folders = new string[]
        {
            createScripts ? "Scripts" : null,
            createMaterials ? "Materials" : null,
            createMusic ? "Music" : null,
            createPrefabs ? "Prefabs" : null,
            createModels ? "Models" : null,
            createTextures ? "Textures" : null,
            createEditor ? "Editor" : null
        };

        foreach (string folder in folders)
        {
            if (folder != null)
            {
                string fullPath = Path.Combine(projectPath, "Project", folder);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    UnityEngine.Debug.Log($"Created folder: {fullPath}");
                }
                else
                {
                    UnityEngine.Debug.Log($"Folder already exists: {fullPath}");
                }
            }
        }

        AssetDatabase.Refresh();
    }

    // Generic package downloader (your existing UI package downloader)
    private static async Task DownloadAndInstallPackage()
    {
        string packageUrl = "https://github.com/Avin19/UnityTools/raw/main/UIPackage.unitypackage"; // Use the correct direct URL
        string packagePath = Path.Combine(Application.dataPath, "..", "UIPackage.unitypackage"); // Save outside "Assets"

        EditorUtility.DisplayProgressBar("Downloading Package", "Downloading package...", 0.1f);
        await DownloadFileAsync(packageUrl, packagePath);
        EditorUtility.ClearProgressBar();

        // Import on main thread
        EditorApplication.delayCall += () => InstallUnityPackage(packagePath);
    }

    // Download & install Google Mobile Ads unitypackage
    private static async Task DownloadAndInstallGoogleMobileAdsPackage()
    {
        string[] packagesToAdd = { "com.unity.ads", "com.unity.textmeshpro" };
        AddPackages(packagesToAdd);
    }

    private static void InstallUnityPackage(string filePath)
    {
        if (File.Exists(filePath))
        {
            AssetDatabase.ImportPackage(filePath, true); // Import with UI confirmation
            UnityEngine.Debug.Log($"Unity Package installed: {filePath}");
        }
        else
        {
            UnityEngine.Debug.LogError("Unity Package not found!");
        }
    }

    private void CreateReadmeFile()
    {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string readmePath = Path.Combine(projectPath, "README.md");

        try
        {
            File.WriteAllText(readmePath, readmeContent);
            UnityEngine.Debug.Log($"README file created at: {readmePath}");
            EditorUtility.DisplayDialog("README Created", $"README.md written to {readmePath}", "OK");
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Error creating README file: {ex.Message}");
            EditorUtility.DisplayDialog("Error", $"Error creating README: {ex.Message}", "OK");
        }
    }

    public static async Task GettingTemplateScripts()
    {
        string folderPath = Application.dataPath;

        string[] fileUrls = {
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/CustomScriptsTemplate.cs",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewScript.cs.txt",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewEnum.cs.Txt",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewScriptableObject.cs.txt",
            "https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewClass.cs.txt"
        };
        string[] fileNames = {
            "CustomScriptsTemplate.cs",
            "NewScript.cs.txt",
            "NewEnum.cs.txt",
            "NewScriptableObject.cs.txt",
            "NewClass.cs.txt"
        };

        EditorUtility.DisplayProgressBar("Downloading Templates", "Downloading template scripts...", 0.05f);

        for (int i = 0; i < fileUrls.Length; i++)
        {
            string fullPath = Path.Combine(folderPath, "Project", "Editor", "Template", fileNames[i]);

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await DownloadFileAsync(fileUrls[i], fullPath);
            float p = 0.05f + 0.9f * ((i + 1) / (float)fileUrls.Length);
            EditorUtility.DisplayProgressBar("Downloading Templates", $"Downloading {fileNames[i]}...", p);
        }

        EditorUtility.ClearProgressBar();
        UnityEngine.Debug.Log("All scripts downloaded successfully.");
        EditorApplication.delayCall += AssetDatabase.Refresh;
    }

    public static async Task GettingGitIgnore()
    {
        string folderPath = Application.dataPath.Replace("/Assets", "");
        string fileUrl = "https://raw.githubusercontent.com/Avin19/UnityTools/main/.gitignore";
        string filePath = Path.Combine(folderPath, ".gitignore");

        EditorUtility.DisplayProgressBar("Downloading .gitignore", "Downloading .gitignore...", 0.05f);
        await DownloadFileAsync(fileUrl, filePath);
        EditorUtility.ClearProgressBar();

        UnityEngine.Debug.Log("Downloaded .gitignore file.");
        EditorApplication.delayCall += AssetDatabase.Refresh;
    }

    // ---------- NEW: Download AdManager.cs from a specific commit ----------
    private static async Task DownloadAdManagerScript()
    {
        string scriptsFolder = Path.Combine(Application.dataPath, "Scripts");
        string savePath = Path.Combine(scriptsFolder, "AdManager.cs");

        try
        {
            // Make sure folder exists
            if (!Directory.Exists(scriptsFolder))
            {
                Directory.CreateDirectory(scriptsFolder);
            }

            // If file exists ask user before overwriting
            if (File.Exists(savePath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Overwrite file?",
                    "Assets/Scripts/AdManager.cs already exists. Do you want to overwrite it?",
                    "Overwrite",
                    "Cancel"
                );

                if (!overwrite)
                {
                    Debug.Log("Download cancelled by user.");
                    return;
                }
            }

            EditorUtility.DisplayProgressBar("Downloading AdManager.cs", "Downloading script...", 0.05f);
            await DownloadFileAsync(AdManagerRawUrl, savePath);
            EditorUtility.ClearProgressBar();

            // Import & refresh on main thread
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath("Assets/Project/Scripts/AdManager.cs");
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }

                EditorUtility.DisplayDialog("Download Complete", "AdManager.cs has been downloaded to Assets/Scripts.", "OK");
            };
        }
        catch (System.Exception ex)
        {
            EditorUtility.ClearProgressBar();
            UnityEngine.Debug.LogError("Error downloading AdManager.cs: " + ex.Message);
            EditorUtility.DisplayDialog("Download Failed", "Error downloading AdManager.cs. See console for details.", "OK");
        }
    }

    // Generic async file downloader used by multiple methods above
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

                // Ensure directory exists
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Write the byte array to the specified file
                await File.WriteAllBytesAsync(filePath, fileBytes);

                UnityEngine.Debug.Log($"Downloaded and saved file to {filePath}");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"An error occurred while downloading {url}: {ex.Message}");
                throw;
            }
        }
    }

    // ---------------- Package Manager helpers ----------------
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
                UnityEngine.Debug.Log($"Successfully added package: {package}");
            }
            else if (request.Status >= StatusCode.Failure)
            {
                UnityEngine.Debug.LogError($"Failed to add package: {package}, Error: {request.Error.message}");
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
                UnityEngine.Debug.Log($"Successfully removed package: {package}");
            }
            else if (request.Status >= StatusCode.Failure)
            {
                UnityEngine.Debug.LogError($"Failed to remove package: {package}, Error: {request.Error.message}");
            }
        }
    }

    private static void Resolve()
    {
        Client.Resolve();
        UnityEngine.Debug.Log("Packages resolved.");
    }
}
