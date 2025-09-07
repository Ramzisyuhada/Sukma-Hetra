using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Blacksmith/Item")]
public class ItemData : ScriptableObject
{
    public JenisBarangEnum itemType;

    [Header("Opsional/Metadata (ikon, prefab default, dsb.)")]
    public Sprite icon;
    public GameObject defaultPrefab;

    [Header("Deprecated / tidak dipakai oleh sistem resep")]
    public int quantity; // Biarkan saja kalau sudah terlanjur ada, tapi tidak digunakan.
}
