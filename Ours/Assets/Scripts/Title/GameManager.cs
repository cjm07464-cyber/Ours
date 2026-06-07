using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public EnemyData currentBattleEnemy;     // currentBattleEnemy = 이번 전투에서 싸울 적
    public string returnSceneName;          //returnSceneName = 전투 끝나고 돌아갈 씬
    public Vector2 returnPlayerPosition;    //returnPlayerPosition = 전투 끝나고 돌아갈 위치

    [Header("Escape Runtime Data")]
    public string currentBattleEnemyId; // currentBattleEnemyId = 이번 전투에서 싸울 적의 ID (EnemyData에서 가져옴)
    public string escapedEnemyId;   // escapedEnemyId = 도망친뒤의 적의 ID (EnemyData에서 가져옴)

    public string playerName;
    public int level;

    public int currentHP;
    public int currentMP;
    public int maxHP;
    public int maxMP;

    public int attack;
    public int magicAttack;
    public int defense;
    public int magicDefense;

    public int speed;
    public int luck;

    public int exp;
    public int gold;


    public string currentSceneName;
    public Vector2 playerPosition;

    public bool introPlayed;
    public bool ratBossDefeated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void StartNewGame(string newName)
    {
        playerName = newName;

        currentHP = 24;
        maxHP = 24;

        currentMP = 10;
        maxMP = 10;

        level = 1;
        exp = 0;

        attack = 7;
        defense = 3;

        magicAttack = 4;
        magicDefense = 2;

        speed = 5;
        luck = 3;

        gold = 0;

        currentSceneName = "MainScene";
        playerPosition = Vector2.zero;

        introPlayed = false;
        ratBossDefeated = false;
    }

    public SaveData GetSaveData()
    {
        SaveData data = new SaveData();

        data.playerName = playerName;
        data.currentHP = currentHP;
        data.maxHP = maxHP;
        data.currentMP = currentMP;
        data.maxMP = maxMP;
        data.level = level;
        data.exp = exp;
        data.attack = attack;
        data.defense = defense;
        data.magicAttack = magicAttack;
        data.magicDefense = magicDefense;
        data.speed = speed;
        data.luck = luck;
        data.gold = gold;

        data.currentSceneName = currentSceneName;

        data.playerPosX = playerPosition.x;
        data.playerPosY = playerPosition.y;

        data.introPlayed = introPlayed;
        data.ratBossDefeated = ratBossDefeated;

        return data;
    }
    public void LoadFromSaveData(SaveData data)
    {
        playerName = data.playerName;
        currentHP = data.currentHP;
        maxHP = data.maxHP;
        currentMP = data.currentMP;
        maxMP = data.maxMP;
        level = data.level;
        exp = data.exp;
        attack = data.attack;
        defense = data.defense;
        magicAttack = data.magicAttack;
        magicDefense = data.magicDefense;
        speed = data.speed;
        luck = data.luck;
        gold = data.gold;

        currentSceneName = data.currentSceneName;
        playerPosition = new Vector2(data.playerPosX, data.playerPosY);

        introPlayed = data.introPlayed;
        ratBossDefeated = data.ratBossDefeated;
    }


}