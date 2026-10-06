using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    private enum BattleState
    {
        StartMessage,
        PlayerCommand,
        PlayerAction,
        EnemyAction,
        Victory,
        Defeat,
        GameOver,
        Returning
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject messagePanel;
    [FormerlySerializedAs("commandPanel")]
    [SerializeField] private GameObject battleCommandUI;
    [SerializeField] private GameObject skillPanel;

    [Header("Character Status Slot Animation")]
    [SerializeField] private RectTransform[] characterStatusSlots;
    [SerializeField] private float statusSlotEntryOffsetY = 250f;
    [SerializeField] private float statusSlotEntryDuration = 0.35f;
    [SerializeField] private float activeTurnYOffset = 12f;
    [SerializeField] private float activeTurnMoveDuration = 0.15f;

    [Header("Battle Command UI Entry")]
    [SerializeField] private float commandUIEntryOffsetY = 250f;
    [SerializeField] private float commandUIEntryDuration = 0.35f;

    [Header("Curtain Entry")]
    [SerializeField] private float curtainEntryOffset;
    [SerializeField] private float curtainEntryDuration = 0.35f;

    [Header("UI Images")]
    [SerializeField] private Image enemyImage;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Battle Message")]
    [SerializeField] private SlidingTypewriterText messageSlidingText;
    [SerializeField] private float messageCharacterInterval = 0.035f;
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private RectTransform gameOverSelector;
    [SerializeField] private RectTransform gameOverContinueText;
    [SerializeField] private RectTransform gameOverQuitText;
    [Header("Enemy")]
    [SerializeField] private EnemyData testEnemyData;
    private EnemyData enemyData;

    [Header("Skills")]
    [SerializeField] private SkillSelector skillSelector;
    [FormerlySerializedAs("pkHealSkill")]
    [SerializeField] private SkillData espHealSkill;
    [SerializeField] private SkillData espThunderAlphaSkill;

    [Header("Battle Settings")]
    [SerializeField] private string defaultReturnSceneName = GameManager.TownSceneName;
    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float battleEntryFadeDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource battleBgmSource;

    [Header("Phone Dialogue")]
    [SerializeField] private DialogueRunner phoneDialogueRunner;
    [SerializeField] private CharacterData phoneDadCharacter;
    [SerializeField] private AudioClip phoneRingSound;
    [SerializeField] private AudioClip phoneHangupSound;

    [Header("Battle Effects")]
    [SerializeField] private Transform effectLayer;
    [SerializeField] private GameObject basicAttackEffectPrefab;
    [SerializeField, Min(0f)] private float basicAttackImpactDelay = 0.1f;

    [Header("Enemy Death Presentation")]
    [SerializeField] private float enemyDeathBlinkDuration = 0.2f;
    [SerializeField] private int enemyDeathBlinkCount = 2;
    [SerializeField] private float enemyDeathFadeDuration = 0.5f;

    [Header("Enemy Action Presentation")]
    [SerializeField] private float enemyActionBlinkDuration = 0.2f;
    [SerializeField] private int enemyActionBlinkCount = 2;
    [SerializeField, Range(0f, 1f)] private float enemyActionFlashAlpha = 0.35f;
    [SerializeField] private AudioClip enemyActionSound;
    [SerializeField, Min(0f)] private float enemyActionSoundVolumeScale = 1f;

    [Header("Player Hit Presentation")]
    [SerializeField] private float hitShakeStrengthY = 25f;
    [SerializeField] private float hitShakeDuration = 0.35f;
    [SerializeField] private int hitShakeVibrato = 12;
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField, Min(0f)] private float playerHitSoundVolumeScale = 1f;

    [Header("Victory Presentation")]
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private float winDisplayDuration = 1.5f;
    [SerializeField] private RectTransform topCurtain;
    [SerializeField] private RectTransform bottomCurtain;
    [SerializeField] private float curtainExitDistance;
    [SerializeField] private float curtainExitDuration = 0.6f;
    [SerializeField] private Image victoryBackgroundDimImage;
    [SerializeField, Range(0f, 1f)] private float victoryBackgroundDimAlpha = 0.25f;
    [SerializeField] private float victoryBackgroundDimDuration = 0.4f;
    [SerializeField] private AudioClip victoryBgmClip;
    [SerializeField, Range(0f, 1f)] private float victoryBgmVolume = 0.7f;
    private BattleState state;

    private int enemyCurrentHP;
    private int enemyCurrentMP;

    private int gameOverSelectedIndex = 0; // 0 = 다시 일어서기, 1 = 그만하기

    private SkillData runtimeESPHealSkill;

    private bool inputLocked;
    private Vector2[] statusSlotBasePositions;
    private CharacterStatusSlotUI[] characterStatusSlotUIs;
    private Coroutine statusSlotTurnCoroutine;
    private RectTransform battleCommandUIRect;
    private Vector2 battleCommandUIBasePosition;
    private CommandSelector commandSelector;
    private RectTransform messagePanelRect;
    private Vector2 messagePanelBasePosition;
    private Color enemyImageOriginalColor = Color.white;
    private bool enemyImageOriginalColorCached;
    private Vector2 topCurtainBasePosition;
    private Vector2 bottomCurtainBasePosition;
    private const int PlayerPartyMemberIndex = 0;
    private readonly HashSet<int> defendingPartyMemberIndices = new HashSet<int>();

    private void Awake()
    {
        PrepareInitialMessageText();
        PrepareBattleEntryFade();
    }

    private void Start()
    {
        CacheStatusSlotBasePositions();
        PrepareStatusSlotsForEntry();
        CacheCommandUIEntryState();
        PrepareCommandUIForEntry();
        CacheMessagePanelState();

        if (GameManager.Instance == null)
        {
            Debug.LogError("BattleManager: GameManager가 없습니다.");
            enabled = false;
            return;
        }

        ResolveEnemyData();
        ResolveEnemyImage();

        if (enemyData == null)
        {
            Debug.LogError("BattleManager: 사용할 EnemyData가 없습니다.");
            enabled = false;
            return;
        }

        InitializeEnemy();
        RefreshPlayerStatusUI();
        PrepareVictoryPresentation();
        PrepareCurtainsForEntry();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        SetCommandUI(false);
        SetSkillPanel(false);
        SetMessagePanel(true);

        state = BattleState.StartMessage;
        inputLocked = true;
        StartCoroutine(ShowStartMessageRoutine());
    }

    private void ResolveEnemyData()
    {
        if (GameManager.Instance.currentBattleEnemy != null)
        {
            enemyData = GameManager.Instance.currentBattleEnemy;
        }
        else
        {
            enemyData = testEnemyData;
        }
    }

    private void ResolveEnemyImage()
    {
        if (enemyImage != null)
        {
            return;
        }

        GameObject enemyImageObject = GameObject.Find("Canvas/EnemyLayer/EnemyImage");
        if (enemyImageObject != null)
        {
            enemyImage = enemyImageObject.GetComponent<Image>();
        }

        if (enemyImage == null)
        {
            Debug.LogError("BattleManager: Enemy Image가 연결되지 않았습니다.");
        }
    }

    private void Update()
    {
        if (inputLocked)
        {
            return;
        }

        switch (state)
        {
            case BattleState.StartMessage:
                HandleStartMessageInput();
                break;

            case BattleState.GameOver:
                HandleGameOverInput();
                break;
        }
    }

    private void InitializeEnemy()
    {
        enemyCurrentHP = enemyData.maxHP;
        enemyCurrentMP = enemyData.maxMP;

        if (enemyImage != null)
        {
            enemyImage.DOKill();
            BlinkEffect blinkEffect = enemyImage.GetComponent<BlinkEffect>();
            if (blinkEffect != null)
            {
                blinkEffect.StopBlinking();
            }

            if (!enemyImageOriginalColorCached)
            {
                enemyImageOriginalColor = enemyImage.color;
                enemyImageOriginalColorCached = true;
            }

            Color restoredColor = enemyImageOriginalColor;
            restoredColor.a = 1f;
            enemyImage.color = restoredColor;

            if (enemyData.enemySprite != null)
            {
                enemyImage.sprite = enemyData.enemySprite;
                enemyImage.gameObject.SetActive(true);
            }
            else
            {
                enemyImage.gameObject.SetActive(false);
            }
        }
    }

    private void HandleStartMessageInput()
    {
        if (!GameInput.ConfirmPressed)
        {
            return;
        }

        inputLocked = true;
        StartCoroutine(FinishStartMessageRoutine());
    }

    private IEnumerator ShowStartMessageRoutine()
    {
        yield return BattleEntryFadeInRoutine();
        yield return ShowBattleMessageRoutine($"{enemyData.enemyName}가 나타났다!");
        inputLocked = false;
    }

    private IEnumerator BattleEntryFadeInRoutine()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        // Awake 이후 다른 초기화 순서와 무관하게 첫 렌더 직전 상태를 다시 보장한다.
        PrepareBattleEntryFade();

        // 완전한 검정 상태를 실제 화면에 최소 한 프레임 표시한다.
        yield return null;

        float duration = Mathf.Max(0f, battleEntryFadeDuration);
        float timer = 0f;
        Color color = Color.black;

        while (timer < duration)
        {
            // Scene 로드 직후 프레임의 큰 unscaledDeltaTime은 Fade 시간에 포함하지 않는다.
            yield return null;
            timer += Time.unscaledDeltaTime;
            color.a = 1f - Mathf.Clamp01(timer / duration);
            fadeImage.color = color;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    private void PrepareInitialMessageText()
    {
        if (messageSlidingText != null)
        {
            messageSlidingText.ResetState(true);
        }

        if (messageText == null)
        {
            return;
        }

        messageText.text = string.Empty;
        messageText.maxVisibleCharacters = 0;
        messageText.ForceMeshUpdate(true, true);
    }

    private void PrepareBattleEntryFade()
    {
        if (fadeImage == null)
        {
            return;
        }

        Transform current = fadeImage.transform;
        while (current != null)
        {
            current.gameObject.SetActive(true);
            if (current.GetComponent<Canvas>() != null)
            {
                break;
            }

            current = current.parent;
        }

        Canvas canvas = fadeImage.GetComponentInParent<Canvas>();
        Transform sortingRoot = fadeImage.transform;
        if (canvas != null)
        {
            while (sortingRoot.parent != null && sortingRoot.parent != canvas.transform)
            {
                sortingRoot = sortingRoot.parent;
            }
        }

        sortingRoot.SetAsLastSibling();

        fadeImage.color = Color.black;
        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator FinishStartMessageRoutine()
    {
        SetMessagePanel(false);
        SetCommandSelectorInputEnabled(false);

        Coroutine statusEntry = StartCoroutine(PlayStatusSlotEntryRoutine());
        Coroutine curtainEntry = StartCoroutine(PlayCurtainEntryRoutine());
        yield return ShowBattleCommandUI();
        if (statusEntry != null)
        {
            yield return statusEntry;
        }

        if (curtainEntry != null)
        {
            yield return curtainEntry;
        }

        if (IsEnemyFaster())
        {
            yield return EnemyTurnRoutine();
        }
        else
        {
            yield return OpenCommandSelectRoutine();
        }
    }

    private bool IsEnemyFaster()
    {
        return enemyData.speed > GameManager.Instance.speed;
    }

    private IEnumerator OpenCommandSelectRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerCommand;
        ClearDefendingPartyMembers();
        SetActiveStatusSlot(0);

        SetSkillPanel(false);
        SetMessagePanel(false);
        RefreshPlayerStatusUI();

        yield return ShowBattleCommandUI();

        inputLocked = false;
        SetCommandSelectorInputEnabled(true);
    }

    private void OpenCommandSelect()
    {
        StartCoroutine(OpenCommandSelectRoutine());
    }

    private IEnumerator PlayerAttackRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerAction;
        ClearActiveStatusSlotImmediately();

        yield return HideBattleCommandUI();
        SetMessagePanel(true);

        int damage = CalculatePhysicalDamage(
            GameManager.Instance.GetEffectiveAttack(),
            GameManager.Instance.luck,
            enemyData.defense,
            out _);

        yield return ShowBattleMessageAndWaitForConfirm(
            $"{GameManager.Instance.playerName}의 공격!");
        yield return ShowBattleMessageRoutine(
            $"{enemyData.enemyName}에게\n{damage}의 데미지!");
        yield return WaitMessage(Mathf.Max(0f, basicAttackImpactDelay));

        OneShotSpriteAnimation attackEffect = SpawnBasicAttackEffect();
        if (attackEffect != null)
        {
            yield return attackEffect.WaitForHitFrame();
        }

        enemyCurrentHP -= damage;
        if (enemyCurrentHP < 0)
        {
            enemyCurrentHP = 0;
        }

        if (attackEffect != null)
        {
            yield return attackEffect.WaitForCompletion();
        }

        if (enemyCurrentHP <= 0)
        {
            yield return VictoryRoutine();
            yield break;
        }

        yield return EnemyTurnRoutine();
    }

    private OneShotSpriteAnimation SpawnBasicAttackEffect()
    {
        if (basicAttackEffectPrefab == null)
        {
            return null;
        }

        if (basicAttackEffectPrefab.scene.IsValid())
        {
            Debug.LogError(
                "BattleManager: Basic Attack Effect Prefab에는 Scene 인스턴스가 아니라 Project의 Prefab Asset을 연결해야 합니다.");
            return null;
        }

        Transform parent = effectLayer != null
            ? effectLayer
            : enemyImage != null ? enemyImage.transform.parent : null;

        if (parent != null && !parent.gameObject.activeSelf)
        {
            parent.gameObject.SetActive(true);
        }

        RectTransform prefabRect = basicAttackEffectPrefab.GetComponent<RectTransform>();
        GameObject spawnedEffect = Instantiate(basicAttackEffectPrefab, parent, false);
        if (enemyImage != null)
        {
            RectTransform effectRect = spawnedEffect.GetComponent<RectTransform>();
            if (effectRect != null)
            {
                effectRect.localScale = Vector3.one;
                effectRect.localRotation = Quaternion.identity;
                if (prefabRect != null)
                {
                    effectRect.sizeDelta = prefabRect.sizeDelta;
                }

                effectRect.position = enemyImage.rectTransform.position;
            }
            else
            {
                spawnedEffect.transform.position = enemyImage.transform.position;
            }
        }

        OneShotSpriteAnimation animation =
            spawnedEffect.GetComponentInChildren<OneShotSpriteAnimation>(true);
        if (animation == null)
        {
            Debug.LogWarning("BattleManager: Basic Attack Effect에 OneShotSpriteAnimation이 없습니다.");
            Destroy(spawnedEffect);
        }

        return animation;
    }

    private IEnumerator EnemyTurnRoutine()
    {
        inputLocked = true;
        state = BattleState.EnemyAction;
        SetActiveStatusSlot(-1);

        yield return HideBattleCommandUI();
        SetMessagePanel(true);

        int damage = CalculatePhysicalDamage(
            enemyData.attackPower,
            enemyData.luck,
            GetPartyMemberBattleDefense(PlayerPartyMemberIndex),
            out _);

        yield return ShowEnemyActionDeclarationRoutine(
            $"{enemyData.enemyName}은(는) {GameManager.Instance.playerName}에게 공격!");

        PlayBattleSfx(playerHitSound, playerHitSoundVolumeScale);
        Coroutine hitVisual = StartCoroutine(PlayPlayerHitPresentationRoutine(0));
        yield return ShowBattleMessageRoutine(
            $"{GameManager.Instance.playerName}에게\n{damage}의 데미지!");
        if (hitVisual != null)
        {
            yield return hitVisual;
        }

        GameManager.Instance.currentHP -= damage;
        if (GameManager.Instance.currentHP < 0)
        {
            GameManager.Instance.currentHP = 0;
        }

        RefreshPlayerStatusUI();
        yield return WaitForConfirm();
        yield return WaitUntilConfirmReleased();

        SetPartyMemberDefending(PlayerPartyMemberIndex, false);

        if (IsPartyDefeated())
        {
            yield return DefeatRoutine();
            yield break;
        }

        yield return OpenCommandSelectRoutine();
    }

    private IEnumerator EscapeRoutine()
    {
        inputLocked = true;
        state = BattleState.Returning;
        ClearDefendingPartyMembers();
        ClearActiveStatusSlotImmediately();

        yield return HideBattleCommandUI();
        SetMessagePanel(true);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.escapedEnemyId = GameManager.Instance.currentBattleEnemyId;
            GameManager.Instance.victoryStoryFlag = "";
        }
        yield return ShowBattleMessageAndWaitForConfirm("도망쳤다!");
        yield return ReturnToFieldRoutine();
    }

    private IEnumerator VictoryRoutine()
    {
        state = BattleState.Victory;
        inputLocked = true;
        ClearDefendingPartyMembers();
        SetActiveStatusSlot(-1);

        SetCommandSelectorInputEnabled(false);
        SetSkillPanel(false);
        SetMessagePanel(true);

        int gainedExp = enemyData.expReward;
        int gainedGold = enemyData.goldReward;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.defeatedEnemyId = GameManager.Instance.currentBattleEnemyId;

            if (!string.IsNullOrWhiteSpace(GameManager.Instance.victoryStoryFlag))
            {
                GameManager.Instance.SetStoryFlag(GameManager.Instance.victoryStoryFlag);
            }

            GameManager.Instance.victoryStoryFlag = "";
        }

        Coroutine enemyDeathVisual = StartCoroutine(PlayEnemyDeathVisualRoutine());
        yield return ShowBattleMessageRoutine($"{enemyData.enemyName}는(은) 조용해졌다!");
        if (enemyDeathVisual != null)
        {
            yield return enemyDeathVisual;
        }

        SetMessagePanel(false);
        yield return PlayVictoryPresentationRoutine();

        GameManager.Instance.AddPendingGold(gainedGold);
        string levelUpMessage = AddExpAndBuildLevelUpMessage(gainedExp);

        SetMessagePanel(true);
        yield return ShowBattleMessageAndWaitForConfirm(
            $"{GameManager.Instance.playerName}은(는) {gainedExp}의 경험치를 얻었다!");

        if (!string.IsNullOrEmpty(levelUpMessage))
        {
            yield return ShowBattleMessageAndWaitForConfirm(levelUpMessage);
        }
        yield return ReturnToFieldRoutine();
    }

    private IEnumerator EscapeBlockedRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerAction;
        ClearActiveStatusSlotImmediately();

        yield return HideBattleCommandUI();
        SetSkillPanel(false);
        SetMessagePanel(true);

        string message = string.IsNullOrWhiteSpace(enemyData.escapeBlockMessage)
            ? "도망칠 수 없다!"
            : enemyData.escapeBlockMessage;
        yield return ShowBattleMessageAndWaitForConfirm(message);
        yield return OpenCommandSelectRoutine();
    }

    private void PrepareVictoryPresentation()
    {
        if (winText != null)
        {
            winText.text = "YOU WIN!";
            winText.gameObject.SetActive(false);
        }

        if (victoryBackgroundDimImage != null)
        {
            victoryBackgroundDimImage.DOKill();
            Color dimColor = victoryBackgroundDimImage.color;
            dimColor.a = 0f;
            victoryBackgroundDimImage.color = dimColor;
            victoryBackgroundDimImage.gameObject.SetActive(false);
        }

        if (topCurtain != null)
        {
            topCurtain.DOKill();
            topCurtainBasePosition = topCurtain.anchoredPosition;
        }

        if (bottomCurtain != null)
        {
            bottomCurtain.DOKill();
            bottomCurtainBasePosition = bottomCurtain.anchoredPosition;
        }
    }

    private void PrepareCurtainsForEntry()
    {
        Canvas.ForceUpdateCanvases();

        if (topCurtain != null)
        {
            topCurtain.DOKill();
            Vector2 position = topCurtainBasePosition;
            position.y += GetCurtainOffscreenDistance(topCurtain, true, curtainEntryOffset);
            topCurtain.anchoredPosition = position;
        }

        if (bottomCurtain != null)
        {
            bottomCurtain.DOKill();
            Vector2 position = bottomCurtainBasePosition;
            position.y -= GetCurtainOffscreenDistance(bottomCurtain, false, curtainEntryOffset);
            bottomCurtain.anchoredPosition = position;
        }
    }

    private IEnumerator PlayCurtainEntryRoutine()
    {
        float duration = Mathf.Max(0f, curtainEntryDuration);
        Sequence entrySequence = DOTween.Sequence().SetUpdate(true);
        bool hasTween = false;

        if (topCurtain != null)
        {
            topCurtain.DOKill();
            if (duration > 0f)
            {
                entrySequence.Join(topCurtain
                    .DOAnchorPos(topCurtainBasePosition, duration)
                    .SetEase(Ease.InOutSine));
                hasTween = true;
            }
            else
            {
                topCurtain.anchoredPosition = topCurtainBasePosition;
            }
        }

        if (bottomCurtain != null)
        {
            bottomCurtain.DOKill();
            if (duration > 0f)
            {
                entrySequence.Join(bottomCurtain
                    .DOAnchorPos(bottomCurtainBasePosition, duration)
                    .SetEase(Ease.InOutSine));
                hasTween = true;
            }
            else
            {
                bottomCurtain.anchoredPosition = bottomCurtainBasePosition;
            }
        }

        if (hasTween)
        {
            yield return entrySequence.WaitForCompletion();
        }
        else
        {
            entrySequence.Kill();
        }

        if (topCurtain != null)
        {
            topCurtain.anchoredPosition = topCurtainBasePosition;
        }

        if (bottomCurtain != null)
        {
            bottomCurtain.anchoredPosition = bottomCurtainBasePosition;
        }
    }

    private IEnumerator PlayEnemyDeathVisualRoutine()
    {
        if (enemyImage == null)
        {
            yield break;
        }

        enemyImage.DOKill();
        BlinkEffect blinkEffect = enemyImage.GetComponent<BlinkEffect>();
        if (blinkEffect != null)
        {
            blinkEffect.StopBlinking();
        }

        Color originalColor = enemyImageOriginalColorCached
            ? enemyImageOriginalColor
            : enemyImage.color;
        originalColor.a = 1f;
        Color blackColor = Color.black;
        blackColor.a = 1f;

        float blinkDuration = Mathf.Max(0.01f, enemyDeathBlinkDuration);
        float fadeDuration = Mathf.Max(0f, enemyDeathFadeDuration);
        int blinkCount = Mathf.Max(0, enemyDeathBlinkCount);

        Sequence deathSequence = DOTween.Sequence().SetUpdate(true);
        for (int i = 0; i < blinkCount; i++)
        {
            deathSequence.Append(enemyImage.DOColor(blackColor, blinkDuration).SetEase(Ease.InOutSine));
            deathSequence.Append(enemyImage.DOColor(originalColor, blinkDuration).SetEase(Ease.InOutSine));
        }

        if (fadeDuration > 0f)
        {
            deathSequence.Append(enemyImage.DOFade(0f, fadeDuration).SetEase(Ease.InOutSine));
        }
        else
        {
            Color transparentColor = originalColor;
            transparentColor.a = 0f;
            enemyImage.color = transparentColor;
        }

        yield return deathSequence.WaitForCompletion();

        Color finalColor = enemyImage.color;
        finalColor.a = 0f;
        enemyImage.color = finalColor;
    }

    private IEnumerator PlayEnemyActionVisualRoutine()
    {
        if (enemyImage == null)
        {
            yield break;
        }

        enemyImage.DOKill();
        BlinkEffect blinkEffect = enemyImage.GetComponent<BlinkEffect>();
        if (blinkEffect != null)
        {
            blinkEffect.StopBlinking();
        }

        Color originalColor = enemyImageOriginalColorCached
            ? enemyImageOriginalColor
            : enemyImage.color;
        originalColor.a = 1f;
        Color flashColor = Color.white;
        flashColor.a = Mathf.Clamp01(enemyActionFlashAlpha);

        float blinkDuration = Mathf.Max(0.01f, enemyActionBlinkDuration);
        int blinkCount = Mathf.Max(0, enemyActionBlinkCount);
        if (blinkCount == 0)
        {
            enemyImage.color = originalColor;
            yield break;
        }

        Sequence actionSequence = DOTween.Sequence().SetUpdate(true);
        for (int i = 0; i < blinkCount; i++)
        {
            actionSequence.Append(enemyImage.DOColor(flashColor, blinkDuration).SetEase(Ease.InOutSine));
            actionSequence.Append(enemyImage.DOColor(originalColor, blinkDuration).SetEase(Ease.InOutSine));
        }

        yield return actionSequence.WaitForCompletion();
        enemyImage.color = originalColor;
    }

    private IEnumerator ShowEnemyActionDeclarationRoutine(string message)
    {
        bool messageCompleted = false;
        bool visualCompleted = false;

        PlayBattleSfx(enemyActionSound, enemyActionSoundVolumeScale);
        StartCoroutine(RunRoutineAndNotify(
            ShowBattleMessageRoutine(message),
            () => messageCompleted = true));
        StartCoroutine(RunRoutineAndNotify(
            PlayEnemyActionVisualRoutine(),
            () => visualCompleted = true));

        while (!messageCompleted || !visualCompleted)
        {
            yield return null;
        }

        yield return WaitForConfirm();
        yield return WaitUntilConfirmReleased();
    }

    private IEnumerator RunRoutineAndNotify(IEnumerator routine, System.Action onComplete)
    {
        if (routine != null)
        {
            yield return routine;
        }

        onComplete?.Invoke();
    }

    private IEnumerator PlayPlayerHitPresentationRoutine(int targetStatusSlotIndex)
    {
        float duration = Mathf.Max(0f, hitShakeDuration);
        float strengthY = Mathf.Max(0f, hitShakeStrengthY);
        int vibrato = Mathf.Max(1, hitShakeVibrato);
        RectTransform targetStatusSlot = null;

        if (characterStatusSlots != null &&
            targetStatusSlotIndex >= 0 &&
            targetStatusSlotIndex < characterStatusSlots.Length)
        {
            targetStatusSlot = characterStatusSlots[targetStatusSlotIndex];
        }

        if (messagePanelRect == null)
        {
            CacheMessagePanelState();
        }

        if (duration <= 0f || strengthY <= 0f)
        {
            RestoreHitShakeTargets(targetStatusSlotIndex, targetStatusSlot);
            yield break;
        }

        Sequence hitSequence = DOTween.Sequence().SetUpdate(true);
        bool hasTween = false;

        if (messagePanelRect != null)
        {
            messagePanelRect.DOKill();
            messagePanelRect.anchoredPosition = messagePanelBasePosition;
            hitSequence.Join(messagePanelRect.DOShakeAnchorPos(
                duration,
                new Vector2(0f, strengthY),
                vibrato,
                0f,
                false,
                true,
                ShakeRandomnessMode.Harmonic));
            hasTween = true;
        }

        if (targetStatusSlot != null)
        {
            targetStatusSlot.DOKill();
            if (statusSlotBasePositions != null &&
                targetStatusSlotIndex < statusSlotBasePositions.Length)
            {
                targetStatusSlot.anchoredPosition = statusSlotBasePositions[targetStatusSlotIndex];
            }

            hitSequence.Join(targetStatusSlot.DOShakeAnchorPos(
                duration,
                new Vector2(0f, strengthY),
                vibrato,
                0f,
                false,
                true,
                ShakeRandomnessMode.Harmonic));
            hasTween = true;
        }

        if (hasTween)
        {
            yield return hitSequence.WaitForCompletion();
        }
        else
        {
            hitSequence.Kill();
        }

        RestoreHitShakeTargets(targetStatusSlotIndex, targetStatusSlot);
    }

    private void RestoreHitShakeTargets(int targetStatusSlotIndex, RectTransform targetStatusSlot)
    {
        if (messagePanelRect != null)
        {
            messagePanelRect.anchoredPosition = messagePanelBasePosition;
        }

        if (targetStatusSlot != null &&
            statusSlotBasePositions != null &&
            targetStatusSlotIndex >= 0 &&
            targetStatusSlotIndex < statusSlotBasePositions.Length)
        {
            targetStatusSlot.anchoredPosition = statusSlotBasePositions[targetStatusSlotIndex];
        }
    }

    private void PlayBattleSfx(AudioClip clip, float volumeScale)
    {
        if (clip == null)
        {
            return;
        }

        float safeVolumeScale = Mathf.Max(0f, volumeScale);
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(clip, safeVolumeScale);
            return;
        }

        Vector3 playPosition = Camera.main != null
            ? Camera.main.transform.position
            : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, playPosition, safeVolumeScale);
    }

    private IEnumerator PlayVictoryPresentationRoutine()
    {
        Coroutine commandExit = null;
        if (battleCommandUI != null && battleCommandUI.activeSelf)
        {
            commandExit = StartCoroutine(HideBattleCommandUI());
        }

        PlayVictoryBgm();

        if (winText != null)
        {
            winText.text = "YOU WIN!";
            winText.gameObject.SetActive(true);
        }

        Tween dimInTween = null;
        if (victoryBackgroundDimImage != null)
        {
            victoryBackgroundDimImage.DOKill();
            victoryBackgroundDimImage.gameObject.SetActive(true);
            Color dimColor = victoryBackgroundDimImage.color;
            dimColor.a = 0f;
            victoryBackgroundDimImage.color = dimColor;
            dimInTween = victoryBackgroundDimImage
                .DOFade(Mathf.Clamp01(victoryBackgroundDimAlpha), Mathf.Max(0.01f, victoryBackgroundDimDuration))
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        float curtainDuration = Mathf.Max(0.01f, curtainExitDuration);
        if (topCurtain != null)
        {
            topCurtain.DOKill();
            topCurtain.anchoredPosition = topCurtainBasePosition;
            float topDistance = GetCurtainExitDistance(topCurtain, true);
            topCurtain.DOAnchorPosY(topCurtainBasePosition.y + topDistance, curtainDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        if (bottomCurtain != null)
        {
            bottomCurtain.DOKill();
            bottomCurtain.anchoredPosition = bottomCurtainBasePosition;
            float bottomDistance = GetCurtainExitDistance(bottomCurtain, false);
            bottomCurtain.DOAnchorPosY(bottomCurtainBasePosition.y - bottomDistance, curtainDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        float displayDuration = Mathf.Max(0f, winDisplayDuration);
        if (topCurtain != null || bottomCurtain != null)
        {
            displayDuration = Mathf.Max(displayDuration, curtainDuration);
        }

        if (displayDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(displayDuration);
        }

        if (commandExit != null)
        {
            yield return commandExit;
        }

        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }

        if (victoryBackgroundDimImage != null)
        {
            dimInTween?.Kill();
            Tween dimOutTween = victoryBackgroundDimImage
                .DOFade(0f, Mathf.Max(0.01f, victoryBackgroundDimDuration))
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
            yield return dimOutTween.WaitForCompletion();
            victoryBackgroundDimImage.gameObject.SetActive(false);
        }
    }

    private float GetCurtainExitDistance(RectTransform curtain, bool exitsUpward)
    {
        return GetCurtainOffscreenDistance(curtain, exitsUpward, curtainExitDistance);
    }

    private float GetCurtainOffscreenDistance(
        RectTransform curtain,
        bool exitsUpward,
        float configuredOffset)
    {
        float configuredDistance = Mathf.Abs(configuredOffset);
        Canvas canvas = curtain.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
        RectTransform parentRect = curtain.parent as RectTransform;
        if (canvasRect == null || parentRect == null)
        {
            return configuredDistance > 0f
                ? configuredDistance
                : Mathf.Max(1f, curtain.rect.height);
        }

        Bounds curtainBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parentRect, curtain);
        Vector3 canvasEdgeWorld = canvasRect.TransformPoint(new Vector3(
            canvasRect.rect.center.x,
            exitsUpward ? canvasRect.rect.yMax : canvasRect.rect.yMin,
            0f));
        float canvasEdgeInParent = parentRect.InverseTransformPoint(canvasEdgeWorld).y;
        float automaticDistance = exitsUpward
            ? canvasEdgeInParent - curtainBounds.min.y + 1f
            : curtainBounds.max.y - canvasEdgeInParent + 1f;

        return Mathf.Max(configuredDistance, automaticDistance);
    }

    private void PlayVictoryBgm()
    {
        if (battleBgmSource == null)
        {
            return;
        }

        battleBgmSource.Stop();
        if (victoryBgmClip == null)
        {
            return;
        }

        battleBgmSource.clip = victoryBgmClip;
        battleBgmSource.loop = false;
        battleBgmSource.volume = Mathf.Clamp01(victoryBgmVolume);
        battleBgmSource.Play();
    }

    private IEnumerator DefeatRoutine()
    {
        state = BattleState.Defeat;
        inputLocked = true;
        ClearDefendingPartyMembers();
        SetActiveStatusSlot(-1);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.victoryStoryFlag = "";
        }

        SetCommandUI(false);
        SetSkillPanel(false);
        SetMessagePanel(true);

        yield return ShowBattleMessageAndWaitForConfirm("모두 쓰러졌다...");

        // 중요:
        // 여기서 MessagePanel을 먼저 끄지 않는다.
        // "모두 쓰러졌다..."가 보이는 상태 그대로 페이드 아웃한다.
        yield return FadeOutRoutine(true);

        // 여기부터는 완전 암전 상태.
        // 이제 기존 전투 UI를 꺼도 화면상으로는 안 보인다.
        SetMessagePanel(false);
        SetCommandUI(false);
        if (enemyImage != null)
        {
            enemyImage.gameObject.SetActive(false);
        }
        // 게임오버 패널을 암전 상태에서 미리 켜둔다.
        ShowGameOverPanel();

        // 검은 화면에서 게임오버 패널로 페이드 인
        yield return FadeInRoutine();

        // 방금 누른 C가 바로 선택 입력으로 들어가지 않게 방지
        yield return WaitUntilConfirmReleased();

        inputLocked = false;
    }

    private void ShowGameOverPanel()
    {
        state = BattleState.GameOver;
        gameOverSelectedIndex = 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        UpdateGameOverSelector();
    }
    private IEnumerator FadeOutRoutine(bool fadeBattleBgm = false)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float timer = 0f;

        float startVolume = 0f;

        if (fadeBattleBgm && battleBgmSource != null)
        {
            startVolume = battleBgmSource.volume;
        }

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            color.a = t;
            fadeImage.color = color;

            if (fadeBattleBgm && battleBgmSource != null)
            {
                battleBgmSource.volume = Mathf.Lerp(startVolume, 0f, t);
            }

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        if (fadeBattleBgm && battleBgmSource != null)
        {
            battleBgmSource.Stop();
            battleBgmSource.volume = startVolume;
        }
    }

    private IEnumerator FadeInRoutine(float durationOverride = -1f)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        float timer = 0f;
        float duration = durationOverride >= 0f
            ? durationOverride
            : fadeDuration;

        if (duration <= 0f)
        {
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(false);
            yield break;
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            color.a = 1f - t;
            fadeImage.color = color;

            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }
    private void ResetFadePanel()
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        fadeImage.gameObject.SetActive(false);
    }

    private int GetPartyMemberBattleDefense(int partyMemberIndex)
    {
        if (GameManager.Instance == null)
        {
            return 0;
        }

        int baseDefense = GameManager.Instance.GetEffectiveDefense();
        return defendingPartyMemberIndices.Contains(partyMemberIndex)
            ? baseDefense * 2
            : baseDefense;
    }

    private void SetPartyMemberDefending(int partyMemberIndex, bool isDefending)
    {
        if (isDefending)
        {
            defendingPartyMemberIndices.Add(partyMemberIndex);
        }
        else
        {
            defendingPartyMemberIndices.Remove(partyMemberIndex);
        }
    }

    private void ClearDefendingPartyMembers()
    {
        defendingPartyMemberIndices.Clear();
    }

    private int CalculatePhysicalDamage(int attackerAttack, int attackerLuck, int defenderDefense, out bool isCritical)
    {
        int damage = Mathf.Max(1, attackerAttack - defenderDefense);

        int criticalChance = Mathf.Clamp(5 + attackerLuck, 0, 30);
        isCritical = Random.Range(0, 100) < criticalChance;

        if (isCritical)
        {
            damage *= 2;
        }

        return damage;
    }

    private string AddExpAndBuildLevelUpMessage(int amount)
    {
        GameManager.Instance.exp += amount;

        string message = "";

        while (GameManager.Instance.exp >= GetRequiredExp(GameManager.Instance.level))
        {
            int requiredExp = GetRequiredExp(GameManager.Instance.level);
            GameManager.Instance.exp -= requiredExp;

            LevelUp();

            message += $"레벨이 {GameManager.Instance.level}이 되었다!";

            if (GameManager.Instance.level == 2)
            {
                GameManager.Instance.LearnSkill(GameManager.ESPThunderAlphaSkillId);
                message += "\nESP 썬더 α를 배웠다!";
            }
        }

        return message;
    }

    private int GetRequiredExp(int level)
    {
        return GameManager.GetRequiredExpForLevel(level);
    }

    private void LevelUp()
    {
        GameManager.Instance.level++;
        GameManager.Instance.ApplyHelloLevelUpGrowth();

        RefreshPlayerStatusUI();
    }

    private void RefreshPlayerStatusUI()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        CacheStatusSlotBasePositions();
        if (characterStatusSlotUIs != null &&
            characterStatusSlotUIs.Length > 0 &&
            characterStatusSlotUIs[0] != null)
        {
            characterStatusSlotUIs[0].Refresh(GameManager.Instance);
        }
    }

    private void CacheStatusSlotBasePositions()
    {
        if (characterStatusSlots == null)
        {
            statusSlotBasePositions = null;
            characterStatusSlotUIs = null;
            return;
        }

        if (statusSlotBasePositions != null &&
            statusSlotBasePositions.Length == characterStatusSlots.Length &&
            characterStatusSlotUIs != null &&
            characterStatusSlotUIs.Length == characterStatusSlots.Length)
        {
            return;
        }

        statusSlotBasePositions = new Vector2[characterStatusSlots.Length];
        characterStatusSlotUIs = new CharacterStatusSlotUI[characterStatusSlots.Length];
        for (int i = 0; i < characterStatusSlots.Length; i++)
        {
            if (characterStatusSlots[i] != null)
            {
                statusSlotBasePositions[i] = characterStatusSlots[i].anchoredPosition;
                characterStatusSlotUIs[i] = characterStatusSlots[i].GetComponent<CharacterStatusSlotUI>();
            }
        }
    }

    private void PrepareStatusSlotsForEntry()
    {
        if (characterStatusSlots == null || statusSlotBasePositions == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < characterStatusSlots.Length && i < statusSlotBasePositions.Length; i++)
        {
            if (characterStatusSlots[i] == null)
            {
                continue;
            }

            Vector2 position = statusSlotBasePositions[i];
            position.y -= GetStatusSlotEntryOffset(characterStatusSlots[i]);
            characterStatusSlots[i].anchoredPosition = position;
        }
    }

    private float GetStatusSlotEntryOffset(RectTransform slot)
    {
        float configuredOffset = Mathf.Abs(statusSlotEntryOffsetY);
        Canvas canvas = slot.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
        RectTransform parentRect = slot.parent as RectTransform;
        if (canvasRect == null || parentRect == null)
        {
            return configuredOffset;
        }

        Vector3[] slotCorners = new Vector3[4];
        slot.GetWorldCorners(slotCorners);

        float slotTopInParent = float.MinValue;
        for (int i = 0; i < slotCorners.Length; i++)
        {
            slotTopInParent = Mathf.Max(slotTopInParent, parentRect.InverseTransformPoint(slotCorners[i]).y);
        }

        Vector3 canvasBottomWorld = canvasRect.TransformPoint(
            new Vector3(canvasRect.rect.center.x, canvasRect.rect.yMin, 0f));
        float canvasBottomInParent = parentRect.InverseTransformPoint(canvasBottomWorld).y;
        float offscreenOffset = slotTopInParent - canvasBottomInParent + 1f;

        return Mathf.Max(configuredOffset, offscreenOffset);
    }

    private void CacheCommandUIEntryState()
    {
        if (battleCommandUI == null)
        {
            battleCommandUIRect = null;
            commandSelector = null;
            return;
        }

        battleCommandUIRect = battleCommandUI.GetComponent<RectTransform>();
        commandSelector = battleCommandUI.GetComponent<CommandSelector>();
        if (battleCommandUIRect != null)
        {
            battleCommandUIBasePosition = battleCommandUIRect.anchoredPosition;
        }
    }

    private void CacheMessagePanelState()
    {
        messagePanelRect = messagePanel != null
            ? messagePanel.GetComponent<RectTransform>()
            : null;
        if (messagePanelRect != null)
        {
            messagePanelBasePosition = messagePanelRect.anchoredPosition;
        }
    }

    private void PrepareCommandUIForEntry()
    {
        if (battleCommandUIRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        battleCommandUIRect.anchoredPosition = GetCommandUIHiddenPosition();
    }

    private Vector2 GetCommandUIHiddenPosition()
    {
        Vector2 position = battleCommandUIBasePosition;
        position.y += GetCommandUIEntryOffset();
        return position;
    }

    private float GetCommandUIEntryOffset()
    {
        float configuredOffset = Mathf.Abs(commandUIEntryOffsetY);
        Canvas canvas = battleCommandUIRect.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
        RectTransform parentRect = battleCommandUIRect.parent as RectTransform;
        if (canvasRect == null || parentRect == null)
        {
            return configuredOffset;
        }

        Bounds commandBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            parentRect,
            battleCommandUIRect);
        Vector3 canvasTopWorld = canvasRect.TransformPoint(
            new Vector3(canvasRect.rect.center.x, canvasRect.rect.yMax, 0f));
        float canvasTopInParent = parentRect.InverseTransformPoint(canvasTopWorld).y;
        float offscreenOffset = canvasTopInParent - commandBounds.min.y + 1f;

        return Mathf.Max(configuredOffset, offscreenOffset);
    }

    private IEnumerator ShowBattleCommandUI()
    {
        SetCommandSelectorInputEnabled(false);

        if (battleCommandUIRect == null)
        {
            SetCommandUI(true);
            yield break;
        }

        bool wasActive = battleCommandUI != null && battleCommandUI.activeSelf;
        SetCommandUI(true);
        battleCommandUIRect.DOKill();

        Vector2 hiddenPosition = GetCommandUIHiddenPosition();
        if (!wasActive)
        {
            battleCommandUIRect.anchoredPosition = hiddenPosition;
        }

        if ((battleCommandUIRect.anchoredPosition - battleCommandUIBasePosition).sqrMagnitude <= 0.01f)
        {
            battleCommandUIRect.anchoredPosition = battleCommandUIBasePosition;
            yield break;
        }

        float duration = Mathf.Max(0f, commandUIEntryDuration);
        if (duration <= 0f)
        {
            battleCommandUIRect.anchoredPosition = battleCommandUIBasePosition;
            yield break;
        }

        Tween entryTween = battleCommandUIRect
            .DOAnchorPos(battleCommandUIBasePosition, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
        yield return entryTween.WaitForCompletion();

        battleCommandUIRect.anchoredPosition = battleCommandUIBasePosition;
    }

    private IEnumerator HideBattleCommandUI()
    {
        SetCommandSelectorInputEnabled(false);

        if (battleCommandUIRect == null)
        {
            SetCommandUI(false);
            yield break;
        }

        Vector2 hiddenPosition = GetCommandUIHiddenPosition();
        if (battleCommandUI == null || !battleCommandUI.activeSelf)
        {
            battleCommandUIRect.DOKill();
            battleCommandUIRect.anchoredPosition = hiddenPosition;
            yield break;
        }

        battleCommandUIRect.DOKill();
        float duration = Mathf.Max(0f, commandUIEntryDuration);
        if (duration > 0f &&
            (battleCommandUIRect.anchoredPosition - hiddenPosition).sqrMagnitude > 0.01f)
        {
            Tween exitTween = battleCommandUIRect
                .DOAnchorPos(hiddenPosition, duration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
            yield return exitTween.WaitForCompletion();
        }

        battleCommandUIRect.anchoredPosition = hiddenPosition;
        SetCommandUI(false);
    }

    private IEnumerator PlayStatusSlotEntryRoutine()
    {
        if (characterStatusSlots == null || statusSlotBasePositions == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0f, statusSlotEntryDuration);
        if (duration <= 0f)
        {
            SetStatusSlotsImmediate(-1);
            yield break;
        }

        Vector2[] startPositions = CaptureCurrentStatusSlotPositions();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            for (int i = 0; i < characterStatusSlots.Length && i < statusSlotBasePositions.Length; i++)
            {
                if (characterStatusSlots[i] != null)
                {
                    characterStatusSlots[i].anchoredPosition = Vector2.Lerp(
                        startPositions[i],
                        statusSlotBasePositions[i],
                        t);
                }
            }

            yield return null;
        }

        SetStatusSlotsImmediate(-1);
    }

    private void SetActiveStatusSlot(int activeIndex)
    {
        CacheStatusSlotBasePositions();
        if (characterStatusSlots == null || statusSlotBasePositions == null)
        {
            return;
        }

        if (statusSlotTurnCoroutine != null)
        {
            StopCoroutine(statusSlotTurnCoroutine);
        }

        statusSlotTurnCoroutine = StartCoroutine(MoveStatusSlotsToTurnStateRoutine(activeIndex));
    }

    private void ClearActiveStatusSlotImmediately()
    {
        if (statusSlotTurnCoroutine != null)
        {
            StopCoroutine(statusSlotTurnCoroutine);
            statusSlotTurnCoroutine = null;
        }

        SetStatusSlotsImmediate(-1);
    }

    private IEnumerator MoveStatusSlotsToTurnStateRoutine(int activeIndex)
    {
        float duration = Mathf.Max(0f, activeTurnMoveDuration);
        if (duration <= 0f)
        {
            SetStatusSlotsImmediate(activeIndex);
            statusSlotTurnCoroutine = null;
            yield break;
        }

        Vector2[] startPositions = CaptureCurrentStatusSlotPositions();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            for (int i = 0; i < characterStatusSlots.Length && i < statusSlotBasePositions.Length; i++)
            {
                if (characterStatusSlots[i] == null)
                {
                    continue;
                }

                Vector2 target = statusSlotBasePositions[i];
                if (i == activeIndex)
                {
                    target.y += activeTurnYOffset;
                }

                characterStatusSlots[i].anchoredPosition = Vector2.Lerp(startPositions[i], target, t);
            }

            yield return null;
        }

        SetStatusSlotsImmediate(activeIndex);
        statusSlotTurnCoroutine = null;
    }

    private Vector2[] CaptureCurrentStatusSlotPositions()
    {
        Vector2[] positions = new Vector2[characterStatusSlots.Length];
        for (int i = 0; i < characterStatusSlots.Length; i++)
        {
            positions[i] = characterStatusSlots[i] != null
                ? characterStatusSlots[i].anchoredPosition
                : Vector2.zero;
        }

        return positions;
    }

    private void SetStatusSlotsImmediate(int activeIndex)
    {
        if (characterStatusSlots == null || statusSlotBasePositions == null)
        {
            return;
        }

        for (int i = 0; i < characterStatusSlots.Length && i < statusSlotBasePositions.Length; i++)
        {
            if (characterStatusSlots[i] == null)
            {
                continue;
            }

            Vector2 position = statusSlotBasePositions[i];
            if (i == activeIndex)
            {
                position.y += activeTurnYOffset;
            }

            characterStatusSlots[i].anchoredPosition = position;
        }
    }

    private void SetMessagePanel(bool active)
    {
        if (!active && messageSlidingText != null)
        {
            messageSlidingText.ResetState(true);
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(active);
        }
    }

    private void SetCommandUI(bool active)
    {
        if (battleCommandUI != null)
        {
            battleCommandUI.SetActive(active);
        }
    }

    private void SetCommandSelectorInputEnabled(bool active)
    {
        if (commandSelector == null && battleCommandUI != null)
        {
            commandSelector = battleCommandUI.GetComponent<CommandSelector>();
        }

        if (commandSelector != null)
        {
            commandSelector.enabled = active;
        }
    }

    private void SetSkillPanel(bool active)
    {
        if (skillPanel != null)
        {
            skillPanel.SetActive(active);
        }
    }

    private IEnumerator WaitMessage(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private IEnumerator ShowBattleMessageAndWaitForConfirm(string message)
    {
        yield return ShowBattleMessageRoutine(message);
        yield return WaitForConfirm();
        yield return WaitUntilConfirmReleased();
    }

    private IEnumerator ShowBattleMessageRoutine(string message)
    {
        if (messageSlidingText != null)
        {
            bool typingCompleted = false;
            bool slidingCanSkip = !GameInput.ConfirmHeld;
            bool slidingSkipRequested = false;

            TMP_Text activeText = messageSlidingText.Play(
                message ?? string.Empty,
                1,
                instant: false,
                characterRevealed: null,
                typingComplete: () => typingCompleted = true);

            if (activeText == null)
            {
                yield break;
            }

            while (!typingCompleted || messageSlidingText.IsSliding)
            {
                if (!slidingCanSkip && !GameInput.ConfirmHeld)
                {
                    slidingCanSkip = true;
                }
                else if (slidingCanSkip &&
                         messageSlidingText.IsTyping &&
                         GameInput.ConfirmPressed)
                {
                    slidingSkipRequested = true;
                    messageSlidingText.CompleteTyping();
                }

                yield return null;
            }

            if (slidingSkipRequested)
            {
                yield return WaitUntilConfirmReleased();
            }

            yield break;
        }

        if (messageText == null)
        {
            yield break;
        }

        messageText.text = message ?? string.Empty;
        messageText.maxVisibleCharacters = 0;
        messageText.ForceMeshUpdate(true, true);

        int characterCount = messageText.textInfo != null
            ? messageText.textInfo.characterCount
            : messageText.text.Length;
        float interval = Mathf.Max(0f, messageCharacterInterval);

        if (characterCount == 0 || interval <= 0f)
        {
            messageText.maxVisibleCharacters = int.MaxValue;
            yield break;
        }

        bool canSkip = !GameInput.ConfirmHeld;
        bool skipRequested = false;

        for (int i = 0; i < characterCount; i++)
        {
            messageText.maxVisibleCharacters = i + 1;
            float elapsed = 0f;

            while (elapsed < interval)
            {
                if (!canSkip && !GameInput.ConfirmHeld)
                {
                    canSkip = true;
                }
                else if (canSkip && GameInput.ConfirmPressed)
                {
                    skipRequested = true;
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (skipRequested)
            {
                break;
            }
        }

        messageText.maxVisibleCharacters = int.MaxValue;
        if (skipRequested)
        {
            yield return WaitUntilConfirmReleased();
        }
    }
    private IEnumerator WaitForConfirm()
    {
        // 방금 누른 C가 바로 다음 메시지를 넘기지 않도록,
        // 먼저 C 키에서 손을 뗄 때까지 기다림
        while (GameInput.ConfirmHeld)
        {
            yield return null;
        }

        // 그 다음 새로 C를 누를 때까지 기다림
        while (!GameInput.ConfirmPressed)
        {
            yield return null;
        }
    }

    private IEnumerator WaitUntilConfirmReleased()
    {
        while (GameInput.ConfirmHeld)
        {
            yield return null;
        }
    }
    private void ReturnToField()
    {
        Time.timeScale = 1f;

        string returnScene = GameManager.NormalizeSceneName(defaultReturnSceneName);

        if (GameManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(GameManager.Instance.returnSceneName))
            {
                returnScene = GameManager.NormalizeSceneName(GameManager.Instance.returnSceneName);
            }

            GameManager.Instance.currentBattleEnemy = null;
            GameManager.Instance.currentBattleEnemyId = "";
            GameManager.Instance.fadeInOnTownSceneLoad = returnScene == GameManager.TownSceneName;
        }
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.ResumeBGM();
        }
        SceneManager.LoadScene(returnScene);
    }
    private IEnumerator ReturnToFieldRoutine()
    {
        yield return FadeOutRoutine();

        ReturnToField();
    }
    // 외부 버튼 연결용
    public void OnAttackCommand()
    {
        if (state == BattleState.PlayerCommand && !inputLocked)
        {
            StartCoroutine(PlayerAttackRoutine());
        }
    }

    public void OnDefenseCommand()
    {
        if (state == BattleState.PlayerCommand && !inputLocked)
        {
            StartCoroutine(PlayerDefenseRoutine());
        }
    }

    private IEnumerator PlayerDefenseRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerAction;
        ClearActiveStatusSlotImmediately();

        yield return HideBattleCommandUI();
        SetSkillPanel(false);
        SetMessagePanel(true);

        yield return ShowBattleMessageAndWaitForConfirm(
            $"{GameManager.Instance.playerName}는(은) 몸을 웅크렸다!");

        SetPartyMemberDefending(PlayerPartyMemberIndex, true);
        yield return EnemyTurnRoutine();
    }

    public void OnPhoneCommand()
    {
        if (state != BattleState.PlayerCommand || inputLocked)
        {
            return;
        }

        if (phoneDialogueRunner == null || phoneDadCharacter == null || enemyData == null)
        {
            Debug.LogWarning(
                "BattleManager: Phone Dialogue Runner, Dad Character 또는 EnemyData가 준비되지 않았습니다.");
            StartCoroutine(UnavailableCommandRoutine());
            return;
        }

        if (phoneDialogueRunner.IsRunning)
        {
            return;
        }

        StartCoroutine(PhoneDialogueRoutine());
    }

    private IEnumerator PhoneDialogueRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerAction;
        ClearActiveStatusSlotImmediately();

        yield return HideBattleCommandUI();
        SetSkillPanel(false);
        SetMessagePanel(false);

        DialogueSequence sequence = BuildPhoneDialogueSequence();
        bool dialogueCompleted = false;
        phoneDialogueRunner.Run(sequence, () => dialogueCompleted = true);

        while (!dialogueCompleted)
        {
            yield return null;
        }

        Destroy(sequence);
        yield return OpenCommandSelectRoutine();
    }

    private DialogueSequence BuildPhoneDialogueSequence()
    {
        DialogueSequence sequence = ScriptableObject.CreateInstance<DialogueSequence>();
        sequence.name = "Runtime Battle Phone Dialogue";
        sequence.hideFlags = HideFlags.HideAndDontSave;

        sequence.lines.Add(CreatePhoneBasicLine("전화를 걸었다.", phoneRingSound, true));
        sequence.lines.Add(CreatePhoneDadLine("여보세요? {player}?"));
        sequence.lines.Add(CreatePhoneDadLine($"그래! {enemyData.enemyName}와 만났구나!"));
        sequence.lines.Add(CreatePhoneDadLine(
            $"{enemyData.enemyName}의 체력은 {enemyData.maxHP}, " +
            $"공격력은 {enemyData.attackPower}, 방어력은 {enemyData.defense}, " +
            $"특수공격력은 {enemyData.magicPower}, 특수방어력은 {enemyData.magicDefense}, " +
            $"속도는 {enemyData.speed}이란다!"));

        AddOptionalPhoneDadLine(sequence, enemyData.phoneWeaknessText);
        AddOptionalPhoneDadLine(sequence, enemyData.phoneTriviaText);
        AddOptionalPhoneDadLine(sequence, enemyData.phoneAdviceText);

        sequence.lines.Add(CreatePhoneBasicLine(".....삑!", phoneHangupSound, false));
        return sequence;
    }

    private DialogueLine CreatePhoneBasicLine(
        string text,
        AudioClip lineStartSound,
        bool stopLineStartSoundOnAdvance)
    {
        return new DialogueLine
        {
            text = text,
            showPortrait = false,
            showSpeakerName = false,
            lineStartSound = lineStartSound,
            stopLineStartSoundOnAdvance = stopLineStartSoundOnAdvance
        };
    }

    private DialogueLine CreatePhoneDadLine(string text)
    {
        return new DialogueLine
        {
            speaker = phoneDadCharacter,
            expressionId = "normal",
            text = text,
            showPortrait = true,
            showSpeakerName = true
        };
    }

    private void AddOptionalPhoneDadLine(DialogueSequence sequence, string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            sequence.lines.Add(CreatePhoneDadLine(text));
        }
    }

    public void OnEscapeCommand()
    {
        if (state == BattleState.PlayerCommand && !inputLocked)
        {
            if (enemyData != null && !enemyData.canEscape)
            {
                StartCoroutine(EscapeBlockedRoutine());
                return;
            }

            StartCoroutine(EscapeRoutine());
        }
    }

    public void ShowUnavailableCommandMessage()
    {
        if (state == BattleState.PlayerCommand && !inputLocked)
        {
            StartCoroutine(UnavailableCommandRoutine());
        }
    }

    private IEnumerator UnavailableCommandRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerAction;
        ClearActiveStatusSlotImmediately();

        yield return HideBattleCommandUI();
        SetSkillPanel(false);
        SetMessagePanel(true);

        yield return ShowBattleMessageAndWaitForConfirm("아직 사용할 수 없다.");

        yield return OpenCommandSelectRoutine();
    }

    public void OpenSkillPanel()
    {
        if (state != BattleState.PlayerCommand || inputLocked)
        {
            return;
        }

        StartCoroutine(OpenSkillPanelRoutine());
    }

    private IEnumerator OpenSkillPanelRoutine()
    {
        inputLocked = true;
        yield return HideBattleCommandUI();
        SetMessagePanel(false);
        SetSkillPanel(true);

        if (skillSelector != null)
        {
            skillSelector.SetSkills(GetLearnedBattleSkills());
        }

        inputLocked = false;
    }

    public void CloseSkillPanelAndReturnToCommand()
    {
        if (state != BattleState.PlayerCommand || inputLocked)
        {
            return;
        }

        SetSkillPanel(false);
        OpenCommandSelect();
    }

    public void OnSkillSelected(SkillData skill)
    {
        if (state != BattleState.PlayerCommand || inputLocked || skill == null)
        {
            return;
        }

        StartCoroutine(PlayerSkillRoutine(skill));
    }

    private List<SkillData> GetLearnedBattleSkills()
    {
        List<SkillData> skills = new List<SkillData>();

        AddLearnedSkill(skills, GetESPHealSkill());
        AddLearnedSkill(skills, espThunderAlphaSkill);

        return skills;
    }

    private void AddLearnedSkill(List<SkillData> skills, SkillData skill)
    {
        if (skill == null || GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.HasSkill(skill.skillId))
        {
            skills.Add(skill);
        }
    }

    private SkillData GetESPHealSkill()
    {
        if (espHealSkill != null)
        {
            return espHealSkill;
        }

        if (runtimeESPHealSkill == null)
        {
            runtimeESPHealSkill = ScriptableObject.CreateInstance<SkillData>();
            runtimeESPHealSkill.hideFlags = HideFlags.HideAndDontSave;
            runtimeESPHealSkill.skillId = GameManager.ESPHealAlphaSkillId;
            runtimeESPHealSkill.skillName = "힐링";
            runtimeESPHealSkill.description = "HP를 30 회복한다.";
            runtimeESPHealSkill.learnLevel = 1;
            runtimeESPHealSkill.skillCategory = SkillCategory.Heal;
            runtimeESPHealSkill.skillTier = SkillTier.Alpha;
            runtimeESPHealSkill.mpCost = 2;
            runtimeESPHealSkill.skillType = SkillType.Heal;
            runtimeESPHealSkill.targetType = TargetType.Self;
            runtimeESPHealSkill.elementType = ElementType.None;
            runtimeESPHealSkill.power = 30;
        }

        return runtimeESPHealSkill;
    }

    private IEnumerator PlayerSkillRoutine(SkillData skill)
    {
        inputLocked = true;
        state = BattleState.PlayerAction;
        ClearActiveStatusSlotImmediately();

        yield return HideBattleCommandUI();
        SetSkillPanel(false);
        SetMessagePanel(true);

        if (!GameManager.Instance.HasSkill(skill.skillId))
        {
            yield return ShowBattleMessageAndWaitForConfirm("아직 사용할 수 없는 스킬이다.");

            yield return OpenCommandSelectRoutine();
            yield break;
        }

        if (GameManager.Instance.currentMP < skill.mpCost)
        {
            yield return ShowBattleMessageAndWaitForConfirm("MP가 부족하다.");

            yield return OpenCommandSelectRoutine();
            yield break;
        }

        if (skill.mpCost > 0)
        {
            GameManager.Instance.currentMP -= skill.mpCost;
            RefreshPlayerStatusUI();
        }

        if (skill.skillId == GameManager.ESPHealAlphaSkillId || skill.skillType == SkillType.Heal)
        {
            yield return UseHealSkillRoutine(skill);
            yield break;
        }

        if (skill.skillId == GameManager.ESPThunderAlphaSkillId || skill.elementType == ElementType.Thunder)
        {
            yield return UseThunderSkillRoutine(skill);
            yield break;
        }

        yield return ShowBattleMessageAndWaitForConfirm("아직 사용할 수 없는 스킬이다.");

        yield return OpenCommandSelectRoutine();
    }

    private IEnumerator UseHealSkillRoutine(SkillData skill)
    {
        int healPower = skill.power > 0 ? skill.power : 30;
        int beforeHP = GameManager.Instance.currentHP;
        GameManager.Instance.currentHP = Mathf.Min(
            GameManager.Instance.maxHP,
            GameManager.Instance.currentHP + healPower);

        int healedAmount = GameManager.Instance.currentHP - beforeHP;

        RefreshPlayerStatusUI();

        yield return ShowBattleMessageAndWaitForConfirm(
            $"{skill.skillName}!\nHP를 {healedAmount} 회복했다!");

        yield return EnemyTurnRoutine();
    }

    private IEnumerator UseThunderSkillRoutine(SkillData skill)
    {
        yield return ShowBattleMessageAndWaitForConfirm($"{skill.skillName}!");

        yield return PlaySkillEffectRoutine(skill);

        int damage = Mathf.Max(3, GameManager.Instance.magicAttack * 2 - enemyData.magicDefense);

        enemyCurrentHP -= damage;
        if (enemyCurrentHP < 0)
        {
            enemyCurrentHP = 0;
        }

        yield return ShowBattleMessageAndWaitForConfirm(
            $"{enemyData.enemyName}에게 {damage}의 데미지!");

        if (enemyCurrentHP <= 0)
        {
            yield return VictoryRoutine();
            yield break;
        }

        yield return EnemyTurnRoutine();
    }

    private void HandleGameOverInput()
    {
        if (GameInput.LeftPressed || GameInput.RightPressed)
        {
            gameOverSelectedIndex = 1 - gameOverSelectedIndex;
            UpdateGameOverSelector();
            return;
        }

        if (GameInput.ConfirmPressed)
        {
            if (gameOverSelectedIndex == 0)
            {
                StartCoroutine(ContinueAfterGameOverRoutine());
            }
            else
            {
                StartCoroutine(QuitAfterGameOverRoutine());
            }
        }
    }
    private void UpdateGameOverSelector()
    {
        if (gameOverSelector == null)
        {
            return;
        }

        RectTransform target = gameOverSelectedIndex == 0
            ? gameOverContinueText
            : gameOverQuitText;

        if (target == null)
        {
            return;
        }

        Vector2 targetPos = target.anchoredPosition;
        gameOverSelector.anchoredPosition = new Vector2(targetPos.x - 80f, targetPos.y);
    }
    private IEnumerator ContinueAfterGameOverRoutine()
    {
        inputLocked = true;
        state = BattleState.Returning;

        // GameOverPanel을 끄지 않고, 보이는 상태로 페이드아웃
        yield return FadeOutRoutine();

        // 완전 암전된 뒤에 꺼도 됨
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (SaveSystem.HasSaveData())
        {
            SaveSystem.LoadGame();
        }
        else
        {
            Debug.LogWarning("세이브 파일이 없어 TownScene으로 복귀합니다.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentHP = Mathf.Max(1, GameManager.Instance.maxHP / 2);
                GameManager.Instance.currentMP = Mathf.Max(0, GameManager.Instance.maxMP / 2);
                GameManager.Instance.currentSceneName = GameManager.NormalizeSceneName(defaultReturnSceneName);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentBattleEnemy = null;
            GameManager.Instance.currentBattleEnemyId = "";
            GameManager.Instance.escapedEnemyId = "";
        }

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.ResumeBGM();
        }

        Time.timeScale = 1f;

        string sceneToLoad = GameManager.NormalizeSceneName(defaultReturnSceneName);

        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.currentSceneName))
        {
            sceneToLoad = GameManager.NormalizeSceneName(GameManager.Instance.currentSceneName);
        }

        SceneManager.LoadScene(sceneToLoad);
    }
    private IEnumerator QuitAfterGameOverRoutine()
    {
        inputLocked = true;
        state = BattleState.Returning;

        // GameOverPanel을 유지한 채 페이드아웃
        yield return FadeOutRoutine();

        // 완전 암전 후 끄기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.StopAndDestroy();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentBattleEnemy = null;
            GameManager.Instance.currentBattleEnemyId = "";
            GameManager.Instance.escapedEnemyId = "";
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(GameManager.TitleSceneName);
    }
    private bool IsPartyDefeated()
    {
        return GameManager.Instance.currentHP <= 0;
    }
    private IEnumerator PlaySkillEffectRoutine(SkillData skill)
    {
        if (skill == null)
        {
            yield break;
        }

        GameObject spawnedEffect = null;

        if (skill.effectPrefab != null)
        {
            Transform parent = effectLayer != null ? effectLayer : null;
            spawnedEffect = Instantiate(skill.effectPrefab, parent);

            RectTransform rectTransform = spawnedEffect.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
            }
        }

        if (skill.sfx != null)
        {
            AudioSource.PlayClipAtPoint(skill.sfx, Vector3.zero);
        }

        float waitTime = Mathf.Max(0f, skill.effectDuration);

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        if (spawnedEffect != null)
        {
            Destroy(spawnedEffect);
        }
    }
}
