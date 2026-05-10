using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fake / staged loading bar with rotating status lines, then loads the next scene by build index.
/// Uses unscaled time so progress still runs if <see cref="Time.timeScale"/> is zero.
/// </summary>
public class BootstrapLoadingUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI percentageText;

    [Header("Loading")]
    [SerializeField] [Min(0.1f)] private float totalLoadingDuration = 5f;
    [SerializeField] private int nextSceneBuildIndex = 1;
    [Tooltip("If true, uses unscaled delta time and real-time waits (recommended for bootstrap).")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] [Min(0f)] private float holdOnCompleteSeconds = 0.5f;
    [SerializeField] private float sliderSmoothing = 8f;

    private float _currentProgress;
    private Coroutine _messagesRoutine;
    private int _lastMessageIndex = -1;

    private static readonly string[] LoadingMessages =
    {
        "Initializing audio systems",
        "Preparing game services",
        "Connecting gameplay modules",
        "Loading player profile",
        "Synchronizing save data",
        "Preparing user interface",
        "Configuring input controls",
        "Optimizing performance",
        "Initializing physics engine",
        "Preparing AI systems",
        "Loading game assets",
        "Building environment",
        "Preparing visual effects",
        "Loading character data",
        "Initializing enemy behaviors",
        "Preparing mission data",
        "Loading world state",
        "Generating gameplay systems",
        "Initializing network components",
        "Preparing adventure",
        "Sharpening blades",
        "Scanning environment",
        "Tracking enemy patrols",
        "Summoning ancient powers",
        "Charging gameplay systems"
    };

    private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private void Start()
    {
        if (loadingSlider != null)
            loadingSlider.value = 0f;

        _currentProgress = 0f;
        _messagesRoutine = StartCoroutine(ChangeLoadingMessages());
        StartCoroutine(InitializeGame());
    }

    private void OnDestroy()
    {
        if (_messagesRoutine != null)
        {
            StopCoroutine(_messagesRoutine);
            _messagesRoutine = null;
        }
    }

    private IEnumerator InitializeGame()
    {
        float timer = 0f;

        while (timer < totalLoadingDuration)
        {
            timer += DeltaTime;
            _currentProgress = Mathf.Clamp01(timer / totalLoadingDuration);
            UpdateUi();
            yield return null;
        }

        _currentProgress = 1f;
        UpdateUi();

        if (loadingText != null)
            loadingText.text = "Loading complete";

        if (holdOnCompleteSeconds > 0f)
        {
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(holdOnCompleteSeconds);
            else
                yield return new WaitForSeconds(holdOnCompleteSeconds);
        }

        if (!TryLoadNextScene())
            yield break;
    }

    private bool TryLoadNextScene()
    {
        if (nextSceneBuildIndex < 0 || nextSceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError(
                $"BootstrapLoadingUI: nextSceneBuildIndex {nextSceneBuildIndex} is out of range (0..{SceneManager.sceneCountInBuildSettings - 1}). " +
                "Fix Build Settings or the field on this component.");
            return false;
        }

        SceneManager.LoadScene(nextSceneBuildIndex);
        return true;
    }

    private IEnumerator ChangeLoadingMessages()
    {
        while (_currentProgress < 1f && isActiveAndEnabled)
        {
            string message = PickNextMessage();
            yield return AnimateDots(message);
            yield return WaitShortGap();
        }
    }

    private string PickNextMessage()
    {
        if (LoadingMessages.Length == 0)
            return "Loading";

        if (LoadingMessages.Length == 1)
            return LoadingMessages[0];

        int idx = Random.Range(0, LoadingMessages.Length);
        if (idx == _lastMessageIndex)
            idx = (idx + 1) % LoadingMessages.Length;

        _lastMessageIndex = idx;
        return LoadingMessages[idx];
    }

    private IEnumerator WaitShortGap()
    {
        const float gap = 0.2f;
        float t = 0f;
        while (t < gap && _currentProgress < 1f && isActiveAndEnabled)
        {
            t += DeltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateDots(string baseMessage)
    {
        const float duration = 1.2f;
        float timer = 0f;

        while (timer < duration && _currentProgress < 1f && isActiveAndEnabled)
        {
            timer += DeltaTime;
            float phase = timer % 0.9f;

            if (loadingText != null)
            {
                if (phase < 0.3f)
                    loadingText.text = baseMessage + ".";
                else if (phase < 0.6f)
                    loadingText.text = baseMessage + "..";
                else
                    loadingText.text = baseMessage + "...";
            }

            yield return null;
        }
    }

    private void UpdateUi()
    {
        if (loadingSlider != null)
            loadingSlider.value = Mathf.Lerp(loadingSlider.value, _currentProgress, DeltaTime * sliderSmoothing);

        if (percentageText != null)
        {
            int percent = Mathf.RoundToInt(_currentProgress * 100f);
            percentageText.text = percent + "%";
        }
    }
}
