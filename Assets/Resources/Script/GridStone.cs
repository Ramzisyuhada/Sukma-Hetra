using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;

#if HURRICANE_VR
using HurricaneVR.Framework.Components;    // HVRGrabbable
using HurricaneVR.Framework.Core.Grabbers; // HVRGrabberBase
#endif

[DisallowMultipleComponent]
public class GridStone : MonoBehaviour
{
    [Header("Filter Target")]
    [SerializeField] private string targetTag = "Besi";

    [Header("Sumber Resep")]
    [SerializeField] private bool loadRecipesFromResources = true;
    [SerializeField] private string resourcesFolderPath = "Script/Recipe";
    [SerializeField] private List<BlacksmithRecipe> recipes = new List<BlacksmithRecipe>();

    [Header("Durasi & Konsumsi")]
    [Min(0.05f)][SerializeField] private float minGrindSeconds = 0.5f;
    [Min(0.05f)][SerializeField] private float defaultGrindSeconds = 1.5f;
    [SerializeField] private bool consumeInputOnCraft = true;

#if HURRICANE_VR
    [Header("Auto-Grab Result (HVR)")]
    [SerializeField] private bool autoGrabResult = true;
    [SerializeField] private bool preferSameHand = true;
    [SerializeField] private float autoGrabDelay = 0.02f; // fallback kecil
#endif

    [Header("FX")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool attachToGridStone = true;
    [SerializeField] private AudioSource audioSource;

    private GameObject _spawnedParticle;
    private ParticleSystem _spawnedPS;
    private bool _effectOn;

    private class GrindState { public float t; public BlacksmithRecipe recipe; }
    private readonly Dictionary<Collider, GrindState> _states = new Dictionary<Collider, GrindState>();

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        if (audioSource != null) audioSource.loop = true;

        if (loadRecipesFromResources)
        {
            var loaded = Resources.LoadAll<BlacksmithRecipe>(resourcesFolderPath);
            recipes = loaded?.Where(r => r != null).Distinct().ToList() ?? new List<BlacksmithRecipe>();
            if (recipes.Count == 0)
                Debug.LogWarning($"[GridStone] Tidak ada Recipe di Resources/{resourcesFolderPath}");
        }
        else
        {
            recipes = recipes?.Where(r => r != null).ToList() ?? new List<BlacksmithRecipe>();
        }
    }

