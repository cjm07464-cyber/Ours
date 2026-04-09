using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string playerName;
    public int currentHP;
    public int maxHP;
    public int level;
    public int exp;
    public int attack;
    public int defense;

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

        maxHP = 20;
        currentHP = 20;

        level = 1;
        exp = 0;

        attack = 5;
        defense = 2;

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
        data.level = level;
        data.exp = exp;
        data.attack = attack;
        data.defense = defense;

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
        level = data.level;
        exp = data.exp;
        attack = data.attack;
        defense = data.defense;

        currentSceneName = data.currentSceneName;
        playerPosition = new Vector2(data.playerPosX, data.playerPosY);

        introPlayed = data.introPlayed;
        ratBossDefeated = data.ratBossDefeated;
    }
}