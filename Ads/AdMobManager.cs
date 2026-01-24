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

        InitializeAdMob();
    }
    private void InitializeAdMob()
    {
        MobileAds.Initialize(_ =>
        {
            Debug.Log("AdMob initialized");

            if (config.loadBannerOnStart)
                LoadBanner();

            LoadInterstitial();

            if (config.preloadRewarded)
                LoadRewarded();
        });
    }


    private void OnDestroy()
    {
        bannerView?.Destroy();
        interstitialAd?.Destroy();
    }

    #endregion

    // =========================================================
    // BANNER
    // =========================================================

    private void LoadBanner()
    {
        if (isBannerLoading || bannerView != null)
            return;

        isBannerLoading = true;
        isBannerLoaded = false;

        // 🔥 Adaptive banner (recommended)
        AdSize adaptiveSize =
            AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                AdSize.FullWidth);

        bannerView = new BannerView(
            config.bannerId,
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
        interstitialAd?.Destroy();
        interstitialAd = null;

        InterstitialAd.Load(
            config.interstitialId,
            new AdRequest(),
            (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning("Interstitial load failed: " + error);
                    return;
                }

                interstitialAd = ad;
                Debug.Log("Interstitial loaded");
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
        if (isRewardedLoading)
            return;

        isRewardedLoading = true;
        isRewardedReady = false;

        Debug.Log("Loading rewarded ad...");

        RewardedAd.Load(
            config.rewardedId,
            new AdRequest(),
            (ad, error) =>
            {
                isRewardedLoading = false;

                if (error != null)
                {
                    Debug.LogWarning("Rewarded load failed: " + error);
                    Invoke(nameof(LoadRewarded), 5f);
                    return;
                }

                rewardedAd = ad;
                isRewardedReady = true;

                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    LoadRewarded();
                };

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

    private IEnumerator InvokeAfterFrame(Action action)
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        action?.Invoke();
    }
}
