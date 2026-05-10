using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public class ScriptDownloaderEditor : EditorWindow
{
    private const string GitHubToolsRawBase = "https://raw.githubusercontent.com/Avin19/UnityTools/main/";

    /// <summary>Direct raw URL for the UI .unitypackage (single source of truth).</summary>
    private static readonly string UnityPackageRawUrl = $"{GitHubToolsRawBase}UIPackage.unitypackage";

    private const string UnityPackageFileName = "UIPackage.unitypackage";

    private const string GoogleMobileAdsPackageFileName = "GoogleMobileAds-v10.6.0.unitypackage";

    /// <summary>Direct raw URL for the Google Mobile Ads .unitypackage (same repo as other tool assets).</summary>
    private static readonly string GoogleMobileAdsPackageRawUrl = $"{GitHubToolsRawBase}{GoogleMobileAdsPackageFileName}";

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
@"# [Project name]

One sentence describing what this Unity project is.

## Overview

Add 2–4 sentences: what players or users get, target platforms, and anything important about scope or production stage.

## Requirements

- Unity Editor: **2021.3 LTS** or newer (change this to match your `ProjectSettings/ProjectVersion.txt`).
- Platform modules you need (Android, iOS, WebGL, etc.) installed via Unity Hub.

## Getting started

1. Clone the repository.
2. Open the project in Unity Hub with the Editor version above.
3. Open the main scene you use for iteration (name it here once it exists).

## Repository layout

- `Assets/` — scenes, art, audio, and gameplay code.
- `Assets/Project/` — optional convention for structured folders (Scripts, Prefabs, Materials, etc.) if your team uses that layout.

## Features (product)

Replace this list with what your game or app actually does.

- Core loop:
- Progression or content structure:
- Notable tech (input, networking, ads/IAP, analytics):

## Tooling (optional)

If this repo uses **Tools > Setup > Script Downloader**, that window can create common `Assets/Project` folders, fetch template editor scripts, download a Unity-focused `.gitignore`, adjust default packages, and import optional `.unitypackage` assets. Remove this section if it does not apply to your project.

## Documentation

- Design notes: (link to wiki, Notion, or a `Docs/` folder.)
- Build and release: (how you ship; signing, store listings, CI.)

## PlantUML (optional)

If you generate UML from C#:

```bash
puml-gen Scripts PlantUml -dir --ignore Private,Protected -createAssociation -allInOne
```

More context: [PlantUmlClassDiagramGenerator](https://github.com/pierre3/PlantUmlClassDiagramGenerator).

## Screenshots

Add images under something like `Docs/Screenshots/` and reference them here.

<!-- ![Example](Docs/Screenshots/example.png) -->

## Contributing

Issues and pull requests are welcome. For larger changes, open an issue first so direction stays aligned.

## License

State your license here (for example MIT, or proprietary / all rights reserved).

## Credits

List third-party assets, audio, fonts, code packages, and people here.
";

    [MenuItem("Tools/Setup/Script Downloader")]
    public static void ShowWindow()
    {
        GetWindow<ScriptDownloaderEditor>("Script Downloader");
    }

    /// <summary>Project root (parent of Assets), works on Windows/macOS/Linux.</summary>
    private static string GetProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    private void OnGUI()
    {
        GUILayout.Label("Folder Setup", EditorStyles.boldLabel);
        createScripts = EditorGUILayout.Toggle("Scripts", createScripts);
        createSprite = EditorGUILayout.Toggle("Sprites", createSprite);
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
        GUILayout.Label("UI Package Management", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox($"UI package URL:\n{UnityPackageRawUrl}", MessageType.Info);
        if (GUILayout.Button("Download & Install UnityPackage"))
        {
            _ = DownloadAndInstallUnityPackageAsync(UnityPackageRawUrl, UnityPackageFileName);
        }

        GUILayout.Label("Google Mobile Ads", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            $"Google Mobile Ads v10.6.0 (.unitypackage)\n{GoogleMobileAdsPackageRawUrl}\n\nHost this file on the same branch as other tool downloads, or download fails until it is published.",
            MessageType.Info);
        if (GUILayout.Button("Download & Install Google Mobile Ads (v10.6.0)"))
        {
            _ = DownloadAndInstallUnityPackageAsync(GoogleMobileAdsPackageRawUrl, GoogleMobileAdsPackageFileName);
        }
        GUILayout.Label("README Setup", EditorStyles.boldLabel);
        readmeContent = EditorGUILayout.TextArea(readmeContent, GUILayout.Height(200));

        if (GUILayout.Button("Create README"))
        {
            CreateReadmeFile();
        }

        GUILayout.Label("Download Scripts", EditorStyles.boldLabel);
        if (GUILayout.Button("Download Scripts"))
        {
            _ = GettingTemplateScripts();
        }

        GUILayout.Label("Download .gitignore", EditorStyles.boldLabel);
        downloadGitIgnore = EditorGUILayout.Toggle("Download .gitignore when clicking button", downloadGitIgnore);

        using (new EditorGUI.DisabledScope(!downloadGitIgnore))
        {
            if (GUILayout.Button("Download .gitignore"))
            {
                _ = GettingGitIgnore();
            }
        }

        GUILayout.Label("Package Management", EditorStyles.boldLabel);
        if (GUILayout.Button("Add/Remove Necessary Packages"))
        {
            _ = AddRemoveNecessaryPackages();
        }
        GUILayout.Label("PlantUML diagram generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "External tool (example): puml-gen Scripts PlantUml -dir --ignore Private,Protected -createAssociation -allInOne",
            MessageType.None);
    }

    private void CreateSelectedFolders()
    {
        string projectPath = Application.dataPath;
        string[] folders = new string[]
        {
            createScripts ? "Scripts" : null,
            createSprite ? "Sprites" : null,
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
                    Debug.Log($"Created folder: {fullPath}");
                }
                else
                {
                    Debug.Log($"Folder already exists: {fullPath}");
                }
            }
        }

        AssetDatabase.Refresh();
    }

    /// <summary>Downloads a .unitypackage to the project root, then runs Unity's import dialog.</summary>
    private static async Task DownloadAndInstallUnityPackageAsync(string rawUrl, string fileName)
    {
        string packagePath = Path.GetFullPath(Path.Combine(GetProjectRoot(), fileName));
        bool ok = await DownloadFileAsync(rawUrl, packagePath);
        if (ok)
            InstallUnityPackage(packagePath);
    }

    private static void InstallUnityPackage(string filePath)
    {
        if (File.Exists(filePath))
        {
            AssetDatabase.ImportPackage(filePath, true);
            Debug.Log($"Unity Package installed: {filePath}");
        }
        else
        {
            Debug.LogError("Unity Package not found!");
        }
    }

    private void CreateReadmeFile()
    {
        string readmePath = Path.Combine(GetProjectRoot(), "README.md");

        try
        {
            File.WriteAllText(readmePath, readmeContent);
            Debug.Log($"README file created at: {readmePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error creating README file: {ex.Message}");
        }
    }

    public static async Task GettingTemplateScripts()
    {
        string folderPath = Application.dataPath;

        (string url, string fileName)[] templates =
        {
            ($"{GitHubToolsRawBase}CustomScriptsTemplate.cs", "CustomScriptsTemplate.cs"),
            ($"{GitHubToolsRawBase}Template/NewScript.cs.txt", "NewScript.cs.txt"),
            ($"{GitHubToolsRawBase}Template/NewEnum.cs.Txt", "NewEnum.cs.txt"),
            ($"{GitHubToolsRawBase}Template/NewScriptableObject.cs.txt", "NewScriptableObject.cs.txt"),
            ($"{GitHubToolsRawBase}Template/NewClass.cs.txt", "NewClass.cs.txt")
        };

        int failures = 0;
        for (int i = 0; i < templates.Length; i++)
        {
            string fullPath = Path.Combine(folderPath, "Project", "Editor", "Template", templates[i].fileName);
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            bool ok = await DownloadFileAsync(templates[i].url, fullPath);
            if (!ok)
                failures++;
        }

        if (failures == 0)
            Debug.Log("All scripts downloaded successfully.");
        else
            Debug.LogError($"Script download finished with {failures} failure(s). Check the Console for details.");
    }

    public static async Task GettingGitIgnore()
    {
        string filePath = Path.Combine(GetProjectRoot(), ".gitignore");
        string fileUrl = $"{GitHubToolsRawBase}.gitignore";
        bool ok = await DownloadFileAsync(fileUrl, filePath);
        if (ok)
            Debug.Log("Downloaded .gitignore file.");
    }

    /// <returns>true if the file was written successfully.</returns>
    private static async Task<bool> DownloadFileAsync(string url, string filePath)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Unity-ScriptDownloaderEditor/1.0");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, fileBytes);
                Debug.Log($"Downloaded and saved file to {filePath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"An error occurred while downloading {url}: {ex.Message}");
                return false;
            }
        }
    }

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

    private static void Resolve()
    {
        Client.Resolve();
        Debug.Log("Packages resolved.");
    }
}
