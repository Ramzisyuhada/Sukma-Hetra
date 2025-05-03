using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    public List<ItemData> allItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (allItems == null || allItems.Count == 0)
        {
            ItemData[] loaded = Resources.LoadAll<ItemData>("Script/ItemType");
            allItems = new List<ItemData>(loaded);
        }
    }

    public ItemData GetByType(JenisBarangEnum type)
    {
        foreach (var item in allItems)
        {
            if (item.itemType == type)
                return item;
        }

        Debug.LogWarning("Item dengan type " + type + " tidak ditemukan!");
        return null;
    }
}
