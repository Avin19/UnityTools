using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    // --- Singleton ----------------------------------------------------------------
    public static AdManager Instance { get; private set; }

    // --- Game IDs -----------------------------------------------------------------
    [Header("Game IDs")]
    [SerializeField] private string androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] private string iOSGameId = "YOUR_IOS_GAME_ID";
    [SerializeField] private bool testMode = true;
    private string gameId;

    // --- Banner -------------------------------------------------------------------
    [Header("Banner")]
    [SerializeField] private BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;
    [SerializeField] private string bannerAndroidAdUnitId = "Banner_Android";
    [SerializeField] private string bannerIOSAdUnitId = "Banner_iOS";
    private string bannerAdUnitId;
    private bool bannerLoaded = false;

    // --- Interstitial -------------------------------------------------------------
    [Header("Interstitial")]
    [SerializeField] private string interstitialAndroidAdUnitId = "Interstitial_Android";
    [SerializeField] private string interstitialIOSAdUnitId = "Interstitial_iOS";
    private string interstitialAdUnitId;
    private bool interstitialLoaded = false;

    // --- Rewarded -----------------------------------------------------------------
    [Header("Rewarded")]
    [SerializeField] private string rewardedAndroidAdUnitId = "Rewarded_Android";
    [SerializeField] private string rewardedIOSAdUnitId = "Rewarded_iOS";
    private string rewardedAdUnitId;
    private bool rewardedLoaded = false;

    // Used to remember which adUnitId is showing (so we can route callbacks)
    private string lastShowingAdUnitId = null;

    // ------------------------------------------------------------------------------

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Choose platform-specific IDs
#if UNITY_IOS
        gameId = iOSGameId;
        bannerAdUnitId = bannerIOSAdUnitId;
        interstitialAdUnitId = interstitialIOSAdUnitId;
        rewardedAdUnitId = rewardedIOSAdUnitId;
#elif UNITY_ANDROID
        gameId = androidGameId;
        bannerAdUnitId = bannerAndroidAdUnitId;
        interstitialAdUnitId = interstitialAndroidAdUnitId;
        rewardedAdUnitId = rewardedAndroidAdUnitId;
#else
        // Editor or unsupported platforms
        gameId = androidGameId; // allow testing in editor
        bannerAdUnitId = bannerAndroidAdUnitId;
        interstitialAdUnitId = interstitialAndroidAdUnitId;
        rewardedAdUnitId = rewardedAndroidAdUnitId;
