using UnityEngine;

public enum ItemType
{
    Consumable,
    Equipment
}

public enum EquipmentSlot
{
    None,
    Weapon
}

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;

    public ItemType itemType;
    public EquipmentSlot equipmentSlot;

    public int attackBonus;
    public int defenseBonus;

    public bool stackable;
    public int maxStack = 1;
}
