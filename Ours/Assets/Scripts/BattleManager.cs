using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public enum BattleState
{
    Start,
    Message,
    CommandSelect,
    TargetSelect,
    EnemyTurn,
    Win,
    Lose
}

public class BattleManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject messagePanel;
    public GameObject commandPanel;
    public GameObject statusPanel;
    public Image enemyImage;
    private PlayerStats player;
    public CommandSelector commandSelector;

    [Header("UI Texts")]
    public TextMeshProUGUI messageText;

    [Header("Enemy Info")]
    [SerializeField] private EnemyData enemyPrefab;  // <- 인스펙터에 할당 가능!
    private EnemyData currentEnemy;
    [SerializeField] private TextMeshProUGUI statusNameText;
    [SerializeField] private TextMeshProUGUI statusHPText;
    [SerializeField] private TextMeshProUGUI statusMPText;
    private BattleState state;
    public BlinkEffect enemyBlinkEffect;

    void Start()
    {
        // 상태창 꺼두기
        commandPanel.SetActive(false);
        statusPanel.SetActive(false);

        // ✅ enemyPrefab이 ScriptableObject인 경우, 복사 생성
        if (enemyPrefab != null)
        {
            currentEnemy = Instantiate(enemyPrefab); // 복사본
            currentEnemy.currentHP = currentEnemy.maxHP;

            if (currentEnemy.enemySprite != null)
            {
                enemyImage.sprite = currentEnemy.enemySprite;
                enemyImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Enemy Sprite가 null입니다!");
            }

            messageText.text = $"{currentEnemy.enemyName}가 나타났다!";
        }
        else
        {
            Debug.LogWarning("EnemyPrefab이 설정되지 않았습니다!");
        }

        // 이후 플레이어 UI 상태 업데이트 코드 계속

        // ✅ 2. 플레이어 상태 패널 표시
        var party = PlayerManager.Instance.partyMembers;

        if (party.Count > 0)
        {
            player = party[0]; // 첫 번째 플레이어

            statusNameText.text = player.playerName;
            statusHPText.text = $"{player.currentHP} / {player.maxHP}";
            statusMPText.text = $"{player.currentMP} / {player.maxMP}";
        }
        else
        {
            Debug.LogWarning("PlayerManager에 파티 멤버가 없습니다!");
        }

        // ✅ 상태 설정
        state = BattleState.Message;
    }

    void Update()
    {
        if (state == BattleState.Message)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                messagePanel.SetActive(false);      // 메세지창 숨기고
                commandPanel.SetActive(true);       // 커맨드창 보여주기
                statusPanel.SetActive(true);        // 상태창 보여주기

                state = BattleState.CommandSelect;  // 다음 상태로 전환
            }
        }
        if (state == BattleState.TargetSelect)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                enemyBlinkEffect.StopBlinking();
                StartCoroutine(PerformAttack()); // 플레이어 공격
                state = BattleState.EnemyTurn; // or Wait 상태
            }
        }

    }
    private IEnumerator PerformAttack()
    {
        // 예시: 데미지 계산
        int damage = Mathf.Max(1, player.attackPower - currentEnemy.defense);
        currentEnemy.currentHP -= damage;

        messagePanel.SetActive(true);
        messageText.text = $"{currentEnemy.enemyName}에게 {damage}의 데미지!";
        yield return new WaitForSeconds(1.5f);

        messagePanel.SetActive(false);

        // 다음 상태 전환 (적 턴 or 다시 플레이어 턴 등)
        state = BattleState.CommandSelect;
        enemyBlinkEffect.StopBlinking();   // 깜빡임 종료

        yield return new WaitForSeconds(0.5f); // 잠깐 쉬고

        commandPanel.SetActive(true);      // 커맨드창 다시 보임
        commandSelector.enabled = true;    // 방향키 및 선택 활성화
    }
    void StartBattleUI()
    {
        messagePanel.SetActive(false);
        commandPanel.SetActive(true);
        statusPanel.SetActive(true);
        enemyImage.gameObject.SetActive(true);

        state = BattleState.CommandSelect;
    }
    void PlayerAttack()
    {
        var player = PlayerManager.Instance.partyMembers[0];

        int damage = Mathf.Max(1, player.attackPower - currentEnemy.defense);
        currentEnemy.currentHP -= damage;

        messageText.text = $"{currentEnemy.enemyName}에게 {damage}의 데미지를 입혔다!";

        // 체크: 적이 죽었는지
        if (currentEnemy.currentHP <= 0)
        {
            messageText.text += "\n적을 물리쳤다!";
            commandPanel.SetActive(false);
            // TODO: 전투 종료 처리
        }
        else
        {
            Invoke(nameof(EnemyAttack), 1.0f); // 반격 타이밍
        }
    }
    void EnemyAttack()
    {
        var player = PlayerManager.Instance.partyMembers[0];

        int damage = Mathf.Max(1, currentEnemy.attackPower - player.defense);
        player.currentHP -= damage;

        messageText.text = $"{currentEnemy.enemyName}의 깨물기!\n{player.playerName}은 {damage}의 데미지를 입었다.";

        // TODO: 플레이어 사망 체크
    }
    public IEnumerator SelectEnemyTarget()
    {
        // 1. 깜빡임 시작
        enemyBlinkEffect.StartBlinking();

        // 2. 대상 지정 대기 (Z 누를 때까지)
        while (!Input.GetKeyDown(KeyCode.Z))
            yield return null;

        // 3. 깜빡임 중지
        enemyBlinkEffect.StopBlinking();

        // 4. 공격 실행
        PerformAttack();

        // 5. 적 반격 타이머 (예: 1초)
        yield return new WaitForSeconds(1f);

        EnemyTurn(); // 적 턴

        // 6. UI 복귀 (둘 다 끝났을 때만!)
        commandPanel.SetActive(true);
        commandSelector.enabled = true;
    }

    public void EnemyTurn()
    {
        // 1. 로그 출력 (임시 확인용)
        Debug.Log("적의 차례입니다!");

        // 2. 적의 공격력 가져오기
        int damage = Mathf.Max(1, currentEnemy.attackPower - player.defense);

        // 3. 플레이어에게 데미지 적용
        player.currentHP -= damage;

        // 4. 데미지 로그
        Debug.Log($"{currentEnemy.enemyName}의 공격! {damage}의 피해!");

        // 5. 플레이어 체력 UI 갱신
        statusHPText.text = $"{player.currentHP} / {player.maxHP}";

        // 6. 플레이어 사망 체크
        if (player.currentHP <= 0)
        {
            messageText.text = $"{player.playerName}가 쓰러졌다!";
            // 이후 GameOver 처리 등 가능
        }
    }
}