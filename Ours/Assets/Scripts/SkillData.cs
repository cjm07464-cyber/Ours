using UnityEngine;

public enum SkillType
{
    MagicAttack,
    Heal
}

public enum TargetType
{
    Self,
    SingleEnemy
}

public enum ElementType
{
    None,
    Thunder
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Battle/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic")]
    public string skillId;
    public string skillName;
    [TextArea]
    public string description;
    public int learnLevel;

    [Header("Cost / Type")]
    public int mpCost;
    public SkillType skillType;
    public TargetType targetType;
    public ElementType elementType;

    [Header("Effect")]
    public int power;
    [Header("Visual / Sound")]
    public GameObject effectPrefab;
    public AudioClip sfx;
    public float effectDuration = 1.0f;
}
