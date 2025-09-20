using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HurricaneVR.Framework.Components;          // HVRGrabbable
using HurricaneVR.Framework.Core.Grabbers;       // HVRHandGrabber, HVRGrabberBase
using HurricaneVR.Framework.Shared;              // HVRGrabTrigger
using HurricaneVR.Framework.Core.HandPoser;      // HVRPosableGrabPoint
using HurricaneVR.Framework.Core;

#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

[RequireComponent(typeof(Collider))] // collider kita biarkan trigger agar mudah overlap
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

    [Header("Safety / Anti-Dobel")]
    [Tooltip("Cooldown lokal setelah crafting agar tidak memicu dua kali beruntun.")]
    [SerializeField] private float craftCooldown = 0.15f;

    [Tooltip("Aktifkan agar hanya server (NGO) yang mengeksekusi crafting. Abaikan jika singleplayer.")]
    public bool serverAuthoritative = false;

    [Header("Scan Area (tanpa Trigger Enter/Exit)")]
    [Tooltip("Radius sphere scan untuk mencari ItemHolder di sekitar Container.")]
    [Min(0.01f)] public float scanRadius = 0.6f;

    [Tooltip("Layer benda-benda ItemHolder. Wajib isi untuk efisiensi.")]
    public LayerMask itemLayers = ~0;

    [Tooltip("Apakah pakai posisi world dari transform ini sebagai pusat scan.")]
    public bool scanAtTransformPosition = true;

    [Tooltip("Offset pusat scan (jika perlu).")]
    public Vector3 scanOffset = Vector3.zero;

    [Header("Load Resep (Optional)")]
    [Tooltip("Jika true, menambah resep dari Resources/Script/Recipe TAPI tidak menimpa daftar di Inspector.")]
    public bool loadRecipesFromResources = true;
    [SerializeField] private string resourcesFolderPath = "Script/Recipe";

    private readonly HashSet<ItemHolder> _itemsInZone = new HashSet<ItemHolder>();
    private HVRGrabberBase _lastHvrInteractor;
    private float _nextCraftAllowed = -999f;

    // Gate lintas-instance agar bahan yang baru saja dipakai tidak terhitung lagi 1 frame setelahnya
    private static readonly HashSet<ItemHolder> _recentlyConsumed = new HashSet<ItemHolder>();
    private static float _recentGateExpiry = -999f;
    private const float RecentGateWindow = 0.25f;

    private int _id;

    [Header("Debug Draw")]
    public bool debugDraw = true;
    public bool debugDrawRuntime = true;
    public bool debugShowText = true;
    public float debugLineDuration = 0.05f;
    public float debugSphereRadius = 0.02f;
    public Color debugNearColor = Color.green;
    public Color debugFarColor = Color.red;
    public Color debugCircleColor = new Color(0.2f, 0.8f, 1f, 0.6f);

    private const int OverlapMax = 64;
    private readonly Collider[] _overlapBuf = new Collider[OverlapMax];

    private void Awake()
    {
        _id = GetInstanceID();

        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;

        // Tambah resep dari Resources TANPA menimpa list dari Inspector
        if (loadRecipesFromResources)
        {
            var loaded = Resources.LoadAll<BlacksmithRecipe>(resourcesFolderPath);
            if (loaded != null && loaded.Length > 0)
            {
                var set = new HashSet<BlacksmithRecipe>(recipes.Where(r => r));
                foreach (var r in loaded) if (r) set.Add(r);
                recipes = set.ToList();
            }
        }
    }

    private void Update()
    {
#if UNITY_NETCODE_GAMEOBJECTS
        if (serverAuthoritative && NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            return;
#endif
        RebuildItemsInZoneViaScan();

        if (debugDraw && debugDrawRuntime)
            DrawRuntimeLines();

        if (Time.time > _recentGateExpiry && _recentlyConsumed.Count > 0)
            _recentlyConsumed.Clear();

        // Cache interaktor tangan terakhir
        foreach (var it in _itemsInZone)
        {
            if (!it) continue;
            if (it.IsHeld && it.currentInteractor != null)
                _lastHvrInteractor = it.currentInteractor;
        }

        TryCraft_HeldOnlyAndClose();
    }

    private void RebuildItemsInZoneViaScan()
    {
        _itemsInZone.Clear();

        Vector3 center = scanAtTransformPosition ? transform.position : Vector3.zero;
        center += scanOffset;

        int hitCount = Physics.OverlapSphereNonAlloc(center, scanRadius, _overlapBuf, itemLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hitCount; i++)
        {
            var col = _overlapBuf[i];
            if (!col) continue;

            var it = col.GetComponentInParent<ItemHolder>();
            if (!it) continue;

            // Jangan ambil dirinya sendiri / child-nya
            if (it.gameObject == gameObject || it.transform.IsChildOf(transform)) continue;

            _itemsInZone.Add(it);
        }
    }

    private void DrawRuntimeLines()
    {
        var list = new List<ItemHolder>(_itemsInZone);
        for (int i = 0; i < list.Count; i++)
        {
            var a = list[i];
            if (!a) continue;

            DebugDrawSphere(a.transform.position, debugSphereRadius, debugNearColor, debugLineDuration);

            for (int j = i + 1; j < list.Count; j++)
            {
                var b = list[j];
                if (!b) continue;

                float d = Vector3.Distance(a.transform.position, b.transform.position);
                var col = (d <= craftDistance) ? debugNearColor : debugFarColor;
                Debug.DrawLine(a.transform.position, b.transform.position, col, debugLineDuration, false);
            }
        }

        const int seg = 32;
        var c = scanAtTransformPosition ? transform.position : Vector3.zero;
        c += scanOffset;
        Vector3 prev = c + new Vector3(scanRadius, 0f, 0f);
        for (int i = 1; i <= seg; i++)
        {
            float t = i * Mathf.PI * 2f / seg;
            Vector3 nxt = c + new Vector3(Mathf.Cos(t) * scanRadius, 0f, Mathf.Sin(t) * scanRadius);
            Debug.DrawLine(prev, nxt, debugCircleColor, debugLineDuration, false);
            prev = nxt;
        }
    }

    private void DebugDrawSphere(Vector3 center, float r, Color c, float dur)
    {
        const int seg = 16;
        // xy
        for (int i = 0; i < seg; i++)
        {
            float t0 = (i) * Mathf.PI * 2f / seg;
            float t1 = (i + 1) * Mathf.PI * 2f / seg;
            var p0 = center + new Vector3(Mathf.Cos(t0) * r, Mathf.Sin(t0) * r, 0f);
            var p1 = center + new Vector3(Mathf.Cos(t1) * r, Mathf.Sin(t1) * r, 0f);
            Debug.DrawLine(p0, p1, c, dur, false);
        }
        // xz
        for (int i = 0; i < seg; i++)
        {
            float t0 = (i) * Mathf.PI * 2f / seg;
            float t1 = (i + 1) * Mathf.PI * 2f / seg;
            var p0 = center + new Vector3(Mathf.Cos(t0) * r, 0f, Mathf.Sin(t0) * r);
            var p1 = center + new Vector3(Mathf.Cos(t1) * r, 0f, Mathf.Sin(t1) * r);
            Debug.DrawLine(p0, p1, c, dur, false);
        }
        // yz
        for (int i = 0; i < seg; i++)
        {
            float t0 = (i) * Mathf.PI * 2f / seg;
            float t1 = (i + 1) * Mathf.PI * 2f / seg;
            var p0 = center + new Vector3(0f, Mathf.Cos(t0) * r, Mathf.Sin(t0) * r);
            var p1 = center + new Vector3(0f, Mathf.Cos(t1) * r, Mathf.Sin(t1) * r);
            Debug.DrawLine(p0, p1, c, dur, false);
        }
    }

    private void TryCraft_HeldOnlyAndClose()
    {
        if (_itemsInZone.Count == 0) return;
        if (Time.time < _nextCraftAllowed) return;

        // Kumpulkan kandidat yang SEDANG dipegang & cukup lama
        var eligible = new List<ItemHolder>();
        foreach (var it in _itemsInZone)
        {
            if (it == null || it.itemData == null) continue;
            if (!it.IsHeld) continue;
            if (!it.HeldFor(holdRequiredSeconds)) continue;
            if (_recentlyConsumed.Contains(it)) continue;   // gate lintas-instance
            eligible.Add(it);
        }

        // Minimal dua item untuk kombinasi
        if (eligible.Count < 2) return;

        // Semua pasangan harus cukup dekat
        if (!AllCloseEnough(eligible, craftDistance))
            return;

        // LOG: tunjukkan apa yang benar-benar sedang dipegang
#if UNITY_EDITOR
        {
            var names = eligible
                .Where(h => h && h.itemData)
                .GroupBy(h => h.itemData.name)
                .Select(g => $"{g.Key} x{g.Count()}")
                .ToArray();
            Debug.Log($"[Container #{_id}] Held items (EXACT check): {string.Join(", ", names)}");
        }
#endif

        // Urutkan resep dari yang paling “spesifik” (total quantity terbesar)
        var ordered = recipes
            .Where(r => r != null)
            .OrderByDescending(r => r.requiredMaterials?.Sum(m => Mathf.Max(1, m?.quantity ?? 0)) ?? 0)
            .ToList();

        foreach (var recipe in ordered)
        {
            if (!RecipeMatchesExact(recipe, eligible, holdRequiredSeconds)) continue;

            // Tandai untuk gate lintas-instance
            foreach (var e in eligible) if (e) _recentlyConsumed.Add(e);
            _recentGateExpiry = Time.time + RecentGateWindow;

            // Jalankan crafting
            Debug.Log($"[Container #{_id}] Craft MATCH (EXACT). Konsumsi bahan & spawn hasil.");
            PlayAudioSafe();
            Consume(recipe, eligible);

            var pos = MidPoint(eligible);
            var result = Instantiate(recipe.resultPrefab, pos, Quaternion.identity);

            if (particle)
            {
                var fx = Instantiate(particle, pos, Quaternion.identity);
                Destroy(fx, 2f);
            }

            AutoAttachResult(result);

            _nextCraftAllowed = Time.time + craftCooldown;
            Debug.Log($"✅ Craft oleh Container #{_id}: {recipe.resultItem?.name ?? result.name} @ {pos}");
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

    // === EXACT recipe match ===
    private bool RecipeMatchesExact(BlacksmithRecipe recipe, List<ItemHolder> heldItems, float holdSeconds)
    {
        if (recipe == null || recipe.requiredMaterials == null || recipe.requiredMaterials.Count == 0)
            return false;

        // Multiset held (yang memenuhi hold time)
        var countsHeld = new Dictionary<ItemData, int>();
        foreach (var h in heldItems)
        {
            if (!h || !h.itemData) continue;
            if (!h.IsHeld || !h.HeldFor(holdSeconds)) continue;
            countsHeld[h.itemData] = countsHeld.TryGetValue(h.itemData, out var n) ? n + 1 : 1;
        }

        // Multiset kebutuhan resep
        var countsNeed = new Dictionary<ItemData, (int qty, int minHit)>();
        foreach (var req in recipe.requiredMaterials)
        {
            if (req == null || req.item == null) return false;
            int q = Mathf.Max(1, req.quantity);
            if (!countsNeed.TryGetValue(req.item, out var v))
                countsNeed[req.item] = (q, req.minForgeHits);
            else
                countsNeed[req.item] = (v.qty + q, Mathf.Max(v.minHit, req.minForgeHits));
        }

        // EXACT: jenis harus sama banyak
        if (countsHeld.Count != countsNeed.Count) return false;

        // EXACT: jumlah per jenis harus sama, dan minForgeHits terpenuhi
        foreach (var kv in countsNeed)
        {
            var item = kv.Key;
            var needQty = kv.Value.qty;
            var minHit = kv.Value.minHit;

            if (!countsHeld.TryGetValue(item, out var haveQty)) return false;
            if (haveQty != needQty) return false;

            if (minHit > 0)
            {
                int ok = 0;
                foreach (var h in heldItems)
                {
                    if (h && h.itemData == item && h.IsHeld && h.HeldFor(holdSeconds) && h.ForgeCount >= minHit)
                    {
                        ok++;
                        if (ok >= needQty) break;
                    }
                }
                if (ok < needQty) return false;
            }
        }
        return true;
    }

    // Konsumsi sesuai requirement resep (prioritas unit yang ForgeCount tinggi, dan hormati minForgeHits)
    private void Consume(BlacksmithRecipe recipe, List<ItemHolder> pool)
    {
        var map = new Dictionary<ItemData, List<ItemHolder>>();
        foreach (var it in pool)
        {
            if (!it || !it.itemData) continue;
            if (!map.TryGetValue(it.itemData, out var list))
            {
                list = new List<ItemHolder>();
                map[it.itemData] = list;
            }
            list.Add(it);
        }

        foreach (var req in recipe.requiredMaterials)
        {
            if (req == null || req.item == null) continue;

            int need = Mathf.Max(1, req.quantity);
            if (!map.TryGetValue(req.item, out var candidates)) continue;

            // Sort by ForgeCount desc
            candidates.Sort((a, b) => b.ForgeCount.CompareTo(a.ForgeCount));

            // Ambil yang memenuhi minForgeHits dulu
            for (int i = candidates.Count - 1; i >= 0 && need > 0; i--)
            {
                var h = candidates[i];
                if (!h) continue;
                if (!h.IsHeld) continue;
                if (req.minForgeHits > 0 && h.ForgeCount < req.minForgeHits) continue;

                _itemsInZone.Remove(h);
                Destroy(h.gameObject);
                candidates.RemoveAt(i);
                need--;
            }

            // Jika minForgeHits = 0, izinkan sisanya
            if (need > 0 && req.minForgeHits == 0)
            {
                for (int i = candidates.Count - 1; i >= 0 && need > 0; i--)
                {
                    var h = candidates[i];
                    if (!h) continue;
                    if (!h.IsHeld) continue;

                    _itemsInZone.Remove(h);
                    Destroy(h.gameObject);
                    candidates.RemoveAt(i);
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
            hand.ForceRelease();
            hvrGrab.ForceRelease();

            result.transform.SetPositionAndRotation(hand.transform.position, hand.transform.rotation);

            // Hindari ManualRelease agar user bisa lepas normal
            var trigger = (resultGrabTrigger == HVRGrabTrigger.ManualRelease)
                            ? HVRGrabTrigger.Active
                            : resultGrabTrigger;

            hand.Grab(hvrGrab, trigger, resultGrabPoint);
        }
        else if (_lastHvrInteractor is HVRHandGrabber lastHand)
        {
            result.transform.SetPositionAndRotation(lastHand.transform.position, lastHand.transform.rotation);
        }
    }

    private void PlayAudioSafe()
    {
        if (audio && !audio.isPlaying) audio.Play();
    }
}
