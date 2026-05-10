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
    private const string GitHubToolsRawBase = "https://raw.githubusercontent.com/Avin19/UnityTools/main/";

    /// <summary>Direct raw URL for the UI .unitypackage (single source of truth).</summary>
    private static readonly string UnityPackageRawUrl = $"{GitHubToolsRawBase}UIPackage.unitypackage";

    private const string UnityPackageFileName = "UIPackage.unitypackage";

    /// <summary>Official Google Mobile Ads Unity plugin release (binary).</summary>
    private const string GoogleMobileAdsUnityPackageUrl =
        "https://github.com/googleads/googleads-mobile-unity/releases/download/v10.6.0/GoogleMobileAds-v10.6.0.unitypackage";

    private const string GoogleMobileAdsUnityPackageFileName = "GoogleMobileAds-v10.6.0.unitypackage";

    private bool createScripts = true;
    private bool createSprite = true;
    private bool createMaterials = true;
    private bool createMusic = true;
    private bool createPrefabs = true;
    private bool createModels = true;
    private bool createTextures = true;
    private bool createEditor = true;
    private bool createScene = true;
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

### Checked-in diagram output (optional)

If you commit generated diagrams:

![Class Diagram](out/PlantUml/include/include.svg)
![Class Diagram](out/PlantUml/include/include.png)

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

    private const string AdMobManagerRawUrl =
        "https://raw.githubusercontent.com/Avin19/UnityTools/refs/heads/main/Ads/AdMobManager.cs";
    private const string GdprConsentRawUrl =
        "https://raw.githubusercontent.com/Avin19/UnityTools/refs/heads/main/Ads/GdprConsentManager.cs";
    private const string AdConfigRawUrl =
        "https://raw.githubusercontent.com/Avin19/UnityTools/refs/heads/main/Ads/AdConfig.cs";

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
        createScene = EditorGUILayout.Toggle("Scene", createScene);

        if (GUILayout.Button("Create Folders"))
        {
            CreateSelectedFolders();
        }

        GUILayout.Space(8);
        GUILayout.Label("UI Package Management", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox($"UI package URL:\n{UnityPackageRawUrl}", MessageType.Info);
        if (GUILayout.Button("Download & Install UnityPackage"))
        {
            _ = DownloadAndInstallUnityPackageAsync(UnityPackageRawUrl, UnityPackageFileName);
        }

        GUILayout.Space(6);
        GUILayout.Label("Google Mobile Ads", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            $"Official release (.unitypackage):\n{GoogleMobileAdsUnityPackageUrl}",
            MessageType.Info);
        if (GUILayout.Button("Download & Install Google Mobile Ads (v10.6.0)"))
        {
            _ = DownloadAndInstallUnityPackageAsync(GoogleMobileAdsUnityPackageUrl, GoogleMobileAdsUnityPackageFileName);
        }

        GUILayout.Space(6);
        GUILayout.Label("Ad scripts", EditorStyles.boldLabel);
        if (GUILayout.Button("Download AdMob scripts from GitHub (Ads folder)"))
        {
            _ = DownloadAdMobScriptsAsync();
        }

        GUILayout.Space(6);
        GUILayout.Label("Player data scripts", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Downloads `PlayerData.cs` and `PlayerDataManager.cs` from the repo into Assets/Project/Script/playerData/",
            MessageType.Info);
        if (GUILayout.Button("Download PlayerData scripts"))
        {
            _ = DownloadPlayerDataFromRepoAsync();
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
            _ = GettingTemplateScripts();
        }

        GUILayout.Space(6);
        GUILayout.Label("Scene folder (repo)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Downloads files from the remote `Scene/` folder into Assets/Project/Scene. Use Create Folders (Scene) first, or this button will create the folder.",
            MessageType.Info);
        if (GUILayout.Button("Download Scene folder from repo"))
        {
            _ = DownloadSceneFolderFromRepoAsync();
        }

        GUILayout.Space(6);
        GUILayout.Label("Download .gitignore", EditorStyles.boldLabel);
        downloadGitIgnore = EditorGUILayout.Toggle("Download .gitignore when clicking button", downloadGitIgnore);

        using (new EditorGUI.DisabledScope(!downloadGitIgnore))
        {
            if (GUILayout.Button("Download .gitignore"))
            {
                _ = GettingGitIgnore();
            }
        }

        GUILayout.Space(6);
        GUILayout.Label("Package Management", EditorStyles.boldLabel);
        if (GUILayout.Button("Add/Remove Necessary Packages"))
        {
            _ = AddRemoveNecessaryPackages();
        }

        GUILayout.Space(6);
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
            createEditor ? "Editor" : null,
            createScene ? "Scene" : null
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
        try
        {
            EditorUtility.DisplayProgressBar("Downloading package", fileName, 0.1f);
            bool ok = await DownloadFileAsync(rawUrl, packagePath);
            if (ok)
                InstallUnityPackage(packagePath);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
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
            EditorUtility.DisplayDialog("README created", $"README.md written to:\n{readmePath}", "OK");
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error creating README file: {ex.Message}");
            EditorUtility.DisplayDialog("Error", $"Could not create README:\n{ex.Message}", "OK");
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
        try
        {
            for (int i = 0; i < templates.Length; i++)
            {
                string fullPath = Path.Combine(folderPath, "Project", "Editor", "Template", templates[i].fileName);
                string dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                float p = 0.05f + 0.9f * ((i + 1) / (float)templates.Length);
                EditorUtility.DisplayProgressBar("Downloading templates", templates[i].fileName, p);

                bool ok = await DownloadFileAsync(templates[i].url, fullPath);
                if (!ok)
                    failures++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (failures == 0)
            Debug.Log("All scripts downloaded successfully.");
        else
            Debug.LogError($"Script download finished with {failures} failure(s). Check the Console for details.");

        EditorApplication.delayCall += AssetDatabase.Refresh;
    }

    /// <summary>
    /// Files under <c>Scene/</c> on the UnityTools repo. Add entries here when new scene assets are published.
    /// </summary>
    private static readonly string[] SceneRepoRelativePaths =
    {
        "Scene/BootStrap.unity",
        "Scene/BootstrapLoadingUI.cs"
    };

    public static async Task DownloadSceneFolderFromRepoAsync()
    {
        string sceneRoot = Path.Combine(Application.dataPath, "Project", "Scene");
        if (!Directory.Exists(sceneRoot))
            Directory.CreateDirectory(sceneRoot);

        int failures = 0;
        try
        {
            for (int i = 0; i < SceneRepoRelativePaths.Length; i++)
            {
                string relative = SceneRepoRelativePaths[i];
                string fileName = Path.GetFileName(relative);
                string url = GitHubToolsRawBase + relative.Replace('\\', '/');
                string fullPath = Path.Combine(sceneRoot, fileName);

                float p = 0.05f + 0.9f * ((i + 1) / (float)SceneRepoRelativePaths.Length);
                EditorUtility.DisplayProgressBar("Downloading Scene folder", fileName, p);

                bool ok = await DownloadFileAsync(url, fullPath);
                if (!ok)
                    failures++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (failures == 0)
            Debug.Log($"Scene folder downloaded to: {sceneRoot}");
        else
            Debug.LogError($"Scene download finished with {failures} failure(s). Check the Console.");

        EditorApplication.delayCall += () =>
        {
            AssetDatabase.Refresh();
            const string sceneAssetPath = "Assets/Project/Scene/BootStrap.unity";
            Object sceneAsset = AssetDatabase.LoadMainAssetAtPath(sceneAssetPath);
            if (sceneAsset != null)
                EditorGUIUtility.PingObject(sceneAsset);
        };
    }

    /// <summary>Files under <c>playerData/</c> on the UnityTools repo (see <c>GitHubToolsRawBase</c>).</summary>
    private static readonly string[] PlayerDataRepoRelativePaths =
    {
        "playerData/PlayerData.cs",
        "playerData/PlayerDataManager.cs"
    };

    public static async Task DownloadPlayerDataFromRepoAsync()
    {
        string destRoot = Path.Combine(Application.dataPath, "Project", "Script", "playerData");
        if (!Directory.Exists(destRoot))
            Directory.CreateDirectory(destRoot);

        int failures = 0;
        try
        {
            for (int i = 0; i < PlayerDataRepoRelativePaths.Length; i++)
            {
                string relative = PlayerDataRepoRelativePaths[i];
                string fileName = Path.GetFileName(relative);
                string url = GitHubToolsRawBase + relative.Replace('\\', '/');
                string fullPath = Path.Combine(destRoot, fileName);

                if (File.Exists(fullPath))
                {
                    bool overwrite = EditorUtility.DisplayDialog(
                        "Overwrite file?",
                        $"Assets/Project/Script/playerData/{fileName} already exists. Overwrite?",
                        "Overwrite",
                        "Cancel");
                    if (!overwrite)
                    {
                        Debug.Log("PlayerData download cancelled by user.");
                        return;
                    }
                }

                float p = 0.05f + 0.9f * ((i + 1) / (float)PlayerDataRepoRelativePaths.Length);
                EditorUtility.DisplayProgressBar("Downloading PlayerData", fileName, p);

                bool ok = await DownloadFileAsync(url, fullPath);
                if (!ok)
                    failures++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (failures == 0)
            Debug.Log($"PlayerData scripts downloaded to: {destRoot}");
        else
            Debug.LogError($"PlayerData download finished with {failures} failure(s). Check the Console.");

        EditorApplication.delayCall += () =>
        {
            AssetDatabase.Refresh();
            Object script = AssetDatabase.LoadMainAssetAtPath("Assets/Project/Script/playerData/PlayerDataManager.cs");
            if (script != null)
                EditorGUIUtility.PingObject(script);
        };
    }

    public static async Task GettingGitIgnore()
    {
        string filePath = Path.Combine(GetProjectRoot(), ".gitignore");
        string fileUrl = $"{GitHubToolsRawBase}.gitignore";
        try
        {
            EditorUtility.DisplayProgressBar("Downloading .gitignore", "Downloading...", 0.2f);
            bool ok = await DownloadFileAsync(fileUrl, filePath);
            if (ok)
            {
                Debug.Log("Downloaded .gitignore file.");
                EditorApplication.delayCall += AssetDatabase.Refresh;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static async Task DownloadAdMobScriptsAsync()
    {
        string scriptsFolder = Path.Combine(Application.dataPath, "Project", "Script", "Ads");
        const string relativePingPath = "Assets/Project/Script/Ads/AdMobManager.cs";

        (string url, string fileName)[] files =
        {
            (AdMobManagerRawUrl, "AdMobManager.cs"),
            (GdprConsentRawUrl, "GdprConsentManager.cs"),
            (AdConfigRawUrl, "AdConfig.cs")
        };

        try
        {
            if (!Directory.Exists(scriptsFolder))
                Directory.CreateDirectory(scriptsFolder);

            for (int i = 0; i < files.Length; i++)
            {
                string savePath = Path.Combine(scriptsFolder, files[i].fileName);
                if (File.Exists(savePath))
                {
                    bool overwrite = EditorUtility.DisplayDialog(
                        "Overwrite file?",
                        $"Assets/Project/Script/Ads/{files[i].fileName} already exists. Overwrite?",
                        "Overwrite",
                        "Cancel");
                    if (!overwrite)
                    {
                        Debug.Log("Download cancelled by user.");
                        return;
                    }
                }

                float p = 0.1f + 0.8f * ((i + 1) / (float)files.Length);
                EditorUtility.DisplayProgressBar("Downloading AdMob scripts", files[i].fileName, p);
                bool ok = await DownloadFileAsync(files[i].url, savePath);
                if (!ok)
                {
                    EditorUtility.DisplayDialog("Download failed", $"Could not download {files[i].fileName}. See Console.", "OK");
                    return;
                }
            }

            EditorUtility.DisplayDialog("Download complete", $"Scripts saved under:\n{scriptsFolder}", "OK");

            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                Object asset = AssetDatabase.LoadMainAssetAtPath(relativePingPath);
                if (asset != null)
                    EditorGUIUtility.PingObject(asset);
            };
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error downloading AdMob scripts: " + ex.Message);
            EditorUtility.DisplayDialog("Download failed", "See Console for details.", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
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

                string dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

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
