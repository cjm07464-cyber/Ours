using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string playerName; // 플레이어 이름

    public int currentHP;   // 현재 HP
    public int maxHP;       // 최대 HP

    public int level;       // 현재 레벨
    public int exp;         // 현재 경험치

    public int attack;      // 공격력
    public int defense;     // 방어력

    public string currentSceneName;     // 진행복구용 현재씬이름

    public float playerPosX;            // 그리고 좌표들
    public float playerPosY;

    public bool introPlayed;            // 인트로 플래그 여부
    public bool ratBossDefeated;        // 래트킹 처치 플래그 여부
}