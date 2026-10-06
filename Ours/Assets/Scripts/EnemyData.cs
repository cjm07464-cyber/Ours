using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic")]
    public string enemyName;
    public int level;
    public Sprite enemySprite;

    [Header("HP / MP")]
    public int maxHP;
    public int maxMP;

    [Header("Stats")]
    public int attackPower;
    public int defense;
    public int magicPower;
    public int magicDefense;
    public int speed;
    public int luck;

    [Header("Reward")]
    public int expReward;
    public int goldReward;

    [Header("Phone Information")]
    [TextArea(2, 4)] public string phoneWeaknessText;
    [TextArea(2, 4)] public string phoneTriviaText;
    [TextArea(2, 4)] public string phoneAdviceText;

    [Header("Escape")]
    public bool canEscape = true;
    [TextArea(2, 4)] public string escapeBlockMessage;
}
