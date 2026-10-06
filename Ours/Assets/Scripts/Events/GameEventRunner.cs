using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEventRunner : MonoBehaviour
{
    private const string DadSaveCallIntroName = "Dad_SaveCall_Intro";

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

    [Header("Move Actor Context")]
    [SerializeField] private Transform moveActor;
    [SerializeField] private Transform moveTarget;
    [SerializeField] private SpriteRenderer moveActorSpriteRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite moveSprite;
    [SerializeField] private bool facePlayerOnEventStart;
    [SerializeField] private bool enableMoveFlipAnimation;
    [SerializeField] private float moveFlipInterval = 0.15f;

    [Header("Quit Game")]
    [SerializeField] private Image quitFadeOverlay;
    [SerializeField] private float quitFadeDuration = 1.5f;

    private Action onComplete;
    private bool quitGameRequested;
    private bool battleRequested;
    private bool moveActorVisualPrepared;
    private bool moveActorInitialFlipX;

    public bool IsRunning { get; private set; }

    private void Start()
    {
        if (moveActorSpriteRenderer != null && idleSprite != null)
        {
            moveActorSpriteRenderer.sprite = idleSprite;
        }
    }

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

    private void OnDisable()
    {
        if (!IsRunning || battleRequested || quitGameRequested)
        {
            return;
        }

        StopAllCoroutines();
        ResetMoveActorVisual();

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

    private IEnumerator RunRoutine(GameEventSequence sequence)
    {
        IsRunning = true;
        quitGameRequested = false;
        battleRequested = false;

        if (lockPlayerDuringEvent)
        {
            EnsurePlayerController();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
        }

        PrepareMoveActorVisual();

        if (sequence != null && sequence.steps != null)
        {
            foreach (GameEventStep step in sequence.steps)
            {
                yield return RunStepRoutine(step);
                if (quitGameRequested || battleRequested)
                {
                    yield break;
                }
            }
        }

        ResetMoveActorVisual();

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
            case GameEventStepType.QuitGame:
                yield return RunQuitGameStep();
                break;
            case GameEventStepType.StartBattle:
                StartBattle(step);
                break;
            case GameEventStepType.MoveActor:
                yield return RunMoveActorStep(step);
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

    private IEnumerator RunQuitGameStep()
    {
        if (quitGameRequested)
        {
            yield break;
        }

        quitGameRequested = true;

        if (lockPlayerDuringEvent)
        {
            EnsurePlayerController();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
        }

        float duration = Mathf.Max(0.01f, quitFadeDuration);
        Sequence quitSequence = DOTween.Sequence();

        if (quitFadeOverlay != null)
        {
            quitFadeOverlay.gameObject.SetActive(true);
            quitFadeOverlay.transform.SetAsLastSibling();

            Color color = quitFadeOverlay.color;
            color.r = 0f;
            color.g = 0f;
            color.b = 0f;
            color.a = Mathf.Clamp01(color.a);
            quitFadeOverlay.color = color;

            quitSequence.Join(quitFadeOverlay.DOFade(1f, duration).SetEase(Ease.Linear));
        }

        if (BGMManager.Instance != null)
        {
            Tween bgmFadeTween = BGMManager.Instance.FadeOutOverDuration(duration);
            if (bgmFadeTween != null)
            {
                quitSequence.Join(bgmFadeTween);
            }
        }

        if (!quitSequence.IsActive() || quitSequence.Duration() <= 0f)
        {
            yield return new WaitForSecondsRealtime(duration);
        }
        else
        {
            yield return quitSequence.WaitForCompletion();
        }

        QuitApplication();
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        Debug.Log("[DEV] QuitGame requested.");
#else
        Application.Quit();
#endif
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
            if (quitGameRequested || battleRequested)
            {
                yield break;
            }
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

        DialogueSequence sequenceToRun = sequence;
        DialogueSequence runtimeSequence = null;
        int pendingGoldLineIndex = -1;

        if (TryCreateDadSaveCallRuntimeSequence(
                sequence,
                out runtimeSequence,
                out pendingGoldLineIndex))
        {
            sequenceToRun = runtimeSequence;
        }

        bool completed = false;
        bool pendingGoldDeposited = false;
        dialogueRunner.Run(
            sequenceToRun,
            () => completed = true,
            lineIndex =>
            {
                if (pendingGoldDeposited || lineIndex != pendingGoldLineIndex)
                {
                    return;
                }

                pendingGoldDeposited = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DepositPendingGold();
                }
            });

        while (!completed)
        {
            yield return null;
        }

        if (runtimeSequence != null)
        {
            Destroy(runtimeSequence);
        }
    }

    private bool TryCreateDadSaveCallRuntimeSequence(
        DialogueSequence source,
        out DialogueSequence runtimeSequence,
        out int pendingGoldLineIndex)
    {
        runtimeSequence = null;
        pendingGoldLineIndex = -1;

        if (source == null ||
            source.name != DadSaveCallIntroName)
        {
            return false;
        }

        runtimeSequence = ScriptableObject.CreateInstance<DialogueSequence>();
        runtimeSequence.name = $"{source.name} (Runtime)";
        runtimeSequence.lines = new List<DialogueLine>();

        DialogueLine dadTemplate = FindDadDialogueTemplate(source.lines);
        int dadTemplateIndex = -1;

        if (source.lines != null)
        {
            for (int i = 0; i < source.lines.Count; i++)
            {
                DialogueLine sourceLine = source.lines[i];
                DialogueLine runtimeLine = CloneDialogueLine(sourceLine);

                if (sourceLine == dadTemplate)
                {
                    dadTemplateIndex = i;
                }

                if (IsDynamicDadLine(runtimeLine, dadTemplate))
                {
                    CopySpeakerPresentation(dadTemplate, runtimeLine);
                }

                runtimeSequence.lines.Add(runtimeLine);
            }
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.pendingGold > 0 && dadTemplate != null)
        {
            DialogueLine pendingGoldLine = CloneDialogueLine(dadTemplate);
            pendingGoldLine.text =
                $"아빠가 용돈으로 {{player}} 계좌에 {gameManager.pendingGold}G 넣어놨다.";

            pendingGoldLineIndex = Mathf.Clamp(
                dadTemplateIndex + 1,
                0,
                runtimeSequence.lines.Count);
            runtimeSequence.lines.Insert(pendingGoldLineIndex, pendingGoldLine);
        }

        return true;
    }

    private DialogueLine FindDadDialogueTemplate(List<DialogueLine> lines)
    {
        if (lines == null)
        {
            return null;
        }

        DialogueLine fallback = null;
        foreach (DialogueLine line in lines)
        {
            if (line == null || line.speaker == null)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = line;
            }

            if (line.showPortrait && line.showSpeakerName)
            {
                return line;
            }
        }

        return fallback;
    }

    private bool IsDynamicDadLine(DialogueLine line, DialogueLine dadTemplate)
    {
        return line != null &&
               dadTemplate != null &&
               line.speaker == dadTemplate.speaker &&
               !string.IsNullOrEmpty(line.text) &&
               line.text.Contains("{");
    }

    private void CopySpeakerPresentation(DialogueLine source, DialogueLine destination)
    {
        if (source == null || destination == null)
        {
            return;
        }

        destination.speaker = source.speaker;
        destination.expressionId = source.expressionId;
        destination.showPortrait = source.showPortrait;
        destination.showSpeakerName = source.showSpeakerName;
    }

    private DialogueLine CloneDialogueLine(DialogueLine source)
    {
        if (source == null)
        {
            return null;
        }

        return new DialogueLine
        {
            speaker = source.speaker,
            expressionId = source.expressionId,
            text = source.text,
            showPortrait = source.showPortrait,
            showSpeakerName = source.showSpeakerName,
            lineStartSound = source.lineStartSound,
            stopLineStartSoundOnAdvance = source.stopLineStartSoundOnAdvance,
            instantText = source.instantText,
            lineEndSound = source.lineEndSound,
            lineEndSoundVolume = source.lineEndSoundVolume,
            waitForLineEndSound = source.waitForLineEndSound,
            pauseBgmDuringLineEndSound = source.pauseBgmDuringLineEndSound,
            bgmResumeFadeDuration = source.bgmResumeFadeDuration
        };
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

    private void StartBattle(GameEventStep step)
    {
        if (step == null || step.battleEnemyData == null)
        {
            Debug.LogError("GameEventRunner: StartBattle Step에 EnemyData가 없습니다.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameEventRunner: StartBattle에 필요한 GameManager가 없습니다.");
            return;
        }

        EnsurePlayerController();

        Vector2 returnPosition = playerController != null
            ? (Vector2)playerController.transform.position
            : Vector2.zero;
        Vector2 returnFacing = playerController != null
            ? playerController.GetFacingDirection()
            : GameManager.Instance.playerFacingDirection;

        string sceneName = string.IsNullOrWhiteSpace(step.battleSceneName)
            ? "BattleScene"
            : step.battleSceneName;

        GameManager.Instance.currentBattleEnemy = step.battleEnemyData;
        GameManager.Instance.currentBattleEnemyId = step.battleEncounterId ?? "";
        GameManager.Instance.RequestBattleReturn(
            SceneManager.GetActiveScene().name,
            returnPosition,
            returnFacing);
        GameManager.Instance.victoryStoryFlag = step.victoryStoryFlag ?? "";

        battleRequested = true;

        BattleTransitionEffect transitionEffect = FindObjectOfType<BattleTransitionEffect>();
        if (transitionEffect != null)
        {
            transitionEffect.battleSceneName = sceneName;
            transitionEffect.Play();
            return;
        }

        Debug.LogWarning("GameEventRunner: BattleTransitionEffect가 없어 바로 전투 Scene으로 이동합니다.");
        BattleTransitionEffect.BeginTransition();
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator RunMoveActorStep(GameEventStep step)
    {
        if (moveActor == null || moveTarget == null)
        {
            Debug.LogError("GameEventRunner: MoveActor Context의 Actor 또는 Target이 연결되지 않았습니다.");
            yield break;
        }

        float speed = Mathf.Max(0.01f, step.moveActorSpeed);
        float tolerance = Mathf.Max(0f, step.moveActorArrivalTolerance);
        float flipInterval = Mathf.Max(0.01f, moveFlipInterval);
        float flipTimer = 0f;
        bool baseFlipX = moveActorSpriteRenderer != null && moveActorSpriteRenderer.flipX;

        if (moveActorSpriteRenderer != null && moveSprite != null)
        {
            moveActorSpriteRenderer.sprite = moveSprite;
        }

        while (Vector3.Distance(moveActor.position, moveTarget.position) > tolerance)
        {
            if (enableMoveFlipAnimation && moveActorSpriteRenderer != null)
            {
                flipTimer += Time.deltaTime;
                if (flipTimer >= flipInterval)
                {
                    flipTimer -= flipInterval;
                    moveActorSpriteRenderer.flipX = !moveActorSpriteRenderer.flipX;
                }
            }

            moveActor.position = Vector3.MoveTowards(
                moveActor.position,
                moveTarget.position,
                speed * Time.deltaTime);
            yield return null;
        }

        moveActor.position = moveTarget.position;

        if (moveActorSpriteRenderer != null)
        {
            moveActorSpriteRenderer.flipX = baseFlipX;
        }
    }

    private void PrepareMoveActorVisual()
    {
        if (moveActorSpriteRenderer == null)
        {
            return;
        }

        moveActorInitialFlipX = moveActorSpriteRenderer.flipX;
        moveActorVisualPrepared = true;

        if (moveSprite != null)
        {
            moveActorSpriteRenderer.sprite = moveSprite;
        }

        if (!facePlayerOnEventStart || moveActor == null)
        {
            return;
        }

        EnsurePlayerController();
        if (playerController == null)
        {
            return;
        }

        float horizontalOffset = playerController.transform.position.x - moveActor.position.x;
        if (Mathf.Abs(horizontalOffset) > 0.001f)
        {
            moveActorSpriteRenderer.flipX = horizontalOffset < 0f;
        }
    }

    private void ResetMoveActorVisual()
    {
        if (!moveActorVisualPrepared || moveActorSpriteRenderer == null)
        {
            return;
        }

        if (idleSprite != null)
        {
            moveActorSpriteRenderer.sprite = idleSprite;
        }

        moveActorSpriteRenderer.flipX = moveActorInitialFlipX;
        moveActorVisualPrepared = false;
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
