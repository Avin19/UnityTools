using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Singleton that loads/saves <see cref="PlayerData"/> to <c>Application.persistentDataPath</c>.
/// Writes atomically and keeps a single <c>.bak</c> backup of the last committed save.
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public const string SaveFileName = "player.json";
    public const string SaveBackupExtension = ".bak";

    public static PlayerDataManager Instance { get; private set; }

    public PlayerData data;

    public int Coins => data != null ? data.Coins : 0;
    public int Hint => data != null ? data.Hints : 0;
    public bool SFX => data != null && data.SoundEnabled;
    public bool Music => data != null && data.MusicEnabled;
    public bool Haptic => data != null && data.Haptic;
    public string PlayerId => data != null ? data.PlayerID : "";
    public string PlayerName => data != null ? data.PlayerName : "";

    private string _savePath;
    private string SavePath => _savePath ??= Path.Combine(Application.persistentDataPath, SaveFileName);

    private string BackupPath => SavePath + SaveBackupExtension;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #region SAVE

    /// <summary>Writes JSON atomically and refreshes <see cref="PlayerData.LastSaveUtc"/>.</summary>
    public void Save()
    {
        if (data == null)
            data = PlayerData.CreateDefault();

        data.Normalize();
        data.LastSaveUtc = DateTime.UtcNow.ToString("o");

        string json = JsonUtility.ToJson(data, true);
        try
        {
            WriteAtomicWithBackup(SavePath, BackupPath, json);
            Debug.Log("Game saved.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Game save failed: {ex.Message}");
        }
    }

    private static void WriteAtomicWithBackup(string finalPath, string backupPath, string contents)
    {
        if (File.Exists(finalPath))
        {
            try
            {
                File.Copy(finalPath, backupPath, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not write save backup: {ex.Message}");
            }
        }

        string tempPath = finalPath + ".tmp";
        File.WriteAllText(tempPath, contents);
        try
        {
            if (File.Exists(finalPath))
                File.Replace(tempPath, finalPath, null);
            else
                File.Move(tempPath, finalPath);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch { /* ignored */ }
            }
        }
    }

    #endregion

    #region LOAD

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<PlayerData>(json);
                if (data == null)
                    data = PlayerData.CreateDefault();
                else
                    MigrateIfNeeded(data);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Save load failed: {ex.Message}. Trying backup.");
                data = TryLoadBackupOrDefault();
            }

            data.Normalize();
            Debug.Log("Save loaded.");
        }
        else
        {
            CreateNewSave();
            return;
        }

        if (EnsureProfileIds())
            Save();
    }

    private PlayerData TryLoadBackupOrDefault()
    {
        if (!File.Exists(BackupPath))
            return PlayerData.CreateDefault();

        try
        {
            string json = File.ReadAllText(BackupPath);
            var loaded = JsonUtility.FromJson<PlayerData>(json);
            return loaded ?? PlayerData.CreateDefault();
        }
        catch
        {
            return PlayerData.CreateDefault();
        }
    }

    private static void MigrateIfNeeded(PlayerData d)
    {
        if (d.SaveVersion < 1)
            d.SaveVersion = 1;

        // Example for future versions:
        // if (d.SaveVersion == 1) { ... d.SaveVersion = 2; }
    }

    private void CreateNewSave()
    {
        data = PlayerData.CreateDefault();
        EnsureProfileIds();
        Save();
        Debug.Log("New player data created.");
    }

    /// <summary>Fills empty <see cref="PlayerData.PlayerID"/> / <see cref="PlayerData.PlayerName"/>. Does not save.</summary>
    private bool EnsureProfileIds()
    {
        if (data == null)
            data = PlayerData.CreateDefault();

        bool changed = false;
        if (string.IsNullOrEmpty(data.PlayerID))
        {
            data.PlayerID = GenerateUniqueId();
            Debug.Log("Generated player ID: " + data.PlayerID);
            changed = true;
        }

        if (string.IsNullOrEmpty(data.PlayerName))
        {
            data.PlayerName = GenerateRandomName();
            Debug.Log("Generated username: " + data.PlayerName);
            changed = true;
        }

        return changed;
    }

    private static string GenerateUniqueId()
    {
        return Guid.NewGuid().ToString();
    }

    private static string GenerateRandomName()
    {
        string[] adjectives = { "Silent", "Dark", "Shadow", "Swift", "Deadly", "Ghost", "Hidden", "Night" };
        string[] nouns = { "Hunter", "Assassin", "Ninja", "Sniper", "Blade", "Reaper", "Stalker", "Phantom" };

        string adj = adjectives[Random.Range(0, adjectives.Length)];
        string noun = nouns[Random.Range(0, nouns.Length)];
        int number = Random.Range(10, 999);

        return adj + noun + number;
    }

    #endregion

    #region CURRENCY

    public void AddCoins(int amount)
    {
        if (data == null || amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning("AddCoins: negative amount ignored. Use SpendCoins instead.");
            return;
        }

        data.Coins += amount;
        Save();
    }

    public bool SpendCoins(int amount)
    {
        if (data == null || amount <= 0) return false;
        if (data.Coins >= amount)
        {
            data.Coins -= amount;
            Save();
            return true;
        }

        return false;
    }

    public void AddHints(int amount)
    {
        if (data == null || amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning("AddHints: negative amount ignored. Use SpendHints instead.");
            return;
        }

        data.Hints += amount;
        Save();
    }

    public bool SpendHints(int amount)
    {
        if (data == null || amount <= 0) return false;
        if (data.Hints >= amount)
        {
            data.Hints -= amount;
            Save();
            return true;
        }

        return false;
    }

    #endregion

    #region LEVEL

    /// <summary>Call when the player finishes <paramref name="completedLevelIndex"/> (1-based level number).</summary>
    public void CompleteLevel(int completedLevelIndex)
    {
        if (data == null || completedLevelIndex < 1) return;

        int nextLevel = completedLevelIndex + 1;
        data.HighestUnlockedLevel = Math.Max(data.HighestUnlockedLevel, nextLevel);
        data.CurrentLevel = Math.Max(data.CurrentLevel, nextLevel);
        Save();
    }

    #endregion

    #region SETTINGS

    public void SetMusicVolume(float volumePercent)
    {
        if (data == null) return;
        data.MusicVolume = Mathf.Clamp(volumePercent, 0f, 100f);
        Save();
    }

    public void SetSfxVolume(float volumePercent)
    {
        if (data == null) return;
        data.SFXVolume = Mathf.Clamp(volumePercent, 0f, 100f);
        Save();
    }

    #endregion

    #region APPLICATION

    private void OnApplicationPause(bool pause)
    {
        if (pause && data != null)
            Save();
    }

    private void OnApplicationQuit()
    {
        if (data != null)
            Save();
    }

    #endregion
}
