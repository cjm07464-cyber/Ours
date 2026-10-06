using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Layouts")]
    [SerializeField] private GameObject basicLayout;
    [SerializeField] private GameObject portraitLayout;

    [Header("Basic")]
    [SerializeField] private TMP_Text basicDialogueText;
    [SerializeField] private SlidingTypewriterText basicSlidingText;

    [Header("Portrait")]
    [SerializeField] private GameObject portraitPanel;
    [SerializeField] private TMP_Text portraitDialogueText;
    [SerializeField] private SlidingTypewriterText portraitSlidingText;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image portraitBackground;
    [SerializeField] private Image portraitGradient;

    [Header("Typewriter")]
    [SerializeField] private float characterInterval = 0.035f;
    [SerializeField] private AudioSource typewriterAudioSource;
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private int soundEveryNCharacters = 1;

    private Action onClosed;
    private int openedFrame = -1;
    private int pageStartedFrame = -1;
    private Coroutine typewriterCoroutine;
    private bool isTyping;
    private int currentPageLastCharacterIndex = -1;
    private int soundCharacterCounter;
    private TMP_Text currentDialogueText;
    private SlidingTypewriterText currentSlidingText;
    private string currentFullText = string.Empty;
    private bool continueToNextLine;
    private AudioClip currentTypewriterSoundOverride;
    private bool currentInstantText;
    private AudioClip pendingLineEndSound;
    private float pendingLineEndSoundVolume = 1f;
    private bool pendingWaitForLineEndSound;
    private bool pendingPauseBgmDuringLineEndSound;
    private float pendingBgmResumeFadeDuration = 1f;
    private AudioClip currentLineEndSound;
    private float currentLineEndSoundVolume = 1f;
    private bool currentWaitForLineEndSound;
    private bool currentPauseBgmDuringLineEndSound;
    private float currentBgmResumeFadeDuration = 1f;
    private bool lineEndSoundStarted;
    private bool lineEndSoundBlockingInput;
    private Coroutine lineEndSoundCoroutine;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        ResetSlidingTexts();

        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Time.frameCount == openedFrame)
        {
            return;
        }

        if (GameInput.DialogueAdvancePressed)
        {
            AdvanceOrClose();
        }
    }

    public void Show(string text, Action onClosed = null)
    {
        Show(text, null, onClosed);
    }

    public void Show(string text, AudioClip typewriterSoundOverride, Action onClosed = null)
    {
        SetBasicLayout();
        TMP_Text targetText = basicDialogueText != null ? basicDialogueText : dialogueText;
        Open(text, targetText, typewriterSoundOverride, instantText: false, onClosed);
    }

    public void ShowInstant(string text, Action onClosed = null)
    {
        SetBasicLayout();
        TMP_Text targetText = basicDialogueText != null ? basicDialogueText : dialogueText;
        Open(text, targetText, null, instantText: true, onClosed);
    }

    public void ConfigureLineEndSound(
        AudioClip sound,
        float volumeScale,
        bool waitForSound,
        bool pauseBgm,
        float bgmResumeFadeDuration)
    {
        pendingLineEndSound = sound;
        pendingLineEndSoundVolume = Mathf.Clamp01(volumeScale);
        pendingWaitForLineEndSound = waitForSound;
        pendingPauseBgmDuringLineEndSound = pauseBgm;
        pendingBgmResumeFadeDuration = Mathf.Max(0f, bgmResumeFadeDuration);
    }

    public void SetContinueToNextLine(bool shouldContinue)
    {
        continueToNextLine = shouldContinue;
    }

    public void ShowPortrait(
        string text,
        string speakerName,
        Sprite portrait,
        Color backgroundColor,
        Color gradientColor,
        Action onClosed = null)
    {
        ShowPortrait(text, speakerName, portrait, backgroundColor, gradientColor, true, onClosed);
    }

    public void ShowPortrait(
        string text,
        string speakerName,
        Sprite portrait,
        Color backgroundColor,
        Color gradientColor,
        bool showSpeakerName,
        Action onClosed = null)
    {
        ShowPortrait(text, speakerName, portrait, backgroundColor, gradientColor, showSpeakerName, null, onClosed);
    }

    public void ShowPortrait(
        string text,
        string speakerName,
        Sprite portrait,
        Color backgroundColor,
        Color gradientColor,
        bool showSpeakerName,
        AudioClip typewriterSoundOverride,
        Action onClosed = null)
    {
        SetPortraitLayout();
        SetPortraitDetailsVisible(true);

        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
            speakerNameText.gameObject.SetActive(showSpeakerName);
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.gameObject.SetActive(portrait != null);
        }

        if (portraitBackground != null)
        {
            portraitBackground.color = backgroundColor;
        }

        if (portraitGradient != null)
        {
            portraitGradient.color = gradientColor;
        }

        TMP_Text targetText = portraitDialogueText != null
            ? portraitDialogueText
            : (basicDialogueText != null ? basicDialogueText : dialogueText);
        Open(text, targetText, typewriterSoundOverride, instantText: false, onClosed);
    }

    public void ShowPortraitBoxOnly(string text, Action onClosed = null)
    {
        ShowPortraitBoxOnly(text, null, onClosed);
    }

    public void ShowPortraitBoxOnly(string text, AudioClip typewriterSoundOverride, Action onClosed = null)
    {
        SetPortraitLayout();
        SetPortraitDetailsVisible(false);

        TMP_Text targetText = portraitDialogueText != null
            ? portraitDialogueText
            : (basicDialogueText != null ? basicDialogueText : dialogueText);
        Open(text, targetText, typewriterSoundOverride, instantText: false, onClosed);
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        Action callback = onClosed;
        onClosed = null;

        bool preserveTextForNextLine = continueToNextLine;
        continueToNextLine = false;
        IsOpen = false;
        StopTypewriter();
        StopLineEndSoundWait();

        if (!preserveTextForNextLine)
        {
            ResetVisibleCharacters();
            ResetSlidingTexts();
            ClearPortraitState();

            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
            }
        }

        currentDialogueText = null;
        if (!preserveTextForNextLine)
        {
            currentSlidingText = null;
        }

        currentFullText = string.Empty;
        currentTypewriterSoundOverride = null;
        currentInstantText = false;
        currentLineEndSound = null;
        currentLineEndSoundVolume = 1f;
        currentWaitForLineEndSound = false;
        currentPauseBgmDuringLineEndSound = false;
        currentBgmResumeFadeDuration = 1f;
        lineEndSoundStarted = false;
        lineEndSoundBlockingInput = false;

        callback?.Invoke();
    }

    private void Open(string text, TMP_Text targetText, AudioClip typewriterSoundOverride, bool instantText, Action onClosed)
    {
        this.onClosed = onClosed;
        SlidingTypewriterText targetSlidingText = ResolveSlidingText(targetText);
        if (currentSlidingText != null && currentSlidingText != targetSlidingText)
        {
            currentSlidingText.ResetState(true);
        }

        currentSlidingText = targetSlidingText;
        currentDialogueText = targetText;
        currentFullText = text ?? string.Empty;
        currentTypewriterSoundOverride = typewriterSoundOverride;
        currentInstantText = instantText;
        currentLineEndSound = pendingLineEndSound;
        currentLineEndSoundVolume = pendingLineEndSoundVolume;
        currentWaitForLineEndSound = pendingWaitForLineEndSound;
        currentPauseBgmDuringLineEndSound = pendingPauseBgmDuringLineEndSound;
        currentBgmResumeFadeDuration = pendingBgmResumeFadeDuration;
        ClearPendingLineEndSound();
        lineEndSoundStarted = false;
        lineEndSoundBlockingInput = false;
        openedFrame = Time.frameCount;
        IsOpen = true;

        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }

        // 비활성 DialogueUI 아래의 SlidingTypewriterText는 최초 활성화 시 Awake에서
        // Text를 초기화하므로, UI 초기화가 끝난 뒤 첫 문장을 설정해야 한다.
        if (currentDialogueText != null && currentSlidingText == null)
        {
            currentDialogueText.text = currentFullText;
        }

        if (currentDialogueText != null)
        {
            StartPage(1);
        }
    }

    private void SetBasicLayout()
    {
        if (portraitLayout != null)
        {
            portraitLayout.SetActive(false);
        }

        if (basicLayout != null)
        {
            basicLayout.SetActive(true);
        }
    }

    private void SetPortraitLayout()
    {
        if (basicLayout != null)
        {
            basicLayout.SetActive(false);
        }

        if (portraitLayout != null)
        {
            portraitLayout.SetActive(true);
        }
    }

    private void SetPortraitDetailsVisible(bool visible)
    {
        if (portraitPanel != null)
        {
            portraitPanel.SetActive(visible);
        }

        if (speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(visible);
        }

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(visible && portraitImage.sprite != null);
            portraitImage.enabled = visible && portraitImage.sprite != null;
        }
    }

    private void ClearPortraitState()
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = "";
            speakerNameText.gameObject.SetActive(true);
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = true;
            portraitImage.gameObject.SetActive(true);
        }

        if (portraitPanel != null)
        {
            portraitPanel.SetActive(true);
        }
    }

    private void AdvanceOrClose()
    {
        if (!IsOpen || currentDialogueText == null)
        {
            return;
        }

        if (Time.frameCount == pageStartedFrame)
        {
            return;
        }

        if (lineEndSoundBlockingInput)
        {
            return;
        }

        if (IsCurrentTextTyping())
        {
            CompleteCurrentPage();
            return;
        }

        if (currentSlidingText != null && currentSlidingText.IsSliding)
        {
            return;
        }

        TMP_Text activeText = currentDialogueText;
        int pageCount = currentSlidingText != null
            ? currentSlidingText.PageCount
            : GetPreparedPageCount(PrepareTextInfo(activeText));
        if (activeText.pageToDisplay < pageCount)
        {
            StartPage(activeText.pageToDisplay + 1);
            return;
        }

        Close();
    }

    private void StartPage(int pageNumber)
    {
        if (currentDialogueText == null)
        {
            return;
        }

        StopTypewriter();

        if (currentSlidingText != null)
        {
            pageStartedFrame = Time.frameCount;
            soundCharacterCounter = 0;
            currentDialogueText = currentSlidingText.Play(
                currentFullText,
                pageNumber,
                currentInstantText,
                PlayTypewriterSoundIfNeeded,
                () =>
                {
                    isTyping = false;
                    TryPlayLineEndSoundIfReady();
                });

            if (currentDialogueText == null)
            {
                return;
            }

            currentPageLastCharacterIndex = currentSlidingText.LastCharacterIndex;
            isTyping = currentSlidingText.IsTyping;
            return;
        }

        currentDialogueText.pageToDisplay = Mathf.Max(1, pageNumber);
        TMP_TextInfo textInfo = PrepareTextInfo(currentDialogueText);
        if (textInfo == null)
        {
            return;
        }

        int pageCount = GetPreparedPageCount(textInfo);
        currentDialogueText.pageToDisplay = Mathf.Clamp(currentDialogueText.pageToDisplay, 1, pageCount);

        int pageIndex = currentDialogueText.pageToDisplay - 1;
        int firstCharacterIndex = 0;
        int characterCount = GetPreparedCharacterCount(textInfo, currentDialogueText);
        int lastCharacterIndex = Mathf.Max(-1, characterCount - 1);
        if (textInfo.characterCount > 0 && textInfo.pageInfo != null && textInfo.pageInfo.Length > pageIndex)
        {
            TMP_PageInfo pageInfo = textInfo.pageInfo[pageIndex];
            if (pageInfo.lastCharacterIndex >= pageInfo.firstCharacterIndex)
            {
                firstCharacterIndex = pageInfo.firstCharacterIndex;
                lastCharacterIndex = pageInfo.lastCharacterIndex;
            }
        }

        currentPageLastCharacterIndex = lastCharacterIndex;
        pageStartedFrame = Time.frameCount;
        soundCharacterCounter = 0;

        if (currentInstantText)
        {
            currentDialogueText.maxVisibleCharacters = lastCharacterIndex + 1;
            isTyping = false;
            typewriterCoroutine = null;
            TryPlayLineEndSoundIfReady();
            return;
        }

        currentDialogueText.maxVisibleCharacters = Mathf.Max(0, firstCharacterIndex);
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(firstCharacterIndex, lastCharacterIndex));
    }

    private TMP_TextInfo PrepareTextInfo(TMP_Text textComponent)
    {
        if (textComponent == null)
        {
            return null;
        }

        textComponent.ForceMeshUpdate(true, true);
        TMP_TextInfo textInfo = textComponent.textInfo;
        if (textInfo == null || (textInfo.characterCount == 0 && CountVisibleCharacters(textComponent.text) > 0))
        {
            textInfo = textComponent.GetTextInfo(textComponent.text ?? "");
        }

        return textInfo;
    }

    private int GetPreparedPageCount(TMP_TextInfo textInfo)
    {
        return Mathf.Max(1, textInfo != null ? textInfo.pageCount : 0);
    }

    private int GetPreparedCharacterCount(TMP_TextInfo textInfo, TMP_Text textComponent)
    {
        if (textInfo != null && textInfo.characterCount > 0)
        {
            return textInfo.characterCount;
        }

        return CountVisibleCharacters(textComponent != null ? textComponent.text : null);
    }

    private int CountVisibleCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        int count = 0;
        bool insideRichTextTag = false;
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if (character == '<')
            {
                insideRichTextTag = true;
                continue;
            }

            if (insideRichTextTag)
            {
                if (character == '>')
                {
                    insideRichTextTag = false;
                }

                continue;
            }

            count++;
        }

        return count;
    }

    private IEnumerator TypewriterRoutine(int firstCharacterIndex, int lastCharacterIndex)
    {
        isTyping = true;

        if (lastCharacterIndex < firstCharacterIndex)
        {
            CompleteCurrentPage();
            yield break;
        }

        float safeInterval = Mathf.Max(0f, characterInterval);

        for (int i = firstCharacterIndex; i <= lastCharacterIndex; i++)
        {
            currentDialogueText.maxVisibleCharacters = i + 1;
            PlayTypewriterSoundIfNeeded(i);

            if (safeInterval > 0f)
            {
                yield return new WaitForSeconds(safeInterval);
            }
            else
            {
                yield return null;
            }
        }

        isTyping = false;
        typewriterCoroutine = null;
        TryPlayLineEndSoundIfReady();
    }

    private void CompleteCurrentPage()
    {
        if (currentSlidingText != null)
        {
            currentSlidingText.CompleteTyping();
            isTyping = false;
            return;
        }

        StopTypewriter();
        isTyping = false;

        if (currentDialogueText != null)
        {
            currentDialogueText.maxVisibleCharacters = currentPageLastCharacterIndex + 1;
        }

        TryPlayLineEndSoundIfReady();
    }

    private void StopTypewriter()
    {
        if (currentSlidingText != null)
        {
            currentSlidingText.CancelTyping();
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
    }

    private void StopLineEndSoundWait()
    {
        if (lineEndSoundCoroutine != null)
        {
            StopCoroutine(lineEndSoundCoroutine);
            lineEndSoundCoroutine = null;
        }

        if (lineEndSoundBlockingInput)
        {
            lineEndSoundBlockingInput = false;
            ResumeBgmAfterLineEndSoundIfNeeded();
        }
    }

    private void TryPlayLineEndSoundIfReady()
    {
        if (lineEndSoundStarted || currentLineEndSound == null || currentDialogueText == null)
        {
            return;
        }

        if (!IsCurrentPageLastPage())
        {
            return;
        }

        lineEndSoundStarted = true;

        if (currentPauseBgmDuringLineEndSound && BGMManager.Instance != null)
        {
            BGMManager.Instance.PauseBGM();
        }

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(currentLineEndSound, currentLineEndSoundVolume);
        }

        if (currentWaitForLineEndSound)
        {
            lineEndSoundCoroutine = StartCoroutine(LineEndSoundRoutine(currentLineEndSound.length));
            return;
        }

        ResumeBgmAfterLineEndSoundIfNeeded();
    }

    private IEnumerator LineEndSoundRoutine(float soundLength)
    {
        lineEndSoundBlockingInput = true;

        float wait = Mathf.Max(0f, soundLength);
        if (wait > 0f)
        {
            yield return new WaitForSeconds(wait);
        }

        lineEndSoundBlockingInput = false;
        lineEndSoundCoroutine = null;
        ResumeBgmAfterLineEndSoundIfNeeded();
    }

    private void ResumeBgmAfterLineEndSoundIfNeeded()
    {
        if (currentPauseBgmDuringLineEndSound && BGMManager.Instance != null)
        {
            BGMManager.Instance.ResumeBGMWithFade(currentBgmResumeFadeDuration);
        }
    }

    private bool IsCurrentPageLastPage()
    {
        if (currentDialogueText == null)
        {
            return false;
        }

        if (currentSlidingText != null)
        {
            return currentSlidingText.CurrentPage >= currentSlidingText.PageCount;
        }

        TMP_TextInfo textInfo = PrepareTextInfo(currentDialogueText);
        return textInfo == null ||
               currentDialogueText.pageToDisplay >= GetPreparedPageCount(textInfo);
    }

    private void ClearPendingLineEndSound()
    {
        pendingLineEndSound = null;
        pendingLineEndSoundVolume = 1f;
        pendingWaitForLineEndSound = false;
        pendingPauseBgmDuringLineEndSound = false;
        pendingBgmResumeFadeDuration = 1f;
    }

    private void ResetVisibleCharacters()
    {
        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        if (basicDialogueText != null)
        {
            basicDialogueText.maxVisibleCharacters = int.MaxValue;
        }

        if (portraitDialogueText != null)
        {
            portraitDialogueText.maxVisibleCharacters = int.MaxValue;
        }
    }

    private bool IsCurrentTextTyping()
    {
        return currentSlidingText != null
            ? currentSlidingText.IsTyping
            : isTyping;
    }

    private SlidingTypewriterText ResolveSlidingText(TMP_Text targetText)
    {
        if (targetText == portraitDialogueText)
        {
            return portraitSlidingText != null
                ? portraitSlidingText
                : FindSlidingTextInParents(targetText);
        }

        if (targetText == basicDialogueText || targetText == dialogueText)
        {
            return basicSlidingText != null
                ? basicSlidingText
                : FindSlidingTextInParents(targetText);
        }

        return null;
    }

    private SlidingTypewriterText FindSlidingTextInParents(TMP_Text targetText)
    {
        return targetText != null
            ? targetText.GetComponentInParent<SlidingTypewriterText>(true)
            : null;
    }

    private void ResetSlidingTexts()
    {
        if (basicSlidingText != null)
        {
            basicSlidingText.ResetState(true);
        }

        if (portraitSlidingText != null && portraitSlidingText != basicSlidingText)
        {
            portraitSlidingText.ResetState(true);
        }
    }

    private void PlayTypewriterSoundIfNeeded(int characterIndex)
    {
        if (currentDialogueText == null ||
            typewriterAudioSource == null ||
            soundEveryNCharacters <= 0)
        {
            return;
        }

        AudioClip sound = currentTypewriterSoundOverride != null
            ? currentTypewriterSoundOverride
            : typewriterSound;

        if (sound == null)
        {
            return;
        }

        if (!TryGetCharacterForSound(characterIndex, out char character))
        {
            return;
        }

        if (char.IsWhiteSpace(character))
        {
            return;
        }

        soundCharacterCounter++;
        if (soundCharacterCounter % soundEveryNCharacters == 0)
        {
            if (ShouldSkipOverlappingOverrideSound(sound))
            {
                return;
            }

            typewriterAudioSource.PlayOneShot(sound);
        }
    }

    private bool TryGetCharacterForSound(int characterIndex, out char character)
    {
        character = '\0';

        if (currentDialogueText == null || characterIndex < 0)
        {
            return false;
        }

        TMP_TextInfo textInfo = currentDialogueText.textInfo;
        if (textInfo != null && characterIndex < textInfo.characterCount)
        {
            character = textInfo.characterInfo[characterIndex].character;
            return true;
        }

        return TryGetVisibleCharacter(currentDialogueText.text, characterIndex, out character);
    }

    private bool TryGetVisibleCharacter(string text, int visibleCharacterIndex, out char character)
    {
        character = '\0';

        if (string.IsNullOrEmpty(text) || visibleCharacterIndex < 0)
        {
            return false;
        }

        int currentVisibleIndex = 0;
        bool insideRichTextTag = false;
        for (int i = 0; i < text.Length; i++)
        {
            char current = text[i];
            if (current == '<')
            {
                insideRichTextTag = true;
                continue;
            }

            if (insideRichTextTag)
            {
                if (current == '>')
                {
                    insideRichTextTag = false;
                }

                continue;
            }

            if (currentVisibleIndex == visibleCharacterIndex)
            {
                character = current;
                return true;
            }

            currentVisibleIndex++;
        }

        return false;
    }

    private bool ShouldSkipOverlappingOverrideSound(AudioClip sound)
    {
        if (currentTypewriterSoundOverride == null || sound == null)
        {
            return false;
        }

        return sound.length > characterInterval && typewriterAudioSource.isPlaying;
    }
}
