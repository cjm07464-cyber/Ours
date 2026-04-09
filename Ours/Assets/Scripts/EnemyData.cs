using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;

    public int maxHP;
    public int currentHP; // 실시간 체력

    public int attackPower;
    public int defense;
    public int magicPower;
    public int magicDefense;
    public int speed;
}