using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private const string ResourcePath = "Items";

    private static Dictionary<string, ItemData> itemById;

    public static ItemData GetItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        EnsureCache();
        itemById.TryGetValue(itemId, out ItemData item);
        return item;
    }

    private static void EnsureCache()
    {
        if (itemById != null)
        {
            return;
        }

        itemById = new Dictionary<string, ItemData>();
        ItemData[] items = Resources.LoadAll<ItemData>(ResourcePath);

        foreach (ItemData item in items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.itemId))
            {
                continue;
            }

            if (itemById.ContainsKey(item.itemId))
            {
                Debug.LogWarning($"ItemDatabase: 중복 itemId가 있습니다: {item.itemId}");
                continue;
            }

            itemById.Add(item.itemId, item);
        }
    }
}
