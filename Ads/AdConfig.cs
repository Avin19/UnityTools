using UnityEngine;

[CreateAssetMenu(
    fileName = "AdConfig",
    menuName = "Ads/AdMob Config",
    order = 0)]
public class AdConfig : ScriptableObject
{
    [Header("Ad Unit IDs")]
    public string bannerId;
    public string interstitialId;
    public string rewardedInterstitialId;
    public string rewardedId;
    public string nativeAdvancedId;
    public string appOpenId;

    [Header("Google Test Ad Unit IDs")]
    [Tooltip("Used automatically when Use Test Ads is enabled.")]
    public string testBannerId = "ca-app-pub-3940256099942544/6300978111";
    public string testInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    public string testRewardedInterstitialId = "ca-app-pub-3940256099942544/5354046379";
    public string testRewardedId = "ca-app-pub-3940256099942544/5224354917";
    public string testNativeAdvancedId = "ca-app-pub-3940256099942544/2247696110";
    public string testAppOpenId = "ca-app-pub-3940256099942544/9257395921";
    [Header("GDPR / Consent")]
    [Tooltip("Enable GDPR consent flow (required for EEA users)")]
    public bool enableGdprConsent = true;

    [Tooltip("Ask consent automatically on app start")]
    public bool requestConsentOnStartup = true;


    [Header("Banner Settings")]
    public bool loadBannerOnStart = true;
    public bool autoShowBanner = false;

    [Tooltip("Initial retry delay when banner returns No Fill (seconds)")]
    public float bannerRetryInitialDelay = 30f;

    [Tooltip("Maximum retry delay for banner (seconds)")]
    public float bannerRetryMaxDelay = 300f;

    [Header("Interstitial Rules")]
    public int interstitialEveryNEvents = 2;
    public float interstitialCooldownSeconds = 60f;
    public float interstitialRetryDelaySeconds = 10f;

    [Header("Rewarded Settings")]
    public bool preloadRewarded = true;
    public float rewardedRetryDelaySeconds = 5f;

    [Header("Rewarded Interstitial Settings")]
    public bool preloadRewardedInterstitial = true;
    public float rewardedInterstitialRetryDelaySeconds = 5f;

    [Header("Native Advanced Settings")]
    public bool loadNativeOnStart = false;
    public float nativeRetryDelaySeconds = 10f;

    [Header("App Open Settings")]
    public bool loadAppOpenOnStart = true;
    public float appOpenRetryDelaySeconds = 10f;
    public float appOpenMinIntervalSeconds = 30f;

    [Header("Debug")]
    public bool useTestAds = true;
}
