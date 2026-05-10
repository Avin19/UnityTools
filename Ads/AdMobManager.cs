using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance { get; private set; }

    [SerializeField] private AdConfig config;

    // =========================
    // Banner
    // =========================
    private BannerView bannerView;
    private bool isBannerLoaded;
    private bool isBannerLoading;
    private float currentBannerRetryDelay;

    // =========================
    // Interstitial
    // =========================
    private InterstitialAd interstitialAd;
    private int interstitialEventCounter;
    private float lastInterstitialTime;

    // =========================
    // Rewarded
    // =========================
    private RewardedAd rewardedAd;
    private bool isRewardedLoading;
    private bool isRewardedReady;

    // =========================
    // Rewarded Interstitial
    // =========================
    private RewardedInterstitialAd rewardedInterstitialAd;
    private bool isRewardedInterstitialLoading;
    private bool isRewardedInterstitialReady;

    // =========================
    // Native Advanced (Overlay)
    // =========================
    private NativeOverlayAd nativeOverlayAd;
    private bool isNativeLoading;
    private bool isNativeReady;

    // =========================
    // App Open
    // =========================
    private AppOpenAd appOpenAd;
    private bool isAppOpenLoading;
    private bool isAppOpenReady;
    private float lastAppOpenShownTime;
    private bool isInitialized;
    private bool canRequestAds;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("AdConfig missing");
            return;
        }

        currentBannerRetryDelay = config.bannerRetryInitialDelay;
        canRequestAds = !config.enableGdprConsent || !config.requestConsentOnStartup;

        if (config.enableGdprConsent && config.requestConsentOnStartup)
        {
            // ✅ GDPR-enabled flow
            GdprConsentManager.RequestConsent(OnConsentResolved);
        }
        else
        {
            // ⚠️ GDPR bypass (DEV / NON-EEA ONLY)
            Debug.LogWarning("GDPR consent disabled via AdConfig");
            InitializeAdMob();
        }
    }
    private void OnConsentResolved()
    {
        Debug.Log($"Consent resolved. CanRequestAds = {GdprConsentManager.CanRequestAds}");

        if (!GdprConsentManager.CanRequestAds)
        {
            Debug.LogWarning("Ads blocked due to GDPR consent");
            return;
        }

        canRequestAds = true;
        InitializeAdMob();
    }
    private void InitializeAdMob()
    {
        if (isInitialized)
            return;

        MobileAds.Initialize(_ =>
        {
            Debug.Log("AdMob initialized");
            isInitialized = true;

            if (config.loadBannerOnStart)
                LoadBanner();

            LoadInterstitial();

            if (config.preloadRewarded)
                LoadRewarded();

            if (config.preloadRewardedInterstitial)
                LoadRewardedInterstitial();

            if (config.loadNativeOnStart)
                LoadNativeAdvanced();

            if (config.loadAppOpenOnStart)
                LoadAppOpen();
        });
    }


    private void OnDestroy()
    {
        CancelInvoke();
        bannerView?.Destroy();
        interstitialAd?.Destroy();
        rewardedAd?.Destroy();
        rewardedInterstitialAd?.Destroy();
        nativeOverlayAd?.Destroy();
        appOpenAd?.Destroy();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            TryShowAppOpen();
    }

    #endregion

    // =========================================================
    // BANNER
    // =========================================================

    private void LoadBanner()
    {
        if (!canRequestAds)
            return;
        if (string.IsNullOrWhiteSpace(GetBannerAdUnitId()))
        {
            Debug.LogWarning("Banner ad unit id missing.");
            return;
        }
        if (isBannerLoading || bannerView != null)
            return;

        isBannerLoading = true;
        isBannerLoaded = false;

        // 🔥 Adaptive banner (recommended)
        AdSize adaptiveSize =
            AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                AdSize.FullWidth);

        bannerView = new BannerView(
            GetBannerAdUnitId(),
            adaptiveSize,
            AdPosition.Bottom
        );

        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner loaded");
            isBannerLoaded = true;
            isBannerLoading = false;

            // reset retry delay on success
            currentBannerRetryDelay = config.bannerRetryInitialDelay;

            if (config.autoShowBanner)
                bannerView.Show();
        };

        bannerView.OnBannerAdLoadFailed += error =>
        {
            Debug.LogWarning($"Banner failed (No fill is normal): {error}");
            isBannerLoaded = false;
            isBannerLoading = false;
            ScheduleBannerRetry();
        };

        bannerView.LoadAd(new AdRequest());
    }

    private void ScheduleBannerRetry()
    {
        bannerView?.Destroy();
        bannerView = null;

        Debug.Log($"Retrying banner in {currentBannerRetryDelay} seconds");

        Invoke(nameof(LoadBanner), currentBannerRetryDelay);

        currentBannerRetryDelay = Mathf.Min(
            currentBannerRetryDelay * 2f,
            config.bannerRetryMaxDelay
        );
    }

    public bool IsBannerLoaded() => isBannerLoaded;

    public void ShowBanner()
    {
        if (!isBannerLoaded || bannerView == null)
        {
            LoadBanner();
            return;
        }

        bannerView.Show();
    }

    public void HideBanner()
    {
        bannerView?.Hide();
    }

    // =========================================================
    // INTERSTITIAL
    // =========================================================

    private void LoadInterstitial()
    {
        if (!canRequestAds)
            return;
        if (string.IsNullOrWhiteSpace(GetInterstitialAdUnitId()))
        {
            Debug.LogWarning("Interstitial ad unit id missing.");
            return;
        }
        interstitialAd?.Destroy();
        interstitialAd = null;

        InterstitialAd.Load(
            GetInterstitialAdUnitId(),
            new AdRequest(),
            (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning("Interstitial load failed: " + error);
                    Invoke(nameof(LoadInterstitial), Mathf.Max(2f, config.interstitialRetryDelaySeconds));
                    return;
                }

                interstitialAd = ad;
                Debug.Log("Interstitial loaded");
                interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitial();
                interstitialAd.OnAdFullScreenContentFailed += _ => LoadInterstitial();
            });
    }

    /// <summary>
    /// Call on GameOver / LevelComplete
    /// </summary>
    public void TryShowInterstitial()
    {
        interstitialEventCounter++;

        if (interstitialEventCounter < config.interstitialEveryNEvents)
            return;

        if (Time.time - lastInterstitialTime < config.interstitialCooldownSeconds)
            return;

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            lastInterstitialTime = Time.time;
            interstitialEventCounter = 0;
            LoadInterstitial();
        }
        else
        {
            Debug.Log("Interstitial not ready");
        }
    }

    // =========================================================
    // REWARDED
    // =========================================================

    private void LoadRewarded()
    {
        if (!canRequestAds)
            return;
        if (string.IsNullOrWhiteSpace(GetRewardedAdUnitId()))
        {
            Debug.LogWarning("Rewarded ad unit id missing.");
            return;
        }
        if (isRewardedLoading)
            return;

        isRewardedLoading = true;
        isRewardedReady = false;

        Debug.Log("Loading rewarded ad...");

        RewardedAd.Load(
            GetRewardedAdUnitId(),
            new AdRequest(),
            (ad, error) =>
            {
                isRewardedLoading = false;

                if (error != null)
                {
                    Debug.LogWarning("Rewarded load failed: " + error);
                    Invoke(nameof(LoadRewarded), Mathf.Max(2f, config.rewardedRetryDelaySeconds));
                    return;
                }

                rewardedAd = ad;
                isRewardedReady = true;

                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    LoadRewarded();
                };
                rewardedAd.OnAdFullScreenContentFailed += _ => LoadRewarded();

                Debug.Log("Rewarded ready");
            });
    }

    public bool IsRewardedReady() => isRewardedReady;

    public void ShowRewarded(Action onRewardGranted)
    {
        if (!isRewardedReady || rewardedAd == null)
        {
            Debug.Log("Rewarded not ready");
            return;
        }

        rewardedAd.Show(_ =>
        {
            // 🔒 SAFE: wait for graphics device restore
            StartCoroutine(InvokeAfterFrame(onRewardGranted));
        });

        isRewardedReady = false;
    }

    // =========================================================
    // REWARDED INTERSTITIAL
    // =========================================================

    private void LoadRewardedInterstitial()
    {
        if (!canRequestAds)
            return;
        if (string.IsNullOrWhiteSpace(GetRewardedInterstitialAdUnitId()))
        {
            Debug.LogWarning("Rewarded interstitial ad unit id missing.");
            return;
        }
        if (isRewardedInterstitialLoading)
            return;

        isRewardedInterstitialLoading = true;
        isRewardedInterstitialReady = false;

        RewardedInterstitialAd.Load(
            GetRewardedInterstitialAdUnitId(),
            new AdRequest(),
            (ad, error) =>
            {
                isRewardedInterstitialLoading = false;

                if (error != null)
                {
                    Debug.LogWarning("Rewarded interstitial load failed: " + error);
                    Invoke(nameof(LoadRewardedInterstitial), Mathf.Max(2f, config.rewardedInterstitialRetryDelaySeconds));
                    return;
                }

                rewardedInterstitialAd = ad;
                isRewardedInterstitialReady = true;
                rewardedInterstitialAd.OnAdFullScreenContentClosed += () => LoadRewardedInterstitial();
                rewardedInterstitialAd.OnAdFullScreenContentFailed += _ => LoadRewardedInterstitial();
                Debug.Log("Rewarded interstitial ready");
            });
    }

    public bool IsRewardedInterstitialReady() => isRewardedInterstitialReady;

    public void ShowRewardedInterstitial(Action onRewardGranted)
    {
        if (!isRewardedInterstitialReady || rewardedInterstitialAd == null)
        {
            Debug.Log("Rewarded interstitial not ready");
            return;
        }

        rewardedInterstitialAd.Show(_ =>
        {
            StartCoroutine(InvokeAfterFrame(onRewardGranted));
        });

        isRewardedInterstitialReady = false;
    }

    // =========================================================
    // NATIVE ADVANCED (OVERLAY)
    // =========================================================

    private void LoadNativeAdvanced()
    {
        if (!canRequestAds)
            return;
        if (string.IsNullOrWhiteSpace(GetNativeAdvancedAdUnitId()))
        {
            Debug.LogWarning("Native ad unit id missing.");
            return;
        }
        if (isNativeLoading)
            return;

        isNativeLoading = true;
        isNativeReady = false;

        NativeOverlayAd.Load(
            GetNativeAdvancedAdUnitId(),
            new AdRequest(),
            new NativeAdOptions(),
            (ad, error) =>
            {
                isNativeLoading = false;

                if (error != null)
                {
                    Debug.LogWarning("Native advanced load failed: " + error);
                    Invoke(nameof(LoadNativeAdvanced), Mathf.Max(2f, config.nativeRetryDelaySeconds));
                    return;
                }

                nativeOverlayAd = ad;
                isNativeReady = true;
                Debug.Log("Native advanced ready");
            });
    }

    public bool IsNativeAdvancedReady() => isNativeReady;

    public void ShowNativeAdvanced()
    {
        if (!isNativeReady || nativeOverlayAd == null)
        {
            LoadNativeAdvanced();
            return;
        }

        nativeOverlayAd.Show();
    }

    public void HideNativeAdvanced()
    {
        nativeOverlayAd?.Hide();
    }

    // =========================================================
    // APP OPEN
    // =========================================================

    private void LoadAppOpen()
    {
        if (!canRequestAds)
            return;
        if (string.IsNullOrWhiteSpace(GetAppOpenAdUnitId()))
        {
            Debug.LogWarning("App open ad unit id missing.");
            return;
        }
        if (isAppOpenLoading)
            return;

        isAppOpenLoading = true;
        isAppOpenReady = false;

        AppOpenAd.Load(
            GetAppOpenAdUnitId(),
            new AdRequest(),
            (ad, error) =>
            {
                isAppOpenLoading = false;
                if (error != null)
                {
                    Debug.LogWarning("App open load failed: " + error);
                    Invoke(nameof(LoadAppOpen), Mathf.Max(2f, config.appOpenRetryDelaySeconds));
                    return;
                }

                appOpenAd = ad;
                isAppOpenReady = true;
                appOpenAd.OnAdFullScreenContentClosed += () => LoadAppOpen();
                appOpenAd.OnAdFullScreenContentFailed += _ => LoadAppOpen();
                Debug.Log("App open ready");
            });
    }

    public bool IsAppOpenReady() => isAppOpenReady;

    public void TryShowAppOpen()
    {
        if (!isAppOpenReady || appOpenAd == null)
            return;

        if (Time.unscaledTime - lastAppOpenShownTime < Mathf.Max(2f, config.appOpenMinIntervalSeconds))
            return;

        appOpenAd.Show();
        lastAppOpenShownTime = Time.unscaledTime;
        isAppOpenReady = false;
    }

    private string GetBannerAdUnitId()
    {
        return config.useTestAds ? config.testBannerId : config.bannerId;
    }

    private string GetInterstitialAdUnitId()
    {
        return config.useTestAds ? config.testInterstitialId : config.interstitialId;
    }

    private string GetRewardedAdUnitId()
    {
        return config.useTestAds ? config.testRewardedId : config.rewardedId;
    }

    private string GetRewardedInterstitialAdUnitId()
    {
        return config.useTestAds ? config.testRewardedInterstitialId : config.rewardedInterstitialId;
    }

    private string GetNativeAdvancedAdUnitId()
    {
        return config.useTestAds ? config.testNativeAdvancedId : config.nativeAdvancedId;
    }

    private string GetAppOpenAdUnitId()
    {
        return config.useTestAds ? config.testAppOpenId : config.appOpenId;
    }

    private IEnumerator InvokeAfterFrame(Action action)
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        action?.Invoke();
    }

}
