using UnityEngine;

[CreateAssetMenu(fileName = "ItemVisualProfile", menuName = "Blacksmith/Item Visual Profile")]
public class ItemVisualProfile : ScriptableObject
{
    [Header("Jenis (opsional, hanya untuk referensi)")]
    public JenisBarangEnum itemType;

    [Header("Material Referensi (opsional)")]
    public Material coldMaterial;        // material dasar
    public Material hotMaterial;         // material saat panas (boleh null)

    [Header("Warna & Emisi")]
    public Color coldColor = Color.white;
    public Color hotColor = new Color(1f, 0.3f, 0f, 1f);
    [Min(0)] public float emissionIntensity = 10f;

    [Header("Animasi")]
    [Min(0)] public float heatingTime = 1.5f;

    [Header("Fitur")]
    public bool supportsHeating = true;  // kalau false, Heat() akan diabaikan
}
