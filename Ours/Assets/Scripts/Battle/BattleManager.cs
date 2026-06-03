using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject commandPanel;
    [SerializeField] private GameObject statusPanel;

    [Header("UI Images")]
    [SerializeField] private Image enemyImage;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI commandText;
    [SerializeField] private TextMeshProUGUI statusNameText;
    [SerializeField] private TextMeshProUGUI statusHPText;
    [SerializeField] private TextMeshProUGUI statusMPText;
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private RectTransform gameOverSelector;
    [SerializeField] private RectTransform gameOverContinueText;
    [SerializeField] private RectTransform gameOverQuitText;
    [Header("Enemy")]
    [SerializeField] private EnemyData testEnemyData;
    private EnemyData enemyData;

    [Header("Battle Settings")]
    [SerializeField] private string defaultReturnSceneName = "MainScene";
    [SerializeField] private float messageWaitSeconds = 1.0f;
    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Audio")]
    [SerializeField] private AudioSource battleBgmSource;
    private BattleState state;

    private int enemyCurrentHP;
    private int enemyCurrentMP;

    private int selectedCommandIndex;
    private int gameOverSelectedIndex = 0; // 0 = 다시 일어서기, 1 = 그만하기
    private readonly string[] commands = { "공격", "PK회복", "도망" };

    private bool inputLocked;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("BattleManager: GameManager가 없습니다.");
            enabled = false;
            return;
        }

        ResolveEnemyData();

        if (enemyData == null)
        {
            Debug.LogError("BattleManager: 사용할 EnemyData가 없습니다.");
            enabled = false;
            return;
        }

        InitializeEnemy();
        RefreshPlayerStatusUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(false);
        }

        SetCommandPanel(false);
        SetStatusPanel(false);
        SetMessagePanel(true);

        state = BattleState.StartMessage;
        messageText.text = $"{enemyData.enemyName}가 나타났다!";
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

            case BattleState.PlayerCommand:
                if (commandText != null)
                {
                    HandleCommandInput();
                }
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
        if (!Input.GetKeyDown(KeyCode.Z))
        {
            return;
        }

        SetMessagePanel(false);
        SetStatusPanel(true);

        if (IsEnemyFaster())
        {
            StartCoroutine(EnemyTurnRoutine());
        }
        else
        {
            OpenCommandSelect();
        }
    }

    private bool IsEnemyFaster()
    {
        return enemyData.speed > GameManager.Instance.speed;
    }

    private void OpenCommandSelect()
    {
        state = BattleState.PlayerCommand;
        selectedCommandIndex = 0;

        SetCommandPanel(true);
        SetMessagePanel(false);
        UpdateCommandText();
        RefreshPlayerStatusUI();
    }

    private void HandleCommandInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedCommandIndex--;
            if (selectedCommandIndex < 0)
            {
                selectedCommandIndex = commands.Length - 1;
            }

            UpdateCommandText();
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedCommandIndex++;
            if (selectedCommandIndex >= commands.Length)
            {
                selectedCommandIndex = 0;
            }

            UpdateCommandText();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ExecuteSelectedCommand();
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            selectedCommandIndex = 0;
            UpdateCommandText();
        }
    }

    private void UpdateCommandText()
    {
        if (commandText == null)
        {
            return;
        }

        string result = "";

        for (int i = 0; i < commands.Length; i++)
        {
            string cursor = i == selectedCommandIndex ? "> " : "  ";
            result += cursor + commands[i];

            if (commands[i] == "PK회복" && !CanUsePKHeal())
            {
                result += " (Lv2)";
            }

            if (i < commands.Length - 1)
            {
                result += "\n";
            }
        }

        commandText.text = result;
    }

    private void ExecuteSelectedCommand()
    {
        switch (selectedCommandIndex)
        {
            case 0:
                StartCoroutine(PlayerAttackRoutine());
                break;

            case 1:
                StartCoroutine(PlayerHealRoutine());
                break;

            case 2:
                StartCoroutine(EscapeRoutine());
                break;
        }
    }

    private IEnumerator PlayerAttackRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerAction;

        SetCommandPanel(false);
        SetMessagePanel(true);

        int damage = CalculatePhysicalDamage(
            GameManager.Instance.attack,
            GameManager.Instance.luck,
            enemyData.defense,
            out bool isCritical);

        enemyCurrentHP -= damage;
        if (enemyCurrentHP < 0)
        {
            enemyCurrentHP = 0;
        }

        messageText.text = isCritical
            ? $"회심의 일격!\n{enemyData.enemyName}에게 {damage}의 데미지!"
            : $"{enemyData.enemyName}에게 {damage}의 데미지!";

        yield return WaitForConfirm();

        if (enemyCurrentHP <= 0)
        {
            yield return VictoryRoutine();
            yield break;
        }

        yield return EnemyTurnRoutine();
    }

    private IEnumerator PlayerHealRoutine()
    {
        inputLocked = true;
        state = BattleState.PlayerAction;

        SetCommandPanel(false);
        SetMessagePanel(true);

        if (!CanUsePKHeal())
        {
            messageText.text = "아직 PK회복을 사용할 수 없다.";
            yield return WaitForConfirm();

            inputLocked = false;
            OpenCommandSelect();
            yield break;
        }

        int beforeHP = GameManager.Instance.currentHP;
        GameManager.Instance.currentHP = Mathf.Min(
            GameManager.Instance.maxHP,
            GameManager.Instance.currentHP + 30);

        int healedAmount = GameManager.Instance.currentHP - beforeHP;

        RefreshPlayerStatusUI();

        messageText.text = $"PK회복!\nHP를 {healedAmount} 회복했다!";
        yield return WaitForConfirm();

        yield return EnemyTurnRoutine();
    }

    private IEnumerator EnemyTurnRoutine()
    {
        inputLocked = true;
        state = BattleState.EnemyAction;

        SetCommandPanel(false);
        SetMessagePanel(true);

        int damage = CalculatePhysicalDamage(
            enemyData.attackPower,
            enemyData.luck,
            GameManager.Instance.defense,
            out bool isCritical);

        GameManager.Instance.currentHP -= damage;
        if (GameManager.Instance.currentHP < 0)
        {
            GameManager.Instance.currentHP = 0;
        }

        RefreshPlayerStatusUI();

        messageText.text = isCritical
            ? $"{enemyData.enemyName}의 회심의 공격!\n{GameManager.Instance.playerName}은 {damage}의 데미지를 입었다!"
            : $"{enemyData.enemyName}의 공격!\n{GameManager.Instance.playerName}은 {damage}의 데미지를 입었다!";

        yield return WaitForConfirm();

        if (IsPartyDefeated())
        {
            yield return DefeatRoutine();
            yield break;
        }

        inputLocked = false;
        OpenCommandSelect();
    }

    private IEnumerator EscapeRoutine()
    {
        inputLocked = true;
        state = BattleState.Returning;

        SetCommandPanel(false);
        SetMessagePanel(true);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.escapedEnemyId = GameManager.Instance.currentBattleEnemyId;
        }
        messageText.text = "도망쳤다!";
        yield return WaitForConfirm();
        yield return ReturnToFieldRoutine();
    }

    private IEnumerator VictoryRoutine()
    {
        state = BattleState.Victory;
        inputLocked = true;

        SetCommandPanel(false);
        SetMessagePanel(true);

        int gainedExp = enemyData.expReward;
        int gainedGold = enemyData.goldReward;

        GameManager.Instance.gold += gainedGold;

        string levelUpMessage = AddExpAndBuildLevelUpMessage(gainedExp);

        messageText.text =
            $"{enemyData.enemyName}를 물리쳤다!\n" +
            $"EXP {gainedExp} 획득!\n" +
            $"G {gainedGold} 획득!";

        if (!string.IsNullOrEmpty(levelUpMessage))
        {
            messageText.text += "\n" + levelUpMessage;
        }

        yield return WaitForConfirm();
        yield return ReturnToFieldRoutine();
    }

    private IEnumerator DefeatRoutine()
    {
        state = BattleState.Defeat;
        inputLocked = true;

        SetCommandPanel(false);
        SetStatusPanel(false);
        SetMessagePanel(true);

        messageText.text = "모두 쓰러졌다...";

        // Z를 눌러야 다음으로 진행
        yield return WaitForConfirm();

        // 중요:
        // 여기서 MessagePanel을 먼저 끄지 않는다.
        // "모두 쓰러졌다..."가 보이는 상태 그대로 페이드 아웃한다.
        yield return FadeOutRoutine(true);

        // 여기부터는 완전 암전 상태.
        // 이제 기존 전투 UI를 꺼도 화면상으로는 안 보인다.
        SetMessagePanel(false);
        SetCommandPanel(false);
        SetStatusPanel(false);
        if (enemyImage != null)
        {
            enemyImage.gameObject.SetActive(false);
        }
        // 게임오버 패널을 암전 상태에서 미리 켜둔다.
        ShowGameOverPanel();

        // 검은 화면에서 게임오버 패널로 페이드 인
        yield return FadeInRoutine();

        // 방금 누른 Z가 바로 선택 입력으로 들어가지 않게 방지
        yield return WaitUntilZReleased();

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

    private IEnumerator FadeInRoutine()
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

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

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

    private bool CanUsePKHeal()
    {
        return GameManager.Instance.level >= 2;
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
                message += "\nPK회복을 배웠다!";
            }
        }

        return message;
    }

    private int GetRequiredExp(int level)
    {
        int[] requiredExpTable =
        {
            0,   // index 0 unused
            10,  // Lv1 -> Lv2
            25,  // Lv2 -> Lv3
            45,  // Lv3 -> Lv4
            70,  // Lv4 -> Lv5
            100, // Lv5 -> Lv6
            135, // Lv6 -> Lv7
            175, // Lv7 -> Lv8
            220, // Lv8 -> Lv9
            270  // Lv9 -> Lv10
        };

        if (level > 0 && level < requiredExpTable.Length)
        {
            return requiredExpTable[level];
        }

        return level * level * 5 + level * 10;
    }

    private void LevelUp()
    {
        GameManager.Instance.level++;

        GameManager.Instance.maxHP += 5;
        GameManager.Instance.currentHP = GameManager.Instance.maxHP;

        GameManager.Instance.maxMP += 2;
        GameManager.Instance.currentMP = GameManager.Instance.maxMP;

        GameManager.Instance.attack += 2;
        GameManager.Instance.defense += 1;

        GameManager.Instance.magicAttack += 2;
        GameManager.Instance.magicDefense += 1;

        GameManager.Instance.speed += 1;

        if (GameManager.Instance.level % 2 == 1)
        {
            GameManager.Instance.luck += 1;
        }

        RefreshPlayerStatusUI();
    }

    private void RefreshPlayerStatusUI()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (statusNameText != null)
        {
            statusNameText.text = GameManager.Instance.playerName;
        }

        if (statusHPText != null)
        {
            statusHPText.text = $"{GameManager.Instance.currentHP} / {GameManager.Instance.maxHP}";
        }

        if (statusMPText != null)
        {
            statusMPText.text = $"{GameManager.Instance.currentMP} / {GameManager.Instance.maxMP}";
        }
    }

    private void SetMessagePanel(bool active)
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(active);
        }
    }

    private void SetCommandPanel(bool active)
    {
        if (commandPanel != null)
        {
            commandPanel.SetActive(active);
        }
    }

    private void SetStatusPanel(bool active)
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(active);
        }
    }

    private IEnumerator WaitMessage()
    {
        yield return new WaitForSeconds(messageWaitSeconds);
    }

    private IEnumerator WaitMessage(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    private IEnumerator WaitForConfirm()
    {
        // 방금 누른 Z가 바로 다음 메시지를 넘기지 않도록,
        // 먼저 Z 키에서 손을 뗄 때까지 기다림
        while (Input.GetKey(KeyCode.Z))
        {
            yield return null;
        }

        // 그 다음 새로 Z를 누를 때까지 기다림
        while (!Input.GetKeyDown(KeyCode.Z))
        {
            yield return null;
        }
    }

    private IEnumerator WaitUntilZReleased()
    {
        while (Input.GetKey(KeyCode.Z))
        {
            yield return null;
        }
    }
    private void ReturnToField()
    {
        Time.timeScale = 1f;

        string returnScene = defaultReturnSceneName;

        if (GameManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(GameManager.Instance.returnSceneName))
            {
                returnScene = GameManager.Instance.returnSceneName;
            }

            GameManager.Instance.currentBattleEnemy = null;
            GameManager.Instance.currentBattleEnemyId = "";
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
    // 기존 CommandSelector가 아직 SelectEnemyTarget()를 호출할 수 있으므로 임시 호환용으로 남긴다.
    public IEnumerator SelectEnemyTarget()
    {
        yield return PlayerAttackRoutine();
    }

    // 외부 버튼 연결용
    public void OnAttackCommand()
    {
        if (state == BattleState.PlayerCommand && !inputLocked)
        {
            StartCoroutine(PlayerAttackRoutine());
        }
    }

    public void OnHealCommand()
    {
        if (state == BattleState.PlayerCommand && !inputLocked)
        {
            StartCoroutine(PlayerHealRoutine());
        }
    }

    public void OnEscapeCommand()
    {
        if (state == BattleState.PlayerCommand && !inputLocked)
        {
            StartCoroutine(EscapeRoutine());
        }
    }

    private void HandleGameOverInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            gameOverSelectedIndex = 1 - gameOverSelectedIndex;
            UpdateGameOverSelector();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z))
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
            Debug.LogWarning("세이브 파일이 없어 MainScene으로 복귀합니다.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentHP = Mathf.Max(1, GameManager.Instance.maxHP / 2);
                GameManager.Instance.currentMP = Mathf.Max(0, GameManager.Instance.maxMP / 2);
                GameManager.Instance.currentSceneName = defaultReturnSceneName;
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

        string sceneToLoad = defaultReturnSceneName;

        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.currentSceneName))
        {
            sceneToLoad = GameManager.Instance.currentSceneName;
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
        SceneManager.LoadScene("Title");
    }
    private bool IsPartyDefeated()
    {
        return GameManager.Instance.currentHP <= 0;
    }
}