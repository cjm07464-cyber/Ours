using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TownOpeningEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private Image fadeOverlay;

    [Header("Opening Dialogue")]
    [TextArea]
    [SerializeField] private string[] openingDialogues =
    {
        ".........",
        "{playerName}...!",
        ".....나..!"
    };
    [SerializeField] private string fallbackPlayerName = "할로";

    [Header("Mother")]
    [SerializeField] private CharacterData motherCharacter;
    [SerializeField] private Sprite motherPortrait;
    [SerializeField] private Color motherBackgroundColor = Color.white;
    [SerializeField] private Color motherGradientColor = Color.white;
    [SerializeField] private string motherSpeakerName = "엄마";
    [TextArea]
    [SerializeField] private string motherDialogue = "일어나...! 학교 가야지!";

    [Header("Timing")]
    [SerializeField] private float townFadeInDuration = 1.5f;

    [Header("Testing")]
    [SerializeField] private bool forcePlayInEditor = false;

    private bool isPlaying;

    private void Start()
    {
        if (!ShouldPlayOpening())
        {
            return;
        }

        StartCoroutine(OpeningRoutine());
    }

    private bool ShouldPlayOpening()
    {
        bool requested = GameManager.Instance != null && GameManager.Instance.ConsumeTownOpeningRequest();

#if UNITY_EDITOR
        requested = requested || forcePlayInEditor;
#endif

        return requested;
    }

    private IEnumerator OpeningRoutine()
    {
        if (isPlaying)
        {
            yield break;
        }

        isPlaying = true;
        SetPlayerCanMove(false);
        SetFadeOverlayAlpha(1f);
        StopTownBgmForOpening();

        PlayOpeningDialogueAt(0);
    }

    private void PlayOpeningDialogueAt(int index)
    {
        if (dialogueController == null || openingDialogues == null || index >= openingDialogues.Length)
        {
            StartCoroutine(FadeInThenPlayMotherRoutine());
            return;
        }

        string text = ResolveDialogueText(openingDialogues[index]);
        dialogueController.ShowPortraitBoxOnly(text, GetMotherTypeSound(), () => PlayOpeningDialogueAt(index + 1));
    }

    private IEnumerator FadeInThenPlayMotherRoutine()
    {
        if (fadeOverlay != null)
        {
            float duration = Mathf.Max(0.01f, townFadeInDuration);
            Tween tween = fadeOverlay.DOFade(0f, duration).SetEase(Ease.Linear);
            yield return tween.WaitForCompletion();
        }

        PlayMotherDialogue();
    }

    private void PlayMotherDialogue()
    {
        if (dialogueController == null)
        {
            FinishOpening();
            return;
        }

        dialogueController.ShowPortrait(
            motherDialogue,
            GetMotherSpeakerName(),
            GetMotherPortrait(),
            GetMotherBackgroundColor(),
            GetMotherGradientColor(),
            true,
            GetMotherTypeSound(),
            FinishOpening);
    }

    private void FinishOpening()
    {
        PlayTownBgmAfterOpening();
        SetPlayerCanMove(true);
        isPlaying = false;
    }

    private string ResolveDialogueText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        return text.Replace("{playerName}", GetPlayerName());
    }

    private string GetPlayerName()
    {
        if (GameManager.Instance != null && !string.IsNullOrWhiteSpace(GameManager.Instance.playerName))
        {
            return GameManager.Instance.playerName;
        }

        return fallbackPlayerName;
    }

    private void SetPlayerCanMove(bool canMove)
    {
        if (playerController != null)
        {
            playerController.SetCanMove(canMove);
        }
    }

    private string GetMotherSpeakerName()
    {
        if (motherCharacter != null && !string.IsNullOrWhiteSpace(motherCharacter.displayName))
        {
            return motherCharacter.displayName;
        }

        return motherSpeakerName;
    }

    private Sprite GetMotherPortrait()
    {
        if (motherCharacter != null)
        {
            Sprite portrait = motherCharacter.GetPortrait("normal");
            if (portrait != null)
            {
                return portrait;
            }
        }

        return motherPortrait;
    }

    private Color GetMotherBackgroundColor()
    {
        return motherCharacter != null ? motherCharacter.portraitBackgroundColor : motherBackgroundColor;
    }

    private Color GetMotherGradientColor()
    {
        return motherCharacter != null ? motherCharacter.portraitGradientColor : motherGradientColor;
    }

    private AudioClip GetMotherTypeSound()
    {
        return motherCharacter != null ? motherCharacter.dialogueTypeSound : null;
    }

    private void StopTownBgmForOpening()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.StopBGM();
        }
    }

    private void PlayTownBgmAfterOpening()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayBGM();
        }
    }

    private void SetFadeOverlayAlpha(float alpha)
    {
        if (fadeOverlay == null)
        {
            return;
        }

        fadeOverlay.gameObject.SetActive(true);
        Color color = fadeOverlay.color;
        color.a = Mathf.Clamp01(alpha);
        fadeOverlay.color = color;
    }
}
