using UnityEngine;
using GoogleMobileAds.Ump.Api;

public static class GdprConsentManager
{
    /// <summary>
    /// True if ads can be requested after consent flow
    /// </summary>
    public static bool CanRequestAds { get; private set; }

    private static bool isRequestInProgress;

    /// <summary>
    /// Request GDPR consent and invoke callback once resolved
    /// </summary>
    public static void RequestConsent(System.Action onComplete)
    {
        if (isRequestInProgress)
            return;

        isRequestInProgress = true;

        Debug.Log("GDPR: Requesting consent…");

        ConsentRequestParameters requestParameters = new ConsentRequestParameters
        {
#if UNITY_EDITOR
            // 🔧 DEBUG ONLY — force EEA in Editor
            ConsentDebugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = { "TEST-DEVICE-ID" } // optional
            }
#endif
        };

        ConsentInformation.Update(requestParameters, updateError =>
        {
            if (updateError != null)
            {
                Debug.LogError("GDPR: Consent update failed: " + updateError);
                CanRequestAds = true; // fail-open (recommended)
                Finish(onComplete);
                return;
            }

            Debug.Log("GDPR: Consent status = " +
                      ConsentInformation.ConsentStatus);

            // Load & show form if required
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                {
                    Debug.LogError("GDPR: Consent form error: " + formError);
                }

                CanRequestAds = ConsentInformation.CanRequestAds();
                Debug.Log("GDPR: CanRequestAds = " + CanRequestAds);

                Finish(onComplete);
            });
        });
    }

    /// <summary>
    /// Reset consent (FOR DEBUG / QA ONLY)
    /// </summary>
    public static void ResetConsent()
    {
#if UNITY_EDITOR
        ConsentInformation.Reset();
        Debug.Log("GDPR: Consent reset (Editor only)");
#endif
    }

    private static void Finish(System.Action onComplete)
    {
        isRequestInProgress = false;
        onComplete?.Invoke();
    }
}

