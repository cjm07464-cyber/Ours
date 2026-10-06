using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SlidingTypewriterText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform textViewport;
    [SerializeField] private TMP_Text currentText;
    [SerializeField] private TMP_Text nextText;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float slideDuration = 0.24f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;
    [SerializeField, Min(0f)] private float characterInterval = 0.035f;

    private RectTransform currentRect;
    private RectTransform nextRect;
    private Vector2 currentBasePosition;
    private Vector2 nextBasePosition;
    private TMP_Text activeText;
    private TMP_Text standbyText;
    private Coroutine typewriterCoroutine;
    private Sequence slideSequence;
    private Action<int> onCharacterRevealed;
    private Action onTypingComplete;
    private bool hasDisplayedText;
    private bool positionsCached;
    private int lastCharacterIndex = -1;

    public TMP_Text ActiveText => activeText != null ? activeText : currentText;
    public bool IsTyping { get; private set; }
    public bool IsSliding { get; private set; }
    public int PageCount { get; private set; } = 1;
    public int CurrentPage { get; private set; } = 1;
    public int LastCharacterIndex => lastCharacterIndex;

    private void Awake()
    {
        CacheReferences();
        ResetState(true);
    }

    private void OnDisable()
    {
        ResetState(true);
    }

    public TMP_Text Play(
        string text,
        int pageNumber,
        bool instant,
        Action<int> characterRevealed = null,
        Action typingComplete = null)
    {
        CacheReferences();
        StopTypewriter(false);
        CompleteSlideImmediately();

        TMP_Text incomingText = GetIncomingText();
        if (incomingText == null)
        {
            typingComplete?.Invoke();
            return null;
        }

        RectTransform incomingRect = incomingText.rectTransform;
        Vector2 incomingBasePosition = GetBasePosition(incomingText);
        TMP_Text outgoingText = hasDisplayedText ? activeText : null;
        RectTransform outgoingRect = outgoingText != null ? outgoingText.rectTransform : null;
        Vector2 outgoingBasePosition = GetBasePosition(outgoingText);

        incomingText.text = text ?? string.Empty;
        incomingText.pageToDisplay = Mathf.Max(1, pageNumber);
        incomingText.ForceMeshUpdate(true, true);

        TMP_TextInfo textInfo = PrepareTextInfo(incomingText);
        PageCount = Mathf.Max(1, textInfo != null ? textInfo.pageCount : 0);
        CurrentPage = Mathf.Clamp(incomingText.pageToDisplay, 1, PageCount);
        incomingText.pageToDisplay = CurrentPage;

        GetPageCharacterRange(
            incomingText,
            textInfo,
            CurrentPage,
            out int firstCharacterIndex,
            out lastCharacterIndex);

        incomingText.maxVisibleCharacters = instant
            ? lastCharacterIndex + 1
            : Mathf.Max(0, firstCharacterIndex);
        incomingText.gameObject.SetActive(true);

        activeText = incomingText;
        standbyText = outgoingText;
        onCharacterRevealed = characterRevealed;
        onTypingComplete = typingComplete;

        if (outgoingText != null && outgoingText != incomingText && nextText != null)
        {
            StartSlide(
                outgoingText,
                outgoingRect,
                outgoingBasePosition,
                incomingRect,
                incomingBasePosition);
        }
        else if (incomingRect != null)
        {
            incomingRect.anchoredPosition = incomingBasePosition;
        }

        hasDisplayedText = true;

        if (instant || lastCharacterIndex < firstCharacterIndex || characterInterval <= 0f)
        {
            incomingText.maxVisibleCharacters = lastCharacterIndex + 1;
            IsTyping = false;
            InvokeTypingComplete();
            return incomingText;
        }

        typewriterCoroutine = StartCoroutine(
            TypewriterRoutine(firstCharacterIndex, lastCharacterIndex));
        return incomingText;
    }

    public void CompleteTyping()
    {
        if (!IsTyping)
        {
            return;
        }

        StopTypewriter(false);
        if (activeText != null)
        {
            activeText.maxVisibleCharacters = lastCharacterIndex + 1;
        }

        InvokeTypingComplete();
    }

    public void CancelTyping()
    {
        StopTypewriter(false);
        onCharacterRevealed = null;
        onTypingComplete = null;
    }

    public void ResetState(bool clearText)
    {
        StopTypewriter(false);

        if (slideSequence != null)
        {
            slideSequence.Kill();
            slideSequence = null;
        }

        CacheReferences();
        ResetTextTransform(currentText, currentBasePosition, clearText);
        ResetTextTransform(nextText, nextBasePosition, clearText);

        activeText = currentText;
        standbyText = nextText;
        hasDisplayedText = false;
        IsSliding = false;
        PageCount = 1;
        CurrentPage = 1;
        lastCharacterIndex = -1;
        onCharacterRevealed = null;
        onTypingComplete = null;
    }

    private void CacheReferences()
    {
        if (currentText != null)
        {
            currentRect = currentText.rectTransform;
        }

        if (nextText != null)
        {
            nextRect = nextText.rectTransform;
        }

        if (positionsCached)
        {
            return;
        }

        if (currentRect != null)
        {
            currentBasePosition = currentRect.anchoredPosition;
        }

        if (nextRect != null)
        {
            nextBasePosition = nextRect.anchoredPosition;
        }

        positionsCached = true;
    }

    private TMP_Text GetIncomingText()
    {
        if (!hasDisplayedText || nextText == null)
        {
            return currentText;
        }

        return activeText == currentText ? nextText : currentText;
    }

    private void StartSlide(
        TMP_Text outgoingText,
        RectTransform outgoingRect,
        Vector2 outgoingBasePosition,
        RectTransform incomingRect,
        Vector2 incomingBasePosition)
    {
        if (outgoingRect == null || incomingRect == null)
        {
            return;
        }

        float distance = GetSlideDistance();
        if (distance <= 0f || slideDuration <= 0f)
        {
            outgoingText.text = string.Empty;
            outgoingRect.anchoredPosition = outgoingBasePosition;
            incomingRect.anchoredPosition = incomingBasePosition;
            return;
        }

        outgoingRect.anchoredPosition = outgoingBasePosition;
        incomingRect.anchoredPosition = incomingBasePosition - Vector2.up * distance;
        IsSliding = true;

        slideSequence = DOTween.Sequence().SetUpdate(true);
        slideSequence.Join(
            outgoingRect.DOAnchorPos(
                    outgoingBasePosition + Vector2.up * distance,
                    slideDuration)
                .SetEase(slideEase));
        slideSequence.Join(
            incomingRect.DOAnchorPos(incomingBasePosition, slideDuration)
                .SetEase(slideEase));
        slideSequence.OnComplete(() =>
        {
            outgoingText.text = string.Empty;
            outgoingText.maxVisibleCharacters = int.MaxValue;
            outgoingRect.anchoredPosition = outgoingBasePosition;
            incomingRect.anchoredPosition = incomingBasePosition;
            standbyText = outgoingText;
            IsSliding = false;
            slideSequence = null;
        });
    }

    private IEnumerator TypewriterRoutine(int firstCharacterIndex, int finalCharacterIndex)
    {
        IsTyping = true;

        for (int i = firstCharacterIndex; i <= finalCharacterIndex; i++)
        {
            if (activeText == null)
            {
                break;
            }

            activeText.maxVisibleCharacters = i + 1;
            onCharacterRevealed?.Invoke(i);

            float elapsed = 0f;
            while (elapsed < characterInterval)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        typewriterCoroutine = null;
        IsTyping = false;
        InvokeTypingComplete();
    }

    private void StopTypewriter(bool invokeComplete)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        bool wasTyping = IsTyping;
        IsTyping = false;
        if (invokeComplete && wasTyping)
        {
            InvokeTypingComplete();
        }
    }

    private void InvokeTypingComplete()
    {
        Action callback = onTypingComplete;
        onTypingComplete = null;
        callback?.Invoke();
    }

    private void CompleteSlideImmediately()
    {
        if (slideSequence == null)
        {
            return;
        }

        slideSequence.Complete(true);
        slideSequence = null;
        IsSliding = false;
    }

    private float GetSlideDistance()
    {
        if (textViewport != null && textViewport.rect.height > 0f)
        {
            return textViewport.rect.height;
        }

        RectTransform fallback = activeText != null ? activeText.rectTransform : currentRect;
        return fallback != null ? fallback.rect.height : 0f;
    }

    private Vector2 GetBasePosition(TMP_Text textComponent)
    {
        if (textComponent == currentText)
        {
            return currentBasePosition;
        }

        if (textComponent == nextText)
        {
            return nextBasePosition;
        }

        return textComponent != null ? textComponent.rectTransform.anchoredPosition : Vector2.zero;
    }

    private void ResetTextTransform(TMP_Text textComponent, Vector2 basePosition, bool clearText)
    {
        if (textComponent == null)
        {
            return;
        }

        textComponent.rectTransform.DOKill();
        textComponent.rectTransform.anchoredPosition = basePosition;
        textComponent.maxVisibleCharacters = int.MaxValue;
        if (clearText)
        {
            textComponent.text = string.Empty;
        }
    }

    private TMP_TextInfo PrepareTextInfo(TMP_Text textComponent)
    {
        if (textComponent == null)
        {
            return null;
        }

        textComponent.ForceMeshUpdate(true, true);
        TMP_TextInfo textInfo = textComponent.textInfo;
        if (textInfo == null ||
            (textInfo.characterCount == 0 && CountVisibleCharacters(textComponent.text) > 0))
        {
            textInfo = textComponent.GetTextInfo(textComponent.text ?? string.Empty);
        }

        return textInfo;
    }

    private void GetPageCharacterRange(
        TMP_Text textComponent,
        TMP_TextInfo textInfo,
        int pageNumber,
        out int firstCharacterIndex,
        out int finalCharacterIndex)
    {
        firstCharacterIndex = 0;
        int characterCount = textInfo != null && textInfo.characterCount > 0
            ? textInfo.characterCount
            : CountVisibleCharacters(textComponent != null ? textComponent.text : null);
        finalCharacterIndex = Mathf.Max(-1, characterCount - 1);

        int pageIndex = Mathf.Max(0, pageNumber - 1);
        if (textInfo == null ||
            textInfo.characterCount <= 0 ||
            textInfo.pageInfo == null ||
            textInfo.pageInfo.Length <= pageIndex)
        {
            return;
        }

        TMP_PageInfo pageInfo = textInfo.pageInfo[pageIndex];
        if (pageInfo.lastCharacterIndex >= pageInfo.firstCharacterIndex)
        {
            firstCharacterIndex = pageInfo.firstCharacterIndex;
            finalCharacterIndex = pageInfo.lastCharacterIndex;
        }
    }

    private int CountVisibleCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        int count = 0;
        bool insideRichTextTag = false;
        foreach (char character in text)
        {
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
}
