using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrologueController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image newsImage;
    [SerializeField] private Image fadeOverlay;

    [Header("News Images")]
    [SerializeField] private Sprite meteorNewsSprite;
    [SerializeField] private Sprite researchNewsSprite;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [TextArea]
    [SerializeField] private string[] meteorDialogues;
    [TextArea]
    [SerializeField] private string[] researchDialogues;

    [Header("Audio")]
    [SerializeField] private AudioSource tvTurnAudioSource;
    [SerializeField] private AudioSource tvNoiseAudioSource;

    [Header("Timing")]
    [SerializeField] private float initialBlackDelay = 1.0f;
    [SerializeField] private float afterTvTurnDelay = 0.5f;
    [SerializeField] private float openingFadeDuration = 1.5f;
    [SerializeField] private float newsChangeFadeDuration = 0.5f;
    [SerializeField] private float endingFadeDuration = 1.5f;

    [Header("Scene")]
    [SerializeField] private string townSceneName = GameManager.TownSceneName;

    private bool endingStarted;
    private Sequence activeSequence;
    private Coroutine prologueCoroutine;
    private int meteorDialogueIndex;
    private int researchDialogueIndex;

    private void Start()
    {
        PrepareInitialState();
        prologueCoroutine = StartCoroutine(PrologueRoutine());
    }

    private void PrepareInitialState()
    {
        if (tvTurnAudioSource != null)
        {
            tvTurnAudioSource.Stop();
        }

        if (tvNoiseAudioSource != null)
        {
            tvNoiseAudioSource.Stop();
        }

        if (newsImage != null)
        {
            newsImage.sprite = meteorNewsSprite;
            SetImageAlpha(newsImage, 1f);
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.transform.SetAsLastSibling();
            SetImageColor(fadeOverlay, Color.black, 1f);
        }
    }

    private IEnumerator PrologueRoutine()
    {
        float blackDelay = Mathf.Max(0f, initialBlackDelay);
        if (blackDelay > 0f)
        {
            yield return new WaitForSeconds(blackDelay);
        }

        if (tvTurnAudioSource != null)
        {
            tvTurnAudioSource.Play();
        }

        float turnDelay = Mathf.Max(0f, afterTvTurnDelay);
        if (turnDelay > 0f)
        {
            yield return new WaitForSeconds(turnDelay);
        }

        if (tvNoiseAudioSource != null)
        {
            tvNoiseAudioSource.Play();
        }

        yield return FadeImageRoutine(fadeOverlay, 0f, openingFadeDuration);

        PlayMeteorDialogueAt(0);
    }

    private void PlayMeteorDialogueAt(int index)
    {
        meteorDialogueIndex = index;
        if (!TryShowDialogueEntry(meteorDialogues, meteorDialogueIndex, out int shownIndex))
        {
            StartCoroutine(SwitchToResearchRoutine());
            return;
        }

        dialogueController.Show(meteorDialogues[shownIndex], () => PlayMeteorDialogueAt(shownIndex + 1));
    }

    private IEnumerator SwitchToResearchRoutine()
    {
        yield return SwitchNewsImageRoutine(researchNewsSprite);
        PlayResearchDialogueAt(0);
    }

    private void PlayResearchDialogueAt(int index)
    {
        researchDialogueIndex = index;
        if (!TryShowDialogueEntry(researchDialogues, researchDialogueIndex, out int shownIndex))
        {
            StartEnding();
            return;
        }

        dialogueController.Show(researchDialogues[shownIndex], () => PlayResearchDialogueAt(shownIndex + 1));
    }

    private bool TryShowDialogueEntry(string[] dialogues, int index, out int shownIndex)
    {
        shownIndex = -1;

        if (dialogueController == null || dialogues == null)
        {
            return false;
        }

        while (index < dialogues.Length && string.IsNullOrEmpty(dialogues[index]))
        {
            index++;
        }

        if (index >= dialogues.Length)
        {
            return false;
        }

        shownIndex = index;
        return true;
    }

    private IEnumerator SwitchNewsImageRoutine(Sprite nextSprite)
    {
        yield return FadeImageRoutine(newsImage, 0f, newsChangeFadeDuration);

        if (newsImage != null)
        {
            newsImage.sprite = nextSprite;
        }

        yield return FadeImageRoutine(newsImage, 1f, newsChangeFadeDuration);
    }

    private IEnumerator FadeImageRoutine(Image image, float targetAlpha, float duration)
    {
        if (image == null)
        {
            yield break;
        }

        float safeDuration = Mathf.Max(0.01f, duration);
        Tween tween = image.DOFade(targetAlpha, safeDuration).SetEase(Ease.Linear);
        yield return tween.WaitForCompletion();
    }

    private void StartEnding()
    {
        if (endingStarted)
        {
            return;
        }

        endingStarted = true;
        float duration = Mathf.Max(0.01f, endingFadeDuration);

        activeSequence?.Kill();
        activeSequence = DOTween.Sequence();

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.transform.SetAsLastSibling();
            activeSequence.Join(fadeOverlay.DOFade(1f, duration).SetEase(Ease.Linear));
        }

        if (tvNoiseAudioSource != null)
        {
            activeSequence.Join(tvNoiseAudioSource.DOFade(0f, duration).SetEase(Ease.Linear));
        }

        if (fadeOverlay == null && tvNoiseAudioSource == null)
        {
            LoadTownScene();
            return;
        }

        activeSequence.OnComplete(LoadTownScene);
    }

    private void LoadTownScene()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RequestTownOpening();
        }

        if (tvTurnAudioSource != null && tvTurnAudioSource.isPlaying)
        {
            tvTurnAudioSource.Stop();
        }

        if (tvNoiseAudioSource != null && tvNoiseAudioSource.isPlaying)
        {
            tvNoiseAudioSource.Stop();
        }

        SceneManager.LoadScene(townSceneName);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void SetImageColor(Image image, Color color, float alpha)
    {
        if (image == null)
        {
            return;
        }

        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void OnDestroy()
    {
        if (prologueCoroutine != null)
        {
            StopCoroutine(prologueCoroutine);
            prologueCoroutine = null;
        }

        activeSequence?.Kill();
    }
}
