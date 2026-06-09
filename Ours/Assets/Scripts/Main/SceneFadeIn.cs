using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFadeIn : MonoBehaviour
{
    private const string TownSceneName = GameManager.TownSceneName;

    [SerializeField] private float fadeDuration = 1.0f;

    private Image fadeImage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoaded()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TownSceneName)
        {
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.fadeInOnTownSceneLoad)
        {
            return;
        }

        GameManager.Instance.fadeInOnTownSceneLoad = false;

        GameObject fadeObject = new GameObject("SceneFadeIn");
        SceneFadeIn fadeIn = fadeObject.AddComponent<SceneFadeIn>();
        fadeIn.BuildFadeCanvas();
        fadeIn.StartCoroutine(fadeIn.FadeInRoutine());
    }

    private void BuildFadeCanvas()
    {
        GameObject canvasObject = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new GameObject("FadeOverlay", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeImage = imageObject.GetComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;
    }

    private IEnumerator FadeInRoutine()
    {
        if (fadeImage == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            color.a = 1f - t;
            fadeImage.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}
