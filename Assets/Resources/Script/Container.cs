using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Components;          // HVRGrabbable
using HurricaneVR.Framework.Core.Grabbers;       // HVRHandGrabber, HVRGrabberBase
using HurricaneVR.Framework.Shared;              // HVRGrabTrigger
using HurricaneVR.Framework.Core.HandPoser;      // HVRPosableGrabPoint
using HurricaneVR.Framework.Core;

[RequireComponent(typeof(Collider))]
public class Container : MonoBehaviour
{
    [Header("Crafting")]
    [Tooltip("Daftar resep yang bisa dibuat di container ini.")]
    public List<BlacksmithRecipe> recipes = new List<BlacksmithRecipe>();

    [Tooltip("Semua bahan harus sudah dipegang terus-menerus minimal ini (detik) sebelum crafting.")]
    [Min(0f)] public float holdRequiredSeconds = 0.25f;

    [Tooltip("Bahan dianggap disatukan jika jarak ANTAR bahan <= nilai ini (meter).")]
    public float craftDistance = 0.30f;

    [Header("FX")]
    public GameObject particle;
    [SerializeField] private AudioSource audio;

    [Header("Auto-Grab Result (HVR)")]
    public HVRGrabTrigger resultGrabTrigger = HVRGrabTrigger.ManualRelease;
    public HVRPosableGrabPoint resultGrabPoint;    // boleh null

    // ==== STATE ====
    private readonly HashSet<ItemHolder> _itemsInZone = new HashSet<ItemHolder>();
    private HVRGrabberBase _lastHvrInteractor;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning("[Container] Collider sebaiknya isTrigger = true (bubble area).");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Penting: collider yang masuk sering child; ambil ItemHolder di parent.
        var it = other.GetComponentInParent<ItemHolder>();
        if (!it) return;

        // hindari menghitung dirinya sendiri (kalau Container ini child item)
        if (it.gameObject == gameObject || it.transform.IsChildOf(transform)) return;

        _itemsInZone.Add(it);
    }

    private void OnTriggerExit(Collider other)
    {
        var it = other.GetComponentInParent<ItemHolder>();
        if (!it) return;

        _itemsInZone.Remove(it);
    }

    private void Update()
    {
        // Simpan tangan terakhir dari item yang sedang dipegang (buat auto-grab hasil)
        foreach (var it in _itemsInZone)
        {
            if (!it) continue;

            // Debug: lihat nilai IsHeld sekarang
            // Debug.Log($"[Container] {it.name} IsHeld={it.IsHeld} interactor={it.currentInteractor?.name}");

            if (it.IsHeld && it.currentInteractor != null)
            {
                _lastHvrInteractor = it.currentInteractor;
            }
        }

        TryCraft_HeldOnlyAndClose();
    }

    /// <summary>
    /// Craft hanya jika: semua material yang dibutuhkan SEDANG DIPEGANG,
    /// masing-masing sudah dipegang >= holdRequiredSeconds, saling berdekatan (<= craftDistance),
    /// dan cocok dengan salah satu resep.
    /// </summary>
    private void TryCraft_HeldOnlyAndClose()
    {
        if (_itemsInZone.Count == 0) return;

        // 1) Kumpulkan kandidat yang sedang dipegang dan cukup lama
        var eligible = new List<ItemHolder>();
        var counts = new Dictionary<ItemData, int>();

        foreach (var it in _itemsInZone)
        {
            if (it == null || it.itemData == null) continue;

            // Abaikan yang belum memenuhi durasi, jangan batalkan seluruh proses
            if (!it.HeldFor(holdRequiredSeconds)) continue;

            eligible.Add(it);
            counts[it.itemData] = counts.TryGetValue(it.itemData, out var n) ? n + 1 : 1;
        }

        if (eligible.Count < 2) return; // minimal 2 item dipegang

        // 2) Harus saling berdekatan (semua pasangan dalam jarak <= craftDistance)
        if (!AllCloseEnough(eligible, craftDistance))
            return;

        // 3) Cek resep pakai material eligible
        foreach (var recipe in recipes)
        {
            if (!RecipeMatches(recipe, counts)) continue;

            // ===== Konsumsi bahan sesuai jumlah resep =====
            PlayAudioSafe();
            Consume(recipe, eligible);

            // ===== Spawn hasil di titik tengah =====
            var pos = MidPoint(eligible);
            var result = Instantiate(recipe.resultPrefab, pos, Quaternion.identity);

            if (particle)
            {
                var fx = Instantiate(particle, pos, Quaternion.identity);
                Destroy(fx, 2f);
            }

            // ===== Auto-grab hasil ke tangan terakhir =====
            AutoAttachResult(result);

            Debug.Log($"✅ Craft berhasil (held-only): {recipe.resultItem?.name ?? result.name}");
            return;
        }
    }

    private bool AllCloseEnough(List<ItemHolder> items, float maxDist)
    {
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = i + 1; j < items.Count; j++)
            {
                if (!items[i] || !items[j]) continue;
                if (Vector3.Distance(items[i].transform.position, items[j].transform.position) > maxDist)
                    return false;
            }
        }
        return true;
    }

    private void Consume(BlacksmithRecipe recipe, List<ItemHolder> pool)
    {
        var work = new List<ItemHolder>(pool);
        foreach (var req in recipe.requiredMaterials)
        {
            if (req == null || req.item == null) continue;

            int need = Mathf.Max(1, req.quantity);
            for (int i = work.Count - 1; i >= 0 && need > 0; i--)
            {
                var h = work[i];
                if (h != null && h.itemData == req.item)
                {
                    _itemsInZone.Remove(h);
                    Destroy(h.gameObject);
                    work.RemoveAt(i);
                    need--;
                }
            }
        }
    }

    private Vector3 MidPoint(List<ItemHolder> items)
    {
        Vector3 sum = Vector3.zero; int n = 0;
        foreach (var it in items)
        {
            if (!it) continue;
            sum += it.transform.position; n++;
        }
        return n > 0 ? sum / n : transform.position;
    }

    private void AutoAttachResult(GameObject result)
    {
        if (!result) return;

        if (_lastHvrInteractor is HVRHandGrabber hand && result.TryGetComponent(out HVRGrabbable hvrGrab))
        {
            if (hand.IsGrabbing) hand.ForceRelease();                 // lepas yang lama kalau perlu
            hand.Grab(hvrGrab, resultGrabTrigger, resultGrabPoint);   // langsung pegang hasil
        }
        else if (_lastHvrInteractor is HVRHandGrabber lastHand)
        {
            // fallback: taruh dekat tangan supaya mudah di-grip manual
            result.transform.SetPositionAndRotation(lastHand.transform.position, lastHand.transform.rotation);
        }
    }

    private bool RecipeMatches(BlacksmithRecipe recipe, Dictionary<ItemData, int> counts)
    {
        if (recipe == null || recipe.requiredMaterials == null || recipe.requiredMaterials.Count == 0)
            return false;

        foreach (var req in recipe.requiredMaterials)
        {
            if (req == null || req.item == null) return false;
            int need = Mathf.Max(1, req.quantity);
            if (!counts.TryGetValue(req.item, out int have) || have < need)
                return false;
        }
        return true;
    }

    private void PlayAudioSafe()
    {
        if (audio && !audio.isPlaying) audio.Play();
    }
}
