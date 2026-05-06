using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade UI")]
    [SerializeField] private GameObject fadeRoot;
    [SerializeField] private CanvasGroup fadeGroup;

    [Header("Fade Timing")]
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.35f;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.35f;
    [SerializeField] private bool fadeInOnFirstScene = true;

    [Header("Next Scene Behaviour")]
    [SerializeField] private bool wrapToFirstScene = false;
    [SerializeField] private int firstSceneBuildIndex = 0;

    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeRoot == null && fadeGroup != null)
            fadeRoot = fadeGroup.gameObject;

        if (fadeGroup == null && fadeRoot != null)
            fadeGroup = fadeRoot.GetComponent<CanvasGroup>();

        HideFadeInstant();
    }

    private IEnumerator Start()
    {
        if (!fadeInOnFirstScene)
            yield break;

        ShowFadeInstant(1f);
        yield return FadeTo(0f, fadeInDuration, deactivateWhenDone: true);
    }

    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            if (!wrapToFirstScene)
            {
                Debug.LogWarning(
                    $"No next scene after build index {currentIndex}. " +
                    "Add another scene to Build Settings or enable Wrap To First Scene."
                );
                return;
            }

            nextIndex = firstSceneBuildIndex;
        }

        LoadScene(nextIndex);
    }

    public void LoadScene(int buildIndex)
    {
        if (isTransitioning)
            return;

        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError(
                $"Scene build index {buildIndex} is invalid. " +
                "Check your scene list in Build Settings / Build Profiles."
            );
            return;
        }

        StartCoroutine(LoadSceneRoutine(buildIndex));
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator LoadSceneRoutine(int buildIndex)
    {
        isTransitioning = true;

        ShowFadeInstant(fadeGroup != null ? fadeGroup.alpha : 0f);
        yield return FadeTo(1f, fadeOutDuration, deactivateWhenDone: false);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(buildIndex);

        if (loadOperation == null)
        {
            Debug.LogError($"Failed to load scene build index {buildIndex}.");
            isTransitioning = false;
            yield break;
        }

        while (!loadOperation.isDone)
            yield return null;

        // Let the newly loaded scene initialize for one frame before fading back in.
        yield return null;

        yield return FadeTo(0f, fadeInDuration, deactivateWhenDone: true);

        isTransitioning = false;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration, bool deactivateWhenDone)
    {
        if (fadeGroup == null)
        {
            Debug.LogError("SceneTransitionManager is missing its CanvasGroup reference.");
            yield break;
        }

        if (fadeRoot != null)
            fadeRoot.SetActive(true);

        fadeGroup.blocksRaycasts = true;
        fadeGroup.interactable = false;

        float startAlpha = fadeGroup.alpha;

        if (duration <= 0f)
        {
            SetFadeAlpha(targetAlpha);
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));

                yield return null;
            }
        }

        SetFadeAlpha(targetAlpha);

        if (deactivateWhenDone && fadeRoot != null)
            fadeRoot.SetActive(false);

        fadeGroup.blocksRaycasts = !deactivateWhenDone;
    }

    private void ShowFadeInstant(float alpha)
    {
        if (fadeRoot != null)
            fadeRoot.SetActive(true);

        SetFadeAlpha(alpha);

        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;
            fadeGroup.interactable = false;
        }
    }

    private void HideFadeInstant()
    {
        SetFadeAlpha(0f);

        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }

        if (fadeRoot != null)
            fadeRoot.SetActive(false);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeGroup != null)
            fadeGroup.alpha = alpha;
    }
}