#endif

        InitializeAds();
    }

    #region Initialization
    public void InitializeAds()
    {
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Debug.Log($"AdManager: Initializing Unity Ads (gameId: {gameId}, testMode: {testMode})");
            Advertisement.Initialize(gameId, testMode, this);
        }
        else
        {
            Debug.Log("AdManager: Advertisement already initialized or not supported.");
            // If already initialized, proactively load ads
            if (Advertisement.isInitialized)
            {
                OnInitializationComplete();
            }
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("AdManager: Unity Ads Initialization Complete.");
        // Set banner position
        Advertisement.Banner.SetPosition(bannerPosition);

        // Load all ad types
        LoadBanner();
        LoadInterstitial();
        LoadRewarded();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"AdManager: Unity Ads Initialization Failed: {error} - {message}");
    }
    #endregion

    #region Banner
    public void LoadBanner()
    {
        if (string.IsNullOrEmpty(bannerAdUnitId))
        {
            Debug.LogWarning("AdManager: Banner Ad Unit Id is null/empty for this platform.");
            return;
        }

        Debug.Log("AdManager: Loading banner: " + bannerAdUnitId);

        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(bannerAdUnitId, options);
    }

    public void ShowBanner()
    {
        if (bannerLoaded)
        {
            BannerOptions options = new BannerOptions
            {
                clickCallback = OnBannerClicked,
                hideCallback = OnBannerHidden,
                showCallback = OnBannerShown
            };

            Advertisement.Banner.Show(bannerAdUnitId, options);
        }
        else
        {
            Debug.Log("AdManager: Banner not loaded yet; loading now.");
            LoadBanner();
        }
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
        bannerLoaded = false;
    }

    private void OnBannerLoaded()
    {
        Debug.Log("AdManager: Banner loaded.");
        bannerLoaded = true;
    }

    private void OnBannerError(string message)
    {
        Debug.LogError($"AdManager: Banner Error: {message}");
        bannerLoaded = false;
    }

    private void OnBannerClicked() { /* optional */ }
    private void OnBannerShown() { /* optional */ }
    private void OnBannerHidden() { /* optional */ }
    #endregion

    #region Interstitial
    public void LoadInterstitial()
    {
        if (string.IsNullOrEmpty(interstitialAdUnitId))
        {
            Debug.LogWarning("AdManager: Interstitial Ad Unit Id is null/empty for this platform.");
            return;
        }

        Debug.Log("AdManager: Loading interstitial: " + interstitialAdUnitId);
        Advertisement.Load(interstitialAdUnitId, this);
    }

    public void ShowInterstitial()
    {
        if (interstitialLoaded)
        {
            lastShowingAdUnitId = interstitialAdUnitId;
            Advertisement.Show(interstitialAdUnitId, this);
        }
        else
        {
            Debug.Log("AdManager: Interstitial not loaded. Loading now.");
            LoadInterstitial();
        }
    }
    #endregion

    #region Rewarded
    public void LoadRewarded()
    {
        if (string.IsNullOrEmpty(rewardedAdUnitId))
        {
            Debug.LogWarning("AdManager: Rewarded Ad Unit Id is null/empty for this platform.");
            return;
        }

        Debug.Log("AdManager: Loading rewarded ad: " + rewardedAdUnitId);
        Advertisement.Load(rewardedAdUnitId, this);
    }

    public void ShowRewarded()
    {
        if (rewardedLoaded)
        {
            lastShowingAdUnitId = rewardedAdUnitId;
            Advertisement.Show(rewardedAdUnitId, this);
        }
        else
        {
            Debug.Log("AdManager: Rewarded ad not loaded. Loading now.");
            LoadRewarded();
        }
    }
    #endregion

    #region IUnityAdsLoadListener
    // Called when an ad successfully loads.
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("AdManager: OnUnityAdsAdLoaded: " + adUnitId);

        if (adUnitId.Equals(interstitialAdUnitId))
        {
            interstitialLoaded = true;
        }
        else if (adUnitId.Equals(rewardedAdUnitId))
        {
            rewardedLoaded = true;
        }
        else if (adUnitId.Equals(bannerAdUnitId))
        {
            bannerLoaded = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"AdManager: Error loading Ad Unit {adUnitId}: {error} - {message}");

        if (adUnitId.Equals(interstitialAdUnitId))
        {
            interstitialLoaded = false;
        }
        else if (adUnitId.Equals(rewardedAdUnitId))
        {
            rewardedLoaded = false;
        }
        else if (adUnitId.Equals(bannerAdUnitId))
        {
            bannerLoaded = false;
        }

        // Optionally implement retry/backoff logic here.
    }
    #endregion

    #region IUnityAdsShowListener
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"AdManager: Error showing Ad Unit {adUnitId}: {error} - {message}");

        // mark as not loaded so we can attempt reload
        if (adUnitId.Equals(interstitialAdUnitId))
            interstitialLoaded = false;
        else if (adUnitId.Equals(rewardedAdUnitId))
            rewardedLoaded = false;
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log("AdManager: OnUnityAdsShowStart: " + adUnitId);
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log("AdManager: OnUnityAdsShowClick: " + adUnitId);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"AdManager: OnUnityAdsShowComplete: {adUnitId} - {showCompletionState}");

        if (adUnitId.Equals(interstitialAdUnitId))
        {
            // Interstitial finished - reload for next time
            interstitialLoaded = false;
            LoadInterstitial();
        }
        else if (adUnitId.Equals(rewardedAdUnitId))
        {
            // Rewarded finished: if COMPLETED -> grant reward, then reload
            rewardedLoaded = false;
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("AdManager: Rewarded ad completed - grant reward here.");
                GrantReward();
            }
            LoadRewarded();
        }

        // reset last showing id
        lastShowingAdUnitId = null;
    }
    #endregion

    #region Reward handling (example)
    private void GrantReward()
    {
        // TODO: Implement the actual reward logic for your game, e.g. give coins/lives.
        Debug.Log("AdManager: Granting reward to user (placeholder).");
    }
    #endregion

    // Optional helper methods for external scripts ------------------------------------------------
    // Example: external script calls AdManager.Instance.ShowInterstitial();
    // or AdManager.Instance.ShowRewarded();

    private void OnDestroy()
    {
        // clean up singleton reference (if this instance was destroyed)
        if (Instance == this)
            Instance = null;
    }
}
