using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootSceneController : MonoBehaviour
{
    private enum BootPhase
    {
        Intro,
        Earth,
        Menu,
        Loading
    }

    private const int NewGameIndex = 0;
    private const int ContinueIndex = 1;
    private const int QuitIndex = 2;
    private const int MenuCount = 3;

    [Header("Existing Name Input Flow")]
    [SerializeField] private TitleManager titleManager;
    [SerializeField] private ForestNameEntryController titleNameEntryController;
    [SerializeField] private bool useTitleManagerNameInput = true;
    [SerializeField] private string townSceneName = GameManager.TownSceneName;
    [SerializeField] private string prologueSceneName = "PrologueScene";

    [Header("Audio")]
    [SerializeField] private AudioSource bootBgmSource;
    [SerializeField] private bool playBgmOnStart = true;

    [Header("Intro")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private CanvasGroup introCreditGroup;
    [SerializeField] private TextMeshProUGUI creditTopText;
    [SerializeField] private TextMeshProUGUI creditBottomText;
    [SerializeField] private Image creditLine;
    [SerializeField] private string firstIntroText = "Created at\nPai Chai University";
    [SerializeField] private string secondIntroText = "Directed by\nDAVID";
    [SerializeField] private string thirdIntroText = "Inspired by\nMOTHER";
    [SerializeField] private float textFadeDuration = 1.0f;
    [SerializeField] private float textHoldDuration = 1.0f;
    [SerializeField] private bool playIntroOnStart = true;
    [SerializeField] private bool allowIntroSkip = true;

    [SerializeField] private GameObject titleGroup;
    [SerializeField] private RectTransform earthImage;
    [SerializeField] private Image earthImageGraphic;
    [SerializeField] private TextMeshProUGUI ursText;
    [SerializeField] private TextMeshProUGUI studentCreditText;
    [SerializeField] private float earthFadeDuration = 1.0f;
    [SerializeField] private float titleTextFadeDuration = 1.0f;
    [SerializeField] private bool allowEarthSkip = true;

    [Header("Earth Animation")]
    [SerializeField] private Vector2 earthStartAnchoredPosition = Vector2.zero;
    [SerializeField] private Vector2 earthTargetAnchoredPosition = new Vector2(-320f, 180f);
    [SerializeField] private Vector3 earthStartScale = Vector3.one;
    [SerializeField] private Vector3 earthTargetScale = new Vector3(0.4f, 0.4f, 0.4f);
    [SerializeField] private float earthMoveDuration = 2.0f;
    [SerializeField] private float earthHoldDuration = 0.5f;

    [Header("Menu")]
    [SerializeField] private GameObject menuGroup;
    [SerializeField] private RectTransform selector;
    [SerializeField] private Vector3 selectorOffset = new Vector3(-80f, 0f, 0f);
    [SerializeField] private TextMeshProUGUI newGameText;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private Color enabledMenuColor = Color.white;
    [SerializeField] private Color disabledMenuColor = new Color(0.55f, 0.55f, 0.55f, 0.45f);

    [Header("Continue Fade")]
    [SerializeField] private float continueFadeOutDuration = 1.0f;
    [SerializeField] private float newGameNameEntryFadeOutDuration = 1.0f;

    [Header("Title Fade In")]
    [SerializeField] private float titleFadeInDuration = 1.5f;

    private int selectedIndex;
    private bool continueEnabled;
    private bool menuInputEnabled;
    private bool skipRequested;
    private bool currentIntroSkipped;
    private bool earthSequenceCompleted;
    private bool startupRoutingStarted;
    private BootPhase phase;

    private void Awake()
    {
        if (titleManager == null)
        {
            titleManager = FindObjectOfType<TitleManager>();
        }

        if (titleManager != null)
        {
            titleManager.SetBootTitleMenuMode(true);
        }
    }

    private void Start()
    {
        if (TryRouteToForestScene())
        {
            return;
        }

        PrepareInitialState();

        if (playBgmOnStart && bootBgmSource != null)
        {
            bootBgmSource.Play();
        }

        if (playIntroOnStart)
        {
            StartCoroutine(BootRoutine());
        }
        else
        {
            ShowTitleAndMenuImmediate();
        }

        StartTitleFadeIn();
    }

    private bool TryRouteToForestScene()
    {
        if (startupRoutingStarted || SaveSystem.HasValidSaveData())
        {
            return false;
        }

        StartupSessionState state = GameManager.Instance != null
            ? GameManager.Instance.startupSessionState
            : StartupSessionState.None;

        if (state == StartupSessionState.NameChosen)
        {
            return false;
        }

        startupRoutingStarted = true;
        SceneManager.LoadScene(GameManager.ForestSceneName);
        return true;
    }

    private void Update()
    {
        if (!menuInputEnabled)
        {
            HandleSkipInput();
            return;
        }

        if (GameInput.UpPressed)
        {
            MoveSelection(-1);
            return;
        }

        if (GameInput.DownPressed)
        {
            MoveSelection(1);
            return;
        }

        if (GameInput.ConfirmPressed)
        {
            ExecuteSelection();
        }
    }

    private void HandleSkipInput()
    {
        if (!GameInput.ConfirmPressed && !GameInput.CancelPressed)
        {
            return;
        }

        if (phase == BootPhase.Intro && allowIntroSkip)
        {
            skipRequested = true;
        }
        else if (phase == BootPhase.Earth && allowEarthSkip)
        {
            skipRequested = true;
        }
    }

    private void PrepareInitialState()
    {
        menuInputEnabled = false;
        earthSequenceCompleted = false;
        RefreshContinueState();

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.transform.SetAsLastSibling();

            Color color = fadeOverlay.color;
            color.r = 0f;
            color.g = 0f;
            color.b = 0f;
            color.a = 1f;
            fadeOverlay.color = color;
        }

        SetTextAlpha(introText, 0f);
        SetCanvasGroupAlpha(introCreditGroup, 0f);
        SetTextAlpha(ursText, 0f);
        SetTextAlpha(studentCreditText, 0f);

        if (titleGroup != null)
        {
            titleGroup.SetActive(false);
        }

        if (menuGroup != null)
        {
            menuGroup.SetActive(false);
        }

        if (earthImage != null)
        {
            earthImage.gameObject.SetActive(false);
            earthImage.anchoredPosition = earthStartAnchoredPosition;
            earthImage.localScale = earthStartScale;
            earthImage.localRotation = Quaternion.identity;
        }

        SetImageAlpha(earthImageGraphic, 0f);
    }

    private IEnumerator BootRoutine()
    {
        phase = BootPhase.Intro;

        yield return FadeTextRoutine(firstIntroText);
        yield return FadeTextRoutine(secondIntroText);
        yield return FadeTextRoutine(thirdIntroText);

        phase = BootPhase.Earth;
        SetImageAlpha(fadeOverlay, 0f);

        if (titleGroup != null)
        {
            titleGroup.SetActive(true);
        }

        if (earthImage != null)
        {
            earthImage.gameObject.SetActive(true);
            earthImage.anchoredPosition = earthStartAnchoredPosition;
            earthImage.localScale = earthStartScale;
            earthImage.localRotation = Quaternion.identity;
        }

        yield return FadeImageAlphaRoutine(earthImageGraphic, 0f, 1f, earthFadeDuration);
        if (earthSequenceCompleted)
        {
            ShowMenu();
            yield break;
        }

        yield return WaitOrSkipRoutine(Mathf.Max(0f, earthHoldDuration));
        if (earthSequenceCompleted)
        {
            ShowMenu();
            yield break;
        }

        yield return AnimateEarthRoutine();
        if (earthSequenceCompleted)
        {
            ShowMenu();
            yield break;
        }

        yield return FadeTextAlphaRoutine(ursText, 0f, 1f, titleTextFadeDuration);
        yield return FadeTextAlphaRoutine(studentCreditText, 0f, 1f, titleTextFadeDuration);
        if (earthSequenceCompleted)
        {
            ShowMenu();
            yield break;
        }

        ShowMenu();
    }

    private void StartTitleFadeIn()
    {
        if (fadeOverlay == null)
        {
            return;
        }

        StartCoroutine(TitleFadeInRoutine());
    }

    private IEnumerator TitleFadeInRoutine()
    {
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.transform.SetAsLastSibling();

        Color color = fadeOverlay.color;
        color.r = 0f;
        color.g = 0f;
        color.b = 0f;
        color.a = 1f;
        fadeOverlay.color = color;

        float safeDuration = Mathf.Max(0.01f, titleFadeInDuration);
        float timer = 0f;

        while (timer < safeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / safeDuration);
            SetImageAlpha(fadeOverlay, 1f - t);
            yield return null;
        }

        SetImageAlpha(fadeOverlay, 0f);
    }

    private IEnumerator FadeTextRoutine(string text)
    {
        currentIntroSkipped = false;

        string topText = text;
        string bottomText = "";
        SplitCreditText(text, out topText, out bottomText);

        if (CanUseCreditGroup())
        {
            creditTopText.text = topText;
            creditBottomText.text = bottomText;
            introCreditGroup.gameObject.SetActive(true);
            if (creditLine != null)
            {
                creditLine.gameObject.SetActive(true);
            }

            yield return FadeCanvasGroupRoutine(introCreditGroup, 0f, 1f, textFadeDuration);
            if (currentIntroSkipped)
            {
                SetCanvasGroupAlpha(introCreditGroup, 0f);
                yield break;
            }

            yield return WaitOrSkipRoutine(Mathf.Max(0f, textHoldDuration));
            if (currentIntroSkipped)
            {
                SetCanvasGroupAlpha(introCreditGroup, 0f);
                yield break;
            }

            yield return FadeCanvasGroupRoutine(introCreditGroup, 1f, 0f, textFadeDuration);
            SetCanvasGroupAlpha(introCreditGroup, 0f);
            yield break;
        }

        if (introText != null)
        {
            introText.text = text.Replace("\\n", "\n");
        }

        yield return FadeTextAlphaRoutine(introText, 0f, 1f, textFadeDuration);
        if (currentIntroSkipped)
        {
            SetTextAlpha(introText, 0f);
            yield break;
        }

        yield return WaitOrSkipRoutine(Mathf.Max(0f, textHoldDuration));
        if (currentIntroSkipped)
        {
            SetTextAlpha(introText, 0f);
            yield break;
        }

        yield return FadeTextAlphaRoutine(introText, 1f, 0f, textFadeDuration);

        SetTextAlpha(introText, 0f);
    }

    private bool CanUseCreditGroup()
    {
        return introCreditGroup != null && creditTopText != null && creditBottomText != null;
    }

    private void SplitCreditText(string text, out string topText, out string bottomText)
    {
        topText = text;
        bottomText = "";

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        string normalizedText = text.Replace("\\n", "\n");
        string[] parts = normalizedText.Split(new[] { '\n' }, 2);

        topText = parts[0];
        if (parts.Length > 1)
        {
            bottomText = parts[1];
        }
    }

    private IEnumerator AnimateEarthRoutine()
    {
        if (earthImage == null)
        {
            yield break;
        }

        if (earthSequenceCompleted)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, earthMoveDuration);
        float timer = 0f;

        while (timer < duration)
        {
            if (ConsumeSkipIfAllowed(BootPhase.Earth, allowEarthSkip))
            {
                CompleteEarthTitleState();
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            earthImage.anchoredPosition = Vector2.Lerp(earthStartAnchoredPosition, earthTargetAnchoredPosition, t);
            earthImage.localScale = Vector3.Lerp(earthStartScale, earthTargetScale, t);

            yield return null;
        }

        earthImage.anchoredPosition = earthTargetAnchoredPosition;
        earthImage.localScale = earthTargetScale;
        earthImage.localRotation = Quaternion.identity;
    }

    private IEnumerator FadeTextAlphaRoutine(TextMeshProUGUI text, float from, float to, float duration)
    {
        if (text == null)
        {
            yield break;
        }

        float safeDuration = Mathf.Max(0.01f, duration);
        float timer = 0f;

        while (timer < safeDuration)
        {
            if (ConsumeSkipIfAllowed(BootPhase.Intro, allowIntroSkip))
            {
                currentIntroSkipped = true;
                SetTextAlpha(text, 0f);
                yield break;
            }

            if (ConsumeSkipIfAllowed(BootPhase.Earth, allowEarthSkip))
            {
                CompleteEarthTitleState();
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / safeDuration);
            SetTextAlpha(text, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetTextAlpha(text, to);
    }
    private IEnumerator FadeImageAlphaRoutine(Image image, float from, float to, float duration)
    {
        if (image == null)
        {
            yield break;
        }

        float safeDuration = Mathf.Max(0.01f, duration);
        float timer = 0f;

        while (timer < safeDuration)
        {
            if (ConsumeSkipIfAllowed(BootPhase.Earth, allowEarthSkip))
            {
                CompleteEarthTitleState();
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / safeDuration);
            SetImageAlpha(image, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetImageAlpha(image, to);
    }

    private IEnumerator FadeCanvasGroupRoutine(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float safeDuration = Mathf.Max(0.01f, duration);
        float timer = 0f;

        while (timer < safeDuration)
        {
            if (ConsumeSkipIfAllowed(BootPhase.Intro, allowIntroSkip))
            {
                currentIntroSkipped = true;
                SetCanvasGroupAlpha(group, 0f);
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / safeDuration);
            SetCanvasGroupAlpha(group, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetCanvasGroupAlpha(group, to);
    }

    private IEnumerator WaitOrSkipRoutine(float seconds)
    {
        float timer = 0f;

        while (timer < seconds)
        {
            if (ConsumeSkipIfAllowed(BootPhase.Intro, allowIntroSkip))
            {
                currentIntroSkipped = true;
                yield break;
            }

            if (ConsumeSkipIfAllowed(BootPhase.Earth, allowEarthSkip))
            {
                CompleteEarthTitleState();
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private bool ConsumeSkipIfAllowed(BootPhase targetPhase, bool allowed)
    {
        if (!skipRequested || !allowed || phase != targetPhase)
        {
            return false;
        }

        skipRequested = false;
        return true;
    }

    private void CompleteEarthTitleState()
    {
        if (titleGroup != null)
        {
            titleGroup.SetActive(true);
        }

        earthSequenceCompleted = true;

        if (earthImage != null)
        {
            earthImage.gameObject.SetActive(true);
            earthImage.anchoredPosition = earthTargetAnchoredPosition;
            earthImage.localScale = earthTargetScale;
            earthImage.localRotation = Quaternion.identity;
        }

        SetImageAlpha(earthImageGraphic, 1f);
        SetTextAlpha(ursText, 1f);
        SetTextAlpha(studentCreditText, 1f);
    }

    private void ShowTitleAndMenuImmediate()
    {
        SetImageAlpha(fadeOverlay, 0f);
        CompleteEarthTitleState();
        ShowMenu();
    }

    private void ShowMenu()
    {
        RefreshContinueState();

        if (menuGroup != null)
        {
            menuGroup.SetActive(true);
        }

        selectedIndex = NewGameIndex;
        if (!IsSelectable(selectedIndex))
        {
            MoveSelection(1);
        }

        UpdateMenuVisuals();
        menuInputEnabled = true;
        phase = BootPhase.Menu;
    }

    public void RefreshContinueState()
    {
        continueEnabled = SaveSystem.HasValidSaveData();
        UpdateMenuVisuals();
    }

    private void MoveSelection(int direction)
    {
        for (int i = 0; i < MenuCount; i++)
        {
            selectedIndex = (selectedIndex + direction + MenuCount) % MenuCount;
            if (IsSelectable(selectedIndex))
            {
                break;
            }
        }

        UpdateMenuVisuals();
    }

    private bool IsSelectable(int index)
    {
        return index != ContinueIndex || continueEnabled;
    }

    private void ExecuteSelection()
    {
        if (!IsSelectable(selectedIndex))
        {
            return;
        }

        switch (selectedIndex)
        {
            case NewGameIndex:
                StartNewGameFlow();
                break;
            case ContinueIndex:
                ContinueGame();
                break;
            case QuitIndex:
                QuitGame();
                break;
        }
    }

    private void StartNewGameFlow()
    {
        menuInputEnabled = false;

        if (TryStartPendingNameNewGame())
        {
            return;
        }

        if (SaveSystem.HasValidSaveData() && titleNameEntryController != null)
        {
            StartCoroutine(StartNameEntryNewGameRoutine());
            return;
        }

        if (menuGroup != null)
        {
            menuGroup.SetActive(false);
        }

        if (titleGroup != null)
        {
            titleGroup.SetActive(false);
        }

        SetTextAlpha(ursText, 0f);
        SetImageAlpha(earthImageGraphic, 0f);
        SetTextAlpha(introText, 0f);
        SetCanvasGroupAlpha(introCreditGroup, 0f);
        SetImageAlpha(fadeOverlay, 0f);
        SetTextAlpha(studentCreditText, 0f);
        if (useTitleManagerNameInput && titleManager != null)
        {
            titleManager.BeginNewGameNameInput();
            return;
        }

        Debug.LogWarning("BootSceneController: TitleManager name input is not connected.");
    }

    private IEnumerator StartNameEntryNewGameRoutine()
    {
        phase = BootPhase.Loading;

        yield return FadeOutTitleForNameEntryRoutine();

        if (menuGroup != null)
        {
            menuGroup.SetActive(false);
        }

        if (titleGroup != null)
        {
            titleGroup.SetActive(false);
        }

        titleNameEntryController.ShowNameEntryForNewGame(prologueSceneName);
    }

    private IEnumerator FadeOutTitleForNameEntryRoutine()
    {
        float duration = Mathf.Max(0.01f, newGameNameEntryFadeOutDuration);
        float timer = 0f;
        float startBgmVolume = bootBgmSource != null ? bootBgmSource.volume : 0f;

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.transform.SetAsLastSibling();
            SetImageAlpha(fadeOverlay, 0f);
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            SetImageAlpha(fadeOverlay, t);
            if (bootBgmSource != null)
            {
                bootBgmSource.volume = Mathf.Lerp(startBgmVolume, 0f, t);
            }

            yield return null;
        }

        SetImageAlpha(fadeOverlay, 1f);
        if (bootBgmSource != null)
        {
            bootBgmSource.volume = 0f;
        }
    }

    private bool TryStartPendingNameNewGame()
    {
        if (SaveSystem.HasValidSaveData() || GameManager.Instance == null)
        {
            return false;
        }

        if (GameManager.Instance.startupSessionState != StartupSessionState.NameChosen ||
            string.IsNullOrWhiteSpace(GameManager.Instance.pendingPlayerName))
        {
            return false;
        }

        string newPlayerName = GameManager.Instance.pendingPlayerName;
        GameManager.Instance.StartNewGame(newPlayerName);
        GameManager.Instance.ClearStartupSession();
        SceneManager.LoadScene(prologueSceneName);
        return true;
    }

    private void ContinueGame()
    {
        if (!SaveSystem.HasValidSaveData())
        {
            RefreshContinueState();
            return;
        }

        StartCoroutine(ContinueGameRoutine());
    }

    private IEnumerator ContinueGameRoutine()
    {
        menuInputEnabled = false;
        phase = BootPhase.Loading;

        yield return FadeOverlayRoutine(0f, 1f, continueFadeOutDuration);

        SaveSystem.LoadGame();

        string sceneToLoad = GameManager.NormalizeSceneName(townSceneName);
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.currentSceneName))
        {
            sceneToLoad = GameManager.NormalizeSceneName(GameManager.Instance.currentSceneName);
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeOverlayRoutine(float from, float to, float duration)
    {
        if (fadeOverlay == null)
        {
            yield break;
        }

        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.transform.SetAsLastSibling();

        float safeDuration = Mathf.Max(0.01f, duration);
        float timer = 0f;

        SetImageAlpha(fadeOverlay, from);

        while (timer < safeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / safeDuration);
            SetImageAlpha(fadeOverlay, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetImageAlpha(fadeOverlay, to);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("BootSceneController: Quit requested.");
#else
        Application.Quit();
#endif
    }

    private void UpdateMenuVisuals()
    {
        SetMenuTextColor(newGameText, enabledMenuColor);
        SetMenuTextColor(continueText, continueEnabled ? enabledMenuColor : disabledMenuColor);
        SetMenuTextColor(quitText, enabledMenuColor);
        MoveSelectorToSelected();
    }

    private void MoveSelectorToSelected()
    {
        if (selector == null)
        {
            return;
        }

        RectTransform target = GetSelectedTextRect();
        if (target == null)
        {
            return;
        }

        selector.position = target.position + selectorOffset;
    }

    private RectTransform GetSelectedTextRect()
    {
        switch (selectedIndex)
        {
            case NewGameIndex:
                return newGameText != null ? newGameText.rectTransform : null;
            case ContinueIndex:
                return continueText != null ? continueText.rectTransform : null;
            case QuitIndex:
                return quitText != null ? quitText.rectTransform : null;
            default:
                return null;
        }
    }

    private void SetMenuTextColor(TextMeshProUGUI text, Color color)
    {
        if (text != null)
        {
            text.color = color;
        }
    }

    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null)
        {
            return;
        }

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void SetCanvasGroupAlpha(CanvasGroup group, float alpha)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = alpha;
    }
}
