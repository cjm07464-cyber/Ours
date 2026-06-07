using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const string PKHealSkillId = "pk_heal";
    public const string PKThunderSkillId = "pk_thunder";

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

    [Header("Skills")]
    public List<string> learnedSkillIds = new List<string>();

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

        learnedSkillIds = new List<string>();
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
        data.learnedSkillIds = learnedSkillIds != null
            ? new List<string>(learnedSkillIds)
            : new List<string>();

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

        learnedSkillIds = data.learnedSkillIds != null
            ? new List<string>(data.learnedSkillIds)
            : new List<string>();

        if (level >= 2)
        {
            LearnSkill(PKHealSkillId);
            LearnSkill(PKThunderSkillId);
        }
    }

    public void LearnSkill(string skillId)
    {
        if (string.IsNullOrEmpty(skillId))
        {
            return;
        }

        if (learnedSkillIds == null)
        {
            learnedSkillIds = new List<string>();
        }

        if (!learnedSkillIds.Contains(skillId))
        {
            learnedSkillIds.Add(skillId);
        }
    }

    public bool HasSkill(string skillId)
    {
        if (string.IsNullOrEmpty(skillId) || learnedSkillIds == null)
        {
            return false;
        }

        return learnedSkillIds.Contains(skillId);
    }
}
