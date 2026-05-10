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
    public string rewardedId;
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

    [Header("Rewarded Settings")]
    public bool preloadRewarded = true;

    [Header("Debug")]
    public bool useTestAds = true;
}
