using System;

/// <summary>
/// Serializable player profile, economy, level progress, audio/haptics, and daily rewards.
/// Call <see cref="Normalize"/> after loading from disk or before persisting so invariants hold.
/// </summary>
[Serializable]
public class PlayerData
{
    /// <summary>Bump when you change fields so <c>PlayerDataManager</c> can migrate old JSON.</summary>
    public int SaveVersion = CurrentSaveVersion;

    public const int CurrentSaveVersion = 1;

    // --- Profile ---
    public string PlayerName = "";
    public string PlayerID = "";

    // --- Currency & consumables ---
    public int Coins;
    /// <summary>Hint consumables available to spend.</summary>
    public int Hints;

    // --- Level progress ---
    public int CurrentLevel = 1;
    public int HighestUnlockedLevel = 1;
    public int LevelStarsTotal;

    // --- Audio & haptics (0–100 for UI sliders; map to 0–1 for mixers as needed) ---
    public bool SoundEnabled = true;
    public bool MusicEnabled = true;
    public float MusicVolume = 100f;
    public float SFXVolume = 100f;
    public bool Haptic = true;

    // --- Daily rewards ---
    public string LastDailyRewardDate = "";
    public int DailyRewardStreak;

    // --- Meta / onboarding ---
    public bool TutorialCompleted;
    public long TotalPlayTimeSeconds;
    public string LastSaveUtc = "";

    /// <summary>Clamps economy, volumes, levels, and streaks to sensible ranges.</summary>
    public void Normalize()
    {
        if (SaveVersion < 1)
            SaveVersion = 1;

        Coins = Math.Max(0, Coins);
        Hints = Math.Max(0, Hints);
        LevelStarsTotal = Math.Max(0, LevelStarsTotal);
        DailyRewardStreak = Math.Max(0, DailyRewardStreak);
        TotalPlayTimeSeconds = Math.Max(0, TotalPlayTimeSeconds);

        MusicVolume = Clamp01To100(MusicVolume);
        SFXVolume = Clamp01To100(SFXVolume);

        CurrentLevel = Math.Max(1, CurrentLevel);
        HighestUnlockedLevel = Math.Max(1, HighestUnlockedLevel);
        if (HighestUnlockedLevel < CurrentLevel)
            HighestUnlockedLevel = CurrentLevel;
    }

    private static float Clamp01To100(float v)
    {
        if (v < 0f) return 0f;
        if (v > 100f) return 100f;
        return v;
    }

    public static PlayerData CreateDefault()
    {
        var d = new PlayerData
        {
            SaveVersion = CurrentSaveVersion,
            PlayerName = "",
            PlayerID = "",
            Coins = 0,
            Hints = 0,
            CurrentLevel = 1,
            HighestUnlockedLevel = 1,
            LevelStarsTotal = 0,
            SoundEnabled = true,
            MusicEnabled = true,
            MusicVolume = 100f,
            SFXVolume = 100f,
            Haptic = true,
            LastDailyRewardDate = "",
            DailyRewardStreak = 0,
            TutorialCompleted = false,
            TotalPlayTimeSeconds = 0,
            LastSaveUtc = ""
        };
        d.Normalize();
        return d;
    }
}
