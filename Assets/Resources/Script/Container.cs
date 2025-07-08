using HurricaneVR.Framework.Core.Grabbers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Container : MonoBehaviour
{
    public List<BlacksmithRecipe> recipes;
    private List<ItemHolder> currentItems = new List<ItemHolder>();
    public GameObject particle;

    [SerializeField] AudioSource audio;
    private void Start()
    {
      currentItems.Add(transform.gameObject.GetComponentInParent<ItemHolder>());
    }

    private void OnTriggerEnter(Collider other)
    {
        ItemHolder item = other.GetComponent<ItemHolder>();

        if (item != null && item.currentInteractor != null && !currentItems.Contains(item))
        {
            Debug.Log(other.GetComponent<ItemHolder>().itemData.itemType);

            audio.Play();

            currentItems.Add(item);
            PrintCurrentItems();

            TryCraft();
        }
    }

    void TryCraft()
    {
        foreach (var item in currentItems)
        {
            if (item.currentInteractor == null)
            {
                Debug.Log("❌ Tidak semua item sedang dipegang. Crafting dibatalkan.");
                return;
            }
        }

        foreach (var recipe in recipes)
        {
            if (RecipeMatches(recipe, currentItems))
            {
                HVRGrabberBase interactorYangPegang = currentItems[currentItems.Count - 1].currentInteractor;

                foreach (var req in recipe.requiredMaterials)
                {
                    int needed = req.quantity;
                    for (int i = currentItems.Count - 1; i >= 0 && needed > 0; i--)
                    {
                        if (currentItems[i].itemData == req.item)
                        {
                            if (audio != null)
                                audio.Play();
                            Destroy(currentItems[i].gameObject);
                            currentItems.RemoveAt(i);
                            needed--;
                        }
                    }
                }
              
                GameObject result = Instantiate(recipe.resultPrefab, transform.position + Vector3.up, Quaternion.identity);
                Destroy(Instantiate(particle,result.transform.position, Quaternion.identity),2f);   
                if (interactorYangPegang != null)
                {
                    XRGrabInteractable grab = result.GetComponent<XRGrabInteractable>();
                    if (grab != null)
                    {
                        Debug.Log("Hello world");
/*                        interactorYangPegang.interactionManager.SelectEnter(interactorYangPegang, grab);
*/                  
                    }
                }

                Debug.Log("✅ Berhasil craft: " + recipe.resultItem.name);
                return;
            }
        }
        currentItems.Clear();
        Debug.Log("❌ Crafting gagal: tidak ada resep yang cocok.");
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
