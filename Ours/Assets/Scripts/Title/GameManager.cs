using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum StartupSessionState
{
    None,
    ForestCompleted,
    NameChosen
}

public class GameManager : MonoBehaviour
{
    public const string PKHealSkillId = "pk_heal";
    public const string PKThunderSkillId = "pk_thunder";
    public const string TitleSceneName = "TitleScene";
    public const string ForestSceneName = "ForestScene";
    public const string TownSceneName = "TownScene";
    public const string FieldMenuUnlockFlagId = "father_phone_call_done";

    public static GameManager Instance;
    public EnemyData currentBattleEnemy;     // currentBattleEnemy = 이번 전투에서 싸울 적
    public string returnSceneName;          //returnSceneName = 전투 끝나고 돌아갈 씬
    public Vector2 returnPlayerPosition;    //returnPlayerPosition = 전투 끝나고 돌아갈 위치

    [Header("Battle Runtime Data")]
    public string currentBattleEnemyId; // currentBattleEnemyId = 이번 전투에서 싸울 적의 ID (EnemyData에서 가져옴)
    public string escapedEnemyId;   // escapedEnemyId = 도망친뒤의 적의 ID (EnemyData에서 가져옴)
    public string defeatedEnemyId;
    [FormerlySerializedAs("fadeInOnMainSceneLoad")]
    public bool fadeInOnTownSceneLoad;

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
    public Vector2 playerFacingDirection = Vector2.right;
    public StartupSessionState startupSessionState = StartupSessionState.None;
    public string pendingPlayerName = "";
    private bool townOpeningRequested;

    public bool introPlayed;
    public bool ratBossDefeated;

    [Header("Skills")]
    public List<string> learnedSkillIds = new List<string>();

    private List<InventoryEntry> inventoryItems = new List<InventoryEntry>();
    public string equippedWeaponItemId = "";
    private List<string> storyFlags = new List<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("GameManager");
        managerObject.AddComponent<GameManager>();
    }

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

    // 개발용: 0키를 누르면 세이브 파일 삭제
    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (GameInput.DebugZeroPressed)
        {
            SaveSystem.DeleteSaveData();
            Debug.Log("[DEV] 저장 파일 삭제 요청");
        }
