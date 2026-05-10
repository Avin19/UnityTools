using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BladeootstrapLoadingUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [Header("Loading Setting")]
    [SerializeField] private float totalLoadingDuration = 5f;

    private float currentProgress;

    private string[] loadingMessages =
    {
        "Initializing Audio Systems",
        "Preparing Game Services",
        "Connecting Gameplay Modules",
        "Loading Player Profile",
        "Synchronizing Save Data",
        "Preparing User Interface",
        "Configuring Input Controls",
        "Optimizing Performance",
        "Initializing Physics Engine",
        "Preparing AI Systems",
        "Loading Game Assets",
        "Building Environment",
        "Preparing Visual Effects",
        "Loading Character Data",
        "Initializing Enemy Behaviors",
        "Preparing Mission Data",
        "Loading World State",
        "Generating Gameplay Systems",
        "Initializing Network Components",
        "Preparing Adventure",
        "Sharpening Blades",
        "Scanning Environment",
        "Tracking Enemy Patrols",
        "Summoning Ancient Powers",
        "Charging Gameplay Systems"
    };

    void Start()
    {
        loadingSlider.value = 0;
        StartCoroutine(InitializeGame());
    }

    IEnumerator InitializeGame()
    {
        float timer = 0f;
        StartCoroutine(ChangeLoadingMessages());

        while (timer < totalLoadingDuration)
        {
            timer += Time.deltaTime;
            currentProgress = Mathf.Clamp01(timer / totalLoadingDuration);
            UpdateUI();
            yield return null;

        }
        currentProgress = 1f;

        UpdateUI();

        loadingText.text = "Loading Complete";

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(1);
    }
    IEnumerator ChangeLoadingMessages()
    {
        while (currentProgress < 1f)
        {
            string message = loadingMessages[Random.Range(0, loadingMessages.Length)];

            yield return StartCoroutine(AnimateDots(message));

            yield return new WaitForSeconds(0.2f);
        }
    }
    IEnumerator AnimateDots(string baseMessage)
    {
        float duration = 1.2f;

        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer % 0.9f;

            if (t < 0.3f)
                loadingText.text = baseMessage + ".";
            else if (t < 0.6f)
                loadingText.text = baseMessage + "..";
            else
                loadingText.text = baseMessage + "...";

            yield return null;
        }
    }
    void UpdateUI()
    {
        loadingSlider.value =
            Mathf.Lerp(loadingSlider.value, currentProgress, Time.deltaTime * 8f);

        int percent =
            Mathf.RoundToInt(currentProgress * 100);

        percentageText.text = percent + "%";
    }
}
