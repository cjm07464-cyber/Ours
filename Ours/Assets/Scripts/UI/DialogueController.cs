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

    [Header("Portrait")]
    [SerializeField] private GameObject portraitPanel;
    [SerializeField] private TMP_Text portraitDialogueText;
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
    private AudioClip currentTypewriterSoundOverride;
    private bool currentInstantText;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
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

        IsOpen = false;
        StopTypewriter();
        ResetVisibleCharacters();
        ClearPortraitState();

        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }

        currentDialogueText = null;
        currentTypewriterSoundOverride = null;
        currentInstantText = false;

        callback?.Invoke();
    }

    private void Open(string text, TMP_Text targetText, AudioClip typewriterSoundOverride, bool instantText, Action onClosed)
    {
        this.onClosed = onClosed;
        currentDialogueText = targetText;
        currentTypewriterSoundOverride = typewriterSoundOverride;
        currentInstantText = instantText;
        openedFrame = Time.frameCount;
        IsOpen = true;

        if (currentDialogueText != null)
        {
            currentDialogueText.text = text;
        }

        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
            Canvas.ForceUpdateCanvases();
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

        if (isTyping)
        {
            CompleteCurrentPage();
            return;
        }

        TMP_Text activeText = currentDialogueText;
        TMP_TextInfo textInfo = PrepareTextInfo(activeText);
        if (textInfo != null && activeText.pageToDisplay < GetPreparedPageCount(textInfo))
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
    }

    private void CompleteCurrentPage()
    {
        StopTypewriter();
        isTyping = false;

        if (currentDialogueText != null)
        {
            currentDialogueText.maxVisibleCharacters = currentPageLastCharacterIndex + 1;
        }
    }

    private void StopTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
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
