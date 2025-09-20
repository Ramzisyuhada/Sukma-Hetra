using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PandaiBesi", menuName = "Recipe/PandaiBesi")]
public class BlacksmithRecipe : ScriptableObject
{
    // === Opsi kompatibel: MaterialRequirement (dipakai oleh requiredMaterials) ===
    [Serializable]
    public class MaterialRequirement
    {
        public ItemData item;
        [Min(1)] public int quantity = 1;

        [Header("Opsional: minimal tempa per unit bahan")]
        [Min(0)] public int minForgeHits = 0;
    }

    // (Opsional) Versi alternatif yang tadinya kamu tulis.
    // Tidak dipakai, tapi boleh dibiarkan atau dihapus jika bingung.
    [Serializable]
    public class MaterialStack
    {
        public ItemData item;
        [Min(1)] public int quantity = 1;

        [Header("Opsional: minimal tempa per unit bahan")]
        [Min(0)] public int minForgeHits = 0;
    }

    [Header("Bahan")]
    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();

    [Header("Hasil")]
    public ItemData resultItem;
    public GameObject resultPrefab;

    [Header("Proses Grid")]
    [Tooltip("Berapa detik bahan harus berada di GridStone sampai jadi (per objek).")]
    [Min(0.1f)] public float grindSeconds = 1.5f;

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
                if (r.minForgeHits < 0) r.minForgeHits = 0;
            }
        }

        if (resultItem == null)
            Debug.LogWarning($"[BlacksmithRecipe] {name} belum punya resultItem.");

        if (resultPrefab == null)
            Debug.LogWarning($"[BlacksmithRecipe] {name} belum punya resultPrefab.");
    }
#endif
}
