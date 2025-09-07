using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PandaiBesi", menuName = "Recipe/PandaiBesi")]
public class BlacksmithRecipe : ScriptableObject
{
    [Header("Bahan")]
    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();

    [Header("Hasil")]
    public ItemData resultItem;
    public GameObject resultPrefab;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Bersihkan entry kosong & normalisasi kuantitas
        if (requiredMaterials != null)
        {
            requiredMaterials.RemoveAll(r => r == null || r.item == null || r.quantity <= 0);
            foreach (var r in requiredMaterials)
            {
                if (r.quantity < 1) r.quantity = 1;
            }
        }

        if (resultItem == null)
        {
            Debug.LogWarning($"[BlacksmithRecipe] {name} belum punya resultItem.");
        }
        if (resultPrefab == null)
        {
            Debug.LogWarning($"[BlacksmithRecipe] {name} belum punya resultPrefab.");
        }
    }
#endif
}
