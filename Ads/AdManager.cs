using System;
using UnityEngine;
using GoogleMobileAds.Api;

/// <summary>
/// AdManager Singleton for Unity using Google Mobile Ads (AdMob).
/// Updated for recent Google Mobile Ads Unity SDK (no MobileAdsEventExecutor).
///
/// Features:
/// - Initialization
/// - Banner (load/show/hide/destroy)
/// - Interstitial (load/show, auto-reload on close)
/// - Rewarded (load/show, reward callback)
/// - RewardedInterstitial (optional)
/// - Public events for game hooks
///
/// Usage:
/// - Place this script on a GameObject in your initial scene or let it be present in a bootstrap scene.
/// - Use AdManager.Instance.ShowInterstitial(); AdManager.Instance.ShowRewarded(); AdManager.Instance.ShowBanner();
/// - Subscribe to OnUserEarnedReward to grant rewards.
///
/// Notes:
/// - Replace test Ad Unit IDs with your production IDs before releasing.
/// - Ensure you have the Google Mobile Ads Unity plugin imported and dependencies resolved.
/// </summary>

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("Ad Unit IDs (replace for production)")]
    public string appId; // Test App ID
    public string bannerAdUnitId; // Test Banner
    public string interstitialAdUnitId; // Test Interstitial
    public string rewardedAdUnitId; // Test Rewarded
    public string rewardedInterstitialAdUnitId; // Optional

    // Internal ad references
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private RewardedInterstitialAd rewardedInterstitialAd;

    // Events
    public event Action OnInterstitialLoaded;
    public event Action OnInterstitialFailedToLoad;
    public event Action OnInterstitialClosed;

    public event Action OnRewardedLoaded;
    public event Action OnRewardedFailedToLoad;
    public event Action<Reward> OnUserEarnedReward;

    public bool AutoReloadOnFail = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Initialize the Mobile Ads SDK. If you manage consent via UMP, do it BEFORE calling Initialize.
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Mobile Ads Initialized.");

            // Newer SDK versions run callbacks on Unity main thread, so it's safe to call Unity APIs directly here.
            // Preload the ads we want ready at startup.
            LoadBanner();
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
        });
    }

    #region BANNER

    public void LoadBanner(AdPosition position = AdPosition.Top)
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, position);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);

        // Some SDK versions expose banner load events; if your SDK doesn't have them, remove or replace these with the appropriate event names from your version of the Google Mobile Ads SDK.
        // Example (if available):
        // bannerView.OnAdLoaded += () => { Debug.Log("Banner loaded."); };
        // bannerView.OnAdFailedToLoad += (LoadAdError err) => { Debug.LogError("Banner failed to load: " + err); };

    }

    public void ShowBanner()
    {
        if (bannerView == null)
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

    public void DestroyBanner()
    {
        bannerView?.Destroy();
        bannerView = null;
    }

    #endregion

    #region INTERSTITIAL

    public void LoadInterstitial()
    {
        AdRequest request = new AdRequest();

        InterstitialAd.Load(interstitialAdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Interstitial failed to load: " + error);
                OnInterstitialFailedToLoad?.Invoke();
                if (AutoReloadOnFail)
                {
                    Invoke(nameof(LoadInterstitial), 5f);
                }
                return;
            }

            Debug.Log("Interstitial loaded.");
            interstitialAd = ad;
            RegisterInterstitialEvents(interstitialAd);
            OnInterstitialLoaded?.Invoke();
        });
    }

    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial closed.");
            OnInterstitialClosed?.Invoke();
            // reload after close
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += (AdError err) =>
        {
            Debug.LogError("Interstitial show failed: " + err);
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial opened.");
        };
    }

    public void ShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial not ready, loading now.");
            LoadInterstitial();
        }
    }

    #endregion

    #region REWARDED

    public void LoadRewarded()
    {
        AdRequest request = new AdRequest();

        RewardedAd.Load(rewardedAdUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded failed to load: " + error);
                OnRewardedFailedToLoad?.Invoke();
                if (AutoReloadOnFail)
                {
                    Invoke(nameof(LoadRewarded), 5f);
                }
                return;
            }

            Debug.Log("Rewarded loaded.");
            rewardedAd = ad;
            RegisterRewardedEvents(rewardedAd);
            OnRewardedLoaded?.Invoke();
        });
    }

    private void RegisterRewardedEvents(RewardedAd ad)
    {
        // Some SDK versions expose an OnUserEarnedReward event on RewardedAd, others do not.
        // To be compatible across SDKs we avoid subscribing to a non-existent event here.
        // Keep full-screen content callbacks for lifecycle handling.

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad closed. Reloading...");
            LoadRewarded();
        };

        ad.OnAdFullScreenContentFailed += (AdError err) =>
        {
            Debug.LogError("Rewarded failed to show: " + err);
        };
    }

    public void ShowRewarded()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            // Call the SDK's Show overload that provides the earned reward via callback.
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Type} amount: {reward.Amount}");
                OnUserEarnedReward?.Invoke(reward);
            });
        }
        else
        {
            Debug.Log("Rewarded ad not ready. Loading...");
            LoadRewarded();
        }
    }

    #endregion

    #region REWARDED INTERSTITIAL (optional)

    public void LoadRewardedInterstitial()
    {
        if (string.IsNullOrEmpty(rewardedInterstitialAdUnitId))
        {
            return; // not configured
        }

        AdRequest request = new AdRequest();
        RewardedInterstitialAd.Load(rewardedInterstitialAdUnitId, request, (RewardedInterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("RewardedInterstitial failed to load: " + error);
                if (AutoReloadOnFail)
                {
                    Invoke(nameof(LoadRewardedInterstitial), 10f);
                }
                return;
            }

            Debug.Log("RewardedInterstitial loaded.");
            rewardedInterstitialAd = ad;

            rewardedInterstitialAd.OnAdFullScreenContentClosed += () => { Debug.Log("RewardedInterstitial closed"); LoadRewardedInterstitial(); };
        });
    }

    public void ShowRewardedInterstitial()
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((reward) =>
            {
                Debug.Log($"RewardedInterstitial reward: {reward.Type} {reward.Amount}");
                OnUserEarnedReward?.Invoke(reward);
            });
        }
        else
        {
            Debug.Log("RewardedInterstitial not ready. Loading...");
            LoadRewardedInterstitial();
        }
    }

    #endregion

    private void OnDestroy()
    {
        DestroyBanner();
        interstitialAd = null;
        rewardedAd = null;
        rewardedInterstitialAd = null;
    }
}