    private void Update()
    {
        if (_states.Count == 0) { if (_effectOn) StopFx(); return; }
        if (!_effectOn) StartFx();

        var keys = ListPool<Collider>.Get(); keys.AddRange(_states.Keys);
        float dt = Time.deltaTime;

        foreach (var col in keys)
        {
            if (!col || !_states.TryGetValue(col, out var s)) { _states.Remove(col); continue; }
            if (s.recipe == null) s.recipe = SelectRecipeFor(col);

            float need = defaultGrindSeconds;
            if (s.recipe != null && s.recipe.grindSeconds >= minGrindSeconds) need = s.recipe.grindSeconds;

            s.t += dt;
            if (s.t >= need)
            {
                CraftWithRecipe(col, s.recipe);
                _states.Remove(col);
            }
        }

        ListPool<Collider>.Release(keys);
        if (_states.Count == 0 && _effectOn) StopFx();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTarget(other)) return;
        if (!_states.ContainsKey(other))
            _states[other] = new GrindState { t = 0f, recipe = SelectRecipeFor(other) };
        if (!_effectOn) StartFx();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidTarget(other)) return;
        if (_states.ContainsKey(other)) _states.Remove(other);
        if (_states.Count == 0 && _effectOn) StopFx();
    }

    private bool IsValidTarget(Collider col)
    {
        if (!col) return false;
        if (string.IsNullOrEmpty(targetTag)) return true;
        return col.CompareTag(targetTag);
    }

    private BlacksmithRecipe SelectRecipeFor(Collider inputCol)
    {
        if (recipes == null || recipes.Count == 0) return null;

        var holder = inputCol ? inputCol.GetComponentInParent<ItemHolder>() : null;
        var item = holder ? holder.itemData : null;

        if (item != null)
        {
            var exact = recipes.FirstOrDefault(r =>
                r != null &&
                r.requiredMaterials != null &&
                r.requiredMaterials.Count > 0 &&
                r.requiredMaterials[0] != null &&
                r.requiredMaterials[0].item == item);
            if (exact != null) return exact;
        }

        return recipes.FirstOrDefault(r => r != null && r.resultPrefab != null && r.grindSeconds >= minGrindSeconds);
    }

    private void CraftWithRecipe(Collider inputCol, BlacksmithRecipe recipeToUse)
    {
        if (recipeToUse == null || recipeToUse.resultPrefab == null || recipeToUse.grindSeconds < minGrindSeconds)
            return;

#if HURRICANE_VR
        // 1) Ambil grabber dari bahan (sebelum bahan dihancurkan)
        var targetGrabber = autoGrabResult ? GetPreferredGrabber(inputCol, preferSameHand) : null;
        TryCall(targetGrabber, "ForceRelease", System.Array.Empty<object>());
#endif

        // 2) Spawn hasil
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
        var resultGO = Instantiate(recipeToUse.resultPrefab, pos, rot);

        // 3) Pastikan prefab siap di-grab
        EnsureGrabbableComponents(resultGO);

        // 4) Hancurkan bahan bila diminta
        if (consumeInputOnCraft && inputCol)
        {
            var go = inputCol.attachedRigidbody ? inputCol.attachedRigidbody.gameObject : inputCol.gameObject;
            if (go) Destroy(go);
        }

#if HURRICANE_VR
        // 5) Coba grab langsung; kalau gagal pakai fallback coroutine
        if (autoGrabResult && targetGrabber != null)
        {
            // a) Set posisi hasil ke tangan terlebih dahulu
            var anchor = FindGrabAnchor(targetGrabber.transform);
            resultGO.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            // b) Immediate grab (multi-signature)
            if (!TryImmediateGrab(targetGrabber, resultGO, anchor))
            {
                // c) Fallback: sesudah physics step + delay kecil
                StartCoroutine(CoAutoGrabResult(resultGO, targetGrabber, anchor));
            }
        }
#endif
    }

    // ================== FX ==================
    private void StartFx()
    {
        _effectOn = true;

        if (particlePrefab != null && _spawnedParticle == null)
        {
            Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

            _spawnedParticle = Instantiate(particlePrefab, pos, rot,
                                           attachToGridStone ? transform : null);
            _spawnedPS = _spawnedParticle.GetComponent<ParticleSystem>();
            if (_spawnedPS != null && !_spawnedPS.isPlaying)
                _spawnedPS.Play(true);
        }

        if (audioSource != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    private void StopFx()
    {
        _effectOn = false;

        if (_spawnedParticle != null)
        {
            if (_spawnedPS != null)
            {
                _spawnedPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(_spawnedParticle, _spawnedPS.main.startLifetime.constantMax + 0.1f);
            }
            else
            {
                Destroy(_spawnedParticle);
            }
            _spawnedParticle = null;
            _spawnedPS = null;
        }

        if (audioSource != null)
            audioSource.Stop();
    }

#if HURRICANE_VR
    // ================== Auto-Grab (HVR) ==================
    private HVRGrabberBase GetPreferredGrabber(Collider inputCol, bool preferSame)
    {
        HVRGrabberBase g = null;

        if (preferSame && inputCol)
        {
            var inputGrabbable = inputCol.GetComponentInParent<HVRGrabbable>();
            if (inputGrabbable != null && inputGrabbable.Grabbers != null && inputGrabbable.Grabbers.Count > 0)
                g = inputGrabbable.Grabbers[0];
        }

        if (g == null)
        {
            var all = FindObjectsOfType<HVRGrabberBase>(true);
            if (all != null && all.Length > 0)
            {
                var p = transform.position;
                g = all.OrderBy(h => (h.transform.position - p).sqrMagnitude).FirstOrDefault();
            }
        }

        return g;
    }

    // Cari anchor di tangan (palm/attach). Kalau tidak ketemu, pakai transform grabber.
    private Transform FindGrabAnchor(Transform grabber)
    {
        string[] names = { "GrabPoint", "GrabAnchor", "Attach", "Anchor", "Palm", "Hand", "Socket" };
        foreach (var n in names)
        {
            var t = grabber.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name.ToLower().Contains(n.ToLower()));
            if (t) return t;
        }
        return grabber;
    }

    // Buat grab-point sementara di result agar ForceGrab(…, Transform) punya target
    private Transform EnsureTempGrabPoint(GameObject result, Transform anchor)
    {
        var t = result.transform.Find("__TempGrabPoint");
        if (!t)
        {
            var go = new GameObject("__TempGrabPoint");
            t = go.transform;
            t.SetParent(result.transform, false);
        }
        t.SetPositionAndRotation(anchor.position, anchor.rotation);
        return t;
    }

    // Lengkapi komponen agar bisa di-grab
    private static void EnsureGrabbableComponents(GameObject go)
    {
        if (!go) return;
        if (!go.GetComponent<Collider>()) go.AddComponent<BoxCollider>();
        var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = false;

        if (!go.GetComponent<HVRGrabbable>()) go.AddComponent<HVRGrabbable>();
    }

    // Coba grab segera dengan berbagai signature, dan optional target transform
    private bool TryImmediateGrab(HVRGrabberBase grabber, GameObject result, Transform anchor)
    {
        if (!grabber || !result) return false;

        var grabbable = result.GetComponent<HVRGrabbable>();
        if (!grabbable) return false;

        // Pastikan RB clean
        if (grabbable.Rigidbody)
        {
            grabbable.Rigidbody.WakeUp();
            grabbable.Rigidbody.velocity = Vector3.zero;
            grabbable.Rigidbody.angularVelocity = Vector3.zero;
        }

        // 1) Signature umum tanpa target transform
        if (TryCall(grabber, "ForceGrab", new object[] { grabbable })) return true;
        if (TryCall(grabber, "TryGrab",   new object[] { grabbable })) return true;
        if (TryCall(grabber, "Grab",      new object[] { grabbable })) return true;

        // 2) Signature dengan Transform target (banyak dipakai di versi tertentu)
        var target = EnsureTempGrabPoint(result, anchor);
        if (TryCall(grabber, "ForceGrab", new object[] { grabbable, target })) return true;
        if (TryCall(grabber, "TryGrab",   new object[] { grabbable, target })) return true;

        // 3) Signature dengan bool flags (mis. matchRotation)
        if (TryCall(grabber, "ForceGrab", new object[] { grabbable, true })) return true;
        if (TryCall(grabber, "TryGrab",   new object[] { grabbable, true })) return true;

        return false;
    }

    private IEnumerator CoAutoGrabResult(GameObject result, HVRGrabberBase grabber, Transform anchor)
    {
        if (!result || !grabber) yield break;

        // Tunggu 1 physics step
        yield return new WaitForFixedUpdate();
        // Delay kecil opsional
        if (autoGrabDelay > 0f) yield return new WaitForSeconds(autoGrabDelay);

        if (TryImmediateGrab(grabber, result, anchor)) yield break;

        // Fallback terakhir: pasang FixedJoint ke tangan supaya tetap “kepegang”
        var rrb = result.GetComponent<Rigidbody>();
        var grbRb = grabber.GetComponentInParent<Rigidbody>();
        if (rrb && grbRb)
        {
            var fj = result.GetComponent<FixedJoint>() ?? result.AddComponent<FixedJoint>();
            fj.connectedBody = grbRb;
            fj.breakForce = Mathf.Infinity;
            fj.breakTorque = Mathf.Infinity;
            // Optional: lepas joint setelah 0.3s lalu coba grab lagi
            yield return new WaitForSeconds(0.3f);
            Destroy(fj);
            TryImmediateGrab(grabber, result, anchor);
        }
    }

    // Refleksi signature beragam
    private static bool TryCall(object obj, string methodName, object[] args)
    {
        if (obj == null) return false;
        var t = obj.GetType();
        var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                       .Where(m => m.Name == methodName);
        foreach (var m in methods)
        {
            var pars = m.GetParameters();
            if (pars.Length != args.Length) continue;

            bool ok = true;
            for (int i = 0; i < pars.Length; i++)
            {
                if (args[i] == null) continue;
                if (!pars[i].ParameterType.IsInstanceOfType(args[i])) { ok = false; break; }
            }
            if (!ok) continue;

            try { m.Invoke(obj, args); return true; } catch { /* ignore */ }
        }
        return false;
    }
#endif
    // Selalu tersedia, hanya penambahan HVRGrabbable yang conditional
    private static void EnsureGrabbableComponents(GameObject go)
    {
        if (!go) return;

        // Pastikan ada Collider
        if (!go.GetComponent<Collider>())
            go.AddComponent<BoxCollider>(); // atau ganti sesuai bentuk hasilmu

        // Pastikan ada Rigidbody
        var rb = go.GetComponent<Rigidbody>();
        if (!rb) rb = go.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = false;

        // Tambahkan HVRGrabbable hanya jika HURRICANE_VR aktif
#if HURRICANE_VR
    if (!go.GetComponent<HVRGrabbable>())
        go.AddComponent<HVRGrabbable>();
#endif
    }

    private void OnDisable()
    {
        StopFx();
        _states.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.05f);
        }
    }

    // ====== ListPool sederhana ======
    static class ListPool<T>
    {
        static readonly Stack<List<T>> Pool = new Stack<List<T>>();
        public static List<T> Get() => Pool.Count > 0 ? Pool.Pop() : new List<T>(8);
        public static void Release(List<T> list) { list.Clear(); Pool.Push(list); }
    }
}