#endif
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

        currentSceneName = TownSceneName;
        playerPosition = Vector2.zero;
        playerFacingDirection = Vector2.right;
        currentBattleEnemy = null;
        returnSceneName = "";
        currentBattleEnemyId = "";
        escapedEnemyId = "";
        defeatedEnemyId = "";
        fadeInOnTownSceneLoad = false;

        introPlayed = false;
        ratBossDefeated = false;

        learnedSkillIds = new List<string>();
        inventoryItems = new List<InventoryEntry>();
        equippedWeaponItemId = "";
        storyFlags = new List<string>();
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

        data.currentSceneName = NormalizeSceneName(currentSceneName);

        data.playerPosX = playerPosition.x;
        data.playerPosY = playerPosition.y;
        data.playerFacingDirX = playerFacingDirection.x;
        data.playerFacingDirY = playerFacingDirection.y;

        data.introPlayed = introPlayed;
        data.ratBossDefeated = ratBossDefeated;
        data.learnedSkillIds = learnedSkillIds != null
            ? new List<string>(learnedSkillIds)
            : new List<string>();
        data.inventoryItems = CloneInventory(inventoryItems);
        data.equippedWeaponItemId = equippedWeaponItemId ?? "";
        data.storyFlags = storyFlags != null
            ? new List<string>(storyFlags)
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

        currentSceneName = NormalizeSceneName(data.currentSceneName);
        playerPosition = new Vector2(data.playerPosX, data.playerPosY);
        playerFacingDirection = new Vector2(data.playerFacingDirX, data.playerFacingDirY);
        if (playerFacingDirection.sqrMagnitude < 0.0001f)
        {
            playerFacingDirection = Vector2.down;
        }
        else
        {
            playerFacingDirection.Normalize();
        }

        introPlayed = data.introPlayed;
        ratBossDefeated = data.ratBossDefeated;

        learnedSkillIds = data.learnedSkillIds != null
            ? new List<string>(data.learnedSkillIds)
            : new List<string>();
        inventoryItems = CloneInventory(data.inventoryItems);
        equippedWeaponItemId = data.equippedWeaponItemId ?? "";

        if (string.IsNullOrWhiteSpace(equippedWeaponItemId) || !CanEquipItem(equippedWeaponItemId))
        {
            equippedWeaponItemId = "";
        }
        storyFlags = data.storyFlags != null
            ? new List<string>(data.storyFlags)
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

    public bool HasStoryFlag(string flagId)
    {
        return !string.IsNullOrWhiteSpace(flagId) &&
               storyFlags != null &&
               storyFlags.Contains(flagId);
    }

    public bool IsFieldMenuUnlocked()
    {
        return HasStoryFlag(FieldMenuUnlockFlagId);
    }

    public int GetExpToNextLevel()
    {
        return Mathf.Max(0, GetRequiredExpForLevel(level) - exp);
    }

    public static int GetRequiredExpForLevel(int level)
    {
        int[] requiredExpTable =
        {
            0,   // index 0 unused
            10,  // Lv1 -> Lv2
            25,  // Lv2 -> Lv3
            45,  // Lv3 -> Lv4
            70,  // Lv4 -> Lv5
            100, // Lv5 -> Lv6
            135, // Lv6 -> Lv7
            175, // Lv7 -> Lv8
            220, // Lv8 -> Lv9
            270  // Lv9 -> Lv10
        };

        if (level > 0 && level < requiredExpTable.Length)
        {
            return requiredExpTable[level];
        }

        return level * level * 5 + level * 10;
    }

    public bool SetStoryFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return false;
        }

        if (storyFlags == null)
        {
            storyFlags = new List<string>();
        }

        if (storyFlags.Contains(flagId))
        {
            return false;
        }

        storyFlags.Add(flagId);
        return true;
    }

    public bool ClearStoryFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId) || storyFlags == null)
        {
            return false;
        }

        return storyFlags.Remove(flagId);
    }

    public bool AddItem(string itemId, int count = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || count <= 0)
        {
            return false;
        }

        ItemData item = ItemDatabase.GetItem(itemId);
        if (item == null)
        {
            Debug.LogWarning($"GameManager: 알 수 없는 itemId입니다: {itemId}");
            return false;
        }

        InventoryEntry entry = FindInventoryEntry(itemId);
        if (item.itemType == ItemType.Equipment && !item.stackable)
        {
            if (entry != null && entry.count > 0)
            {
                return false;
            }

            EnsureInventoryList();
            inventoryItems.Add(new InventoryEntry { itemId = itemId, count = 1 });
            return true;
        }

        int maxStack = item.stackable && item.maxStack > 0 ? item.maxStack : int.MaxValue;
        if (entry == null)
        {
            EnsureInventoryList();
            int amountToAdd = Mathf.Min(count, maxStack);
            if (amountToAdd <= 0)
            {
                return false;
            }

            inventoryItems.Add(new InventoryEntry { itemId = itemId, count = amountToAdd });
            return true;
        }

        int newCount = Mathf.Min(entry.count + count, maxStack);
        if (newCount <= entry.count)
        {
            return false;
        }

        entry.count = newCount;
        return true;
    }

    public bool RemoveItem(string itemId, int count = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || count <= 0)
        {
            return false;
        }

        InventoryEntry entry = FindInventoryEntry(itemId);
        if (entry == null || entry.count < count)
        {
            return false;
        }

        entry.count -= count;
        if (entry.count <= 0)
        {
            inventoryItems.Remove(entry);
            if (equippedWeaponItemId == itemId)
            {
                UnequipWeapon();
            }
        }

        return true;
    }

    public bool HasItem(string itemId, int count = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || count <= 0)
        {
            return false;
        }

        return GetItemCount(itemId) >= count;
    }

    public int GetItemCount(string itemId)
    {
        InventoryEntry entry = FindInventoryEntry(itemId);
        return entry != null ? Mathf.Max(0, entry.count) : 0;
    }

    public bool EquipItem(string itemId)
    {
        if (!CanEquipItem(itemId))
        {
            return false;
        }

        equippedWeaponItemId = itemId;
        return true;
    }

    public void UnequipWeapon()
    {
        equippedWeaponItemId = "";
    }

    public ItemData GetEquippedWeapon()
    {
        if (string.IsNullOrWhiteSpace(equippedWeaponItemId))
        {
            return null;
        }

        return ItemDatabase.GetItem(equippedWeaponItemId);
    }

    public int GetEffectiveAttack()
    {
        ItemData weapon = GetEquippedWeapon();
        return attack + (weapon != null ? weapon.attackBonus : 0);
    }

    public int GetEffectiveDefense()
    {
        ItemData weapon = GetEquippedWeapon();
        return defense + (weapon != null ? weapon.defenseBonus : 0);
    }

    private bool CanEquipItem(string itemId)
    {
        if (!HasItem(itemId))
        {
            return false;
        }

        ItemData item = ItemDatabase.GetItem(itemId);
        return item != null &&
               item.itemType == ItemType.Equipment &&
               item.equipmentSlot == EquipmentSlot.Weapon;
    }

    private InventoryEntry FindInventoryEntry(string itemId)
    {
        if (inventoryItems == null || string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return inventoryItems.Find(entry => entry != null && entry.itemId == itemId);
    }

    private void EnsureInventoryList()
    {
        if (inventoryItems == null)
        {
            inventoryItems = new List<InventoryEntry>();
        }
    }

    private List<InventoryEntry> CloneInventory(List<InventoryEntry> source)
    {
        List<InventoryEntry> result = new List<InventoryEntry>();
        if (source == null)
        {
            return result;
        }

        foreach (InventoryEntry entry in source)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.itemId) || entry.count <= 0)
            {
                continue;
            }

            result.Add(new InventoryEntry { itemId = entry.itemId, count = entry.count });
        }

        return result;
    }

    public void MarkForestCompleted()
    {
        startupSessionState = StartupSessionState.ForestCompleted;
    }

    public bool SetPendingPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        pendingPlayerName = name;
        startupSessionState = StartupSessionState.NameChosen;
        return true;
    }

    public void ClearStartupSession()
    {
        startupSessionState = StartupSessionState.None;
        pendingPlayerName = "";
    }

    public void RequestTownOpening()
    {
        townOpeningRequested = true;
    }

    public bool HasTownOpeningRequest()
    {
        return townOpeningRequested;
    }

    public bool ConsumeTownOpeningRequest()
    {
        if (!townOpeningRequested)
        {
            return false;
        }

        townOpeningRequested = false;
        return true;
    }

    public static string NormalizeSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return sceneName;
        }

        if (sceneName == "Title" || sceneName == "BootScene")
        {
            return TitleSceneName;
        }

        if (sceneName == "MainScene")
        {
            return TownSceneName;
        }

        return sceneName;
    }
}
