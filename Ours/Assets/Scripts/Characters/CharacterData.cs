using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Front,
    Back,
    Right,
    Left
}

[System.Serializable]
public class PortraitEntry
{
    public string expressionId;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public string displayName;

    [Header("Field Sprites")]
    public Sprite frontSprite;
    public Sprite backSprite;
    public Sprite rightSprite;
    public Sprite leftSprite;

    [Header("Portrait")]
    public Color portraitBackgroundColor = Color.white;
    public Color portraitGradientColor = Color.white;
    public AudioClip dialogueTypeSound;
    public List<PortraitEntry> portraits = new List<PortraitEntry>();

    public Sprite GetPortrait(string expressionId)
    {
        if (portraits == null || portraits.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(expressionId))
        {
            PortraitEntry match = portraits.Find(entry =>
                entry != null && entry.expressionId == expressionId);

            if (match != null)
            {
                return match.sprite;
            }
        }

        PortraitEntry normal = portraits.Find(entry =>
            entry != null && entry.expressionId == "normal");

        return normal != null ? normal.sprite : portraits[0].sprite;
    }
}
