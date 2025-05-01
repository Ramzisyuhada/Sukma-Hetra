using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<BlacksmithRecipe> recipes;
    private List<ItemHolder> currentItems = new List<ItemHolder>();


    private void Start()
    {
        currentItems.Add(GetComponentInParent<ItemHolder>());
        Debug.Log(currentItems.Count);
    }

    private void OnTriggerEnter(Collider other)
    {
        ItemHolder item = other.GetComponent<ItemHolder>();
        if (item != null && !currentItems.Contains(item))
        {
            currentItems.Add(item);
            PrintCurrentItems();

            TryCraft();
        }
    }

    void TryCraft()
    {
        foreach (var recipe in recipes)
        {
            if (RecipeMatches(recipe, currentItems))
            {
                foreach (var req in recipe.requiredMaterials)
                {
                    int needed = req.quantity;
                    for (int i = currentItems.Count - 1; i >= 0 && needed > 0; i--)
                    {
                        if (currentItems[i].itemData == req.item)
                        {
                            Destroy(currentItems[i].gameObject);
                            currentItems.RemoveAt(i);
                            needed--;
                        }
                    }
                }

                Instantiate(recipe.resultPrefab, transform.position + Vector3.up, Quaternion.identity);
                Debug.Log("Berhasil craft: " + recipe.resultItem.name);
                return; 
            }
        }

        Debug.Log("❌ Crafting gagal: tidak ada resep yang cocok dengan kombinasi item saat ini.");

    }
    void PrintCurrentItems()
    {
        Debug.Log("🧾 Isi container saat ini:");
        foreach (var item in currentItems)
        {
            string name = item != null && item.itemData != null ? item.itemData.name : "Item NULL";
            Debug.Log("- " + name);
        }
    }

    bool RecipeMatches(BlacksmithRecipe recipe, List<ItemHolder> items)
    {
        Dictionary<ItemData, int> itemCounts = new Dictionary<ItemData, int>();

        foreach (var item in items)
        {
            if (item == null || item.itemData == null)
            {
                Debug.LogWarning("ItemHolder atau itemData null ditemukan saat pengecekan resep!");
                continue;
            }

            if (itemCounts.ContainsKey(item.itemData))
                itemCounts[item.itemData]++;
            else
                itemCounts[item.itemData] = 1;
        }

        foreach (var req in recipe.requiredMaterials)
        {
            if (req.item == null)
            {
                Debug.LogWarning("Ada item null di daftar requiredMaterials recipe!");
                return false;
            }

            if (!itemCounts.ContainsKey(req.item) || itemCounts[req.item] < req.quantity)
                return false;
        }

        return true;
    }

}
