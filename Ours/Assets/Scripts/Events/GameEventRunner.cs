using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEventRunner : MonoBehaviour
{
    [Header("Context")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private ChoiceUIController choiceUIController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool lockPlayerDuringEvent = true;

    [Header("Reaction Context")]
    [SerializeField] private GameObject reactionIcon;
    [SerializeField] private AudioSource reactionAudioSource;
    [SerializeField] private AudioClip defaultReactionSound;
    [SerializeField] private float reactionWaitDuration = 0.6f;

    private Action onComplete;

    public bool IsRunning { get; private set; }

    public void Run(GameEventSequence sequence, Action onComplete = null)
    {
        if (IsRunning)
        {
            return;
        }

        this.onComplete = onComplete;
        StartCoroutine(RunRoutine(sequence));
    }

    public void SetPlayerController(PlayerController controller)
    {
        if (controller != null)
        {
            playerController = controller;
        }
    }

    private IEnumerator RunRoutine(GameEventSequence sequence)
    {
        IsRunning = true;

        if (lockPlayerDuringEvent)
        {
            EnsurePlayerController();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
        }

        if (sequence != null && sequence.steps != null)
        {
            foreach (GameEventStep step in sequence.steps)
            {
                yield return RunStepRoutine(step);
            }
        }

        if (lockPlayerDuringEvent)
        {
            EnsurePlayerController();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }

        IsRunning = false;
        Action callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    private void EnsurePlayerController()
    {
        if (playerController != null)
        {
            return;
        }

        playerController = FindObjectOfType<PlayerController>();
    }

    private IEnumerator RunStepRoutine(GameEventStep step)
    {
        if (step == null)
        {
            yield break;
        }

        switch (step.stepType)
        {
            case GameEventStepType.Dialogue:
                yield return RunDialogueStep(step.dialogueSequence);
                break;
            case GameEventStepType.SystemMessage:
                yield return RunSystemMessageStep(step);
                break;
            case GameEventStepType.GiveItem:
                GiveItem(step.itemId, step.count);
                break;
            case GameEventStepType.SetStoryFlag:
                SetStoryFlag(step.flagId);
                break;
            case GameEventStepType.Reaction:
                yield return RunReactionStep();
                break;
            case GameEventStepType.Choice:
                yield return RunChoiceStep(step);
                break;
            case GameEventStepType.SaveGame:
                CaptureCurrentSaveContext();
                SaveSystem.SaveGame();
                break;
        }
    }

    private IEnumerator RunChoiceStep(GameEventStep step)
    {
        if (choiceUIController == null || step == null)
        {
            yield break;
        }

        bool completed = false;
        bool selectedYes = false;
        choiceUIController.Show(choice =>
        {
            selectedYes = choice;
            completed = true;
        });

        while (!completed)
        {
            yield return null;
        }

        GameEventSequence nextSequence = selectedYes
            ? step.yesEventSequence
            : step.noEventSequence;

        if (nextSequence != null)
        {
            yield return RunNestedSequenceRoutine(nextSequence);
        }
    }

    private IEnumerator RunNestedSequenceRoutine(GameEventSequence sequence)
    {
        if (sequence == null || sequence.steps == null)
        {
            yield break;
        }

        foreach (GameEventStep step in sequence.steps)
        {
            yield return RunStepRoutine(step);
        }
    }

    private IEnumerator RunSystemMessageStep(GameEventStep step)
    {
        if (dialogueController == null || step == null)
        {
            yield break;
        }

        if (step.pauseBgmWhileOpen && BGMManager.Instance != null)
        {
            BGMManager.Instance.PauseBGM();
        }

        if (step.messageSound != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(step.messageSound);
        }

        bool completed = false;
        dialogueController.ShowInstant(ResolveText(step.messageText), () => completed = true);

        while (!completed)
        {
            yield return null;
        }

        if (step.pauseBgmWhileOpen && BGMManager.Instance != null)
        {
            BGMManager.Instance.ResumeBGMWithFade(step.bgmResumeFadeDuration);
        }
    }

    private IEnumerator RunDialogueStep(DialogueSequence sequence)
    {
        if (dialogueRunner == null || sequence == null)
        {
            yield break;
        }

        bool completed = false;
        dialogueRunner.Run(sequence, () => completed = true);

        while (!completed)
        {
            yield return null;
        }
    }

    private void GiveItem(string itemId, int count)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddItem(itemId, count);
        }
    }

    private void SetStoryFlag(string flagId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetStoryFlag(flagId);
        }
    }

    private void CaptureCurrentSaveContext()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.currentSceneName = SceneManager.GetActiveScene().name;

        Transform playerTransform = null;
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
        else
        {
            GameObject playerObject = null;
            try
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                playerObject = null;
            }

            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        if (playerTransform != null)
        {
            GameManager.Instance.playerPosition = playerTransform.position;
        }
    }

    private IEnumerator RunReactionStep()
    {
        if (reactionIcon != null)
        {
            reactionIcon.SetActive(true);
        }

        if (reactionAudioSource != null && defaultReactionSound != null)
        {
            reactionAudioSource.PlayOneShot(defaultReactionSound);
        }

        float wait = Mathf.Max(0f, reactionWaitDuration);
        if (wait > 0f)
        {
            yield return new WaitForSeconds(wait);
        }

        if (reactionIcon != null)
        {
            reactionIcon.SetActive(false);
        }
    }

    private string ResolveText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        GameManager gameManager = GameManager.Instance;
        string playerName = "할로";
        string level = "1";
        string expToNextLevel = "0";

        if (gameManager != null)
        {
            if (!string.IsNullOrWhiteSpace(gameManager.playerName))
            {
                playerName = gameManager.playerName;
            }

            level = gameManager.level.ToString();
            expToNextLevel = gameManager.GetExpToNextLevel().ToString();
        }

        return text
            .Replace("{player}", playerName)
            .Replace("{level}", level)
            .Replace("{expToNextLevel}", expToNextLevel);
    }
}
