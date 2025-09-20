using UnityEngine;
using HurricaneVR.Framework.Components;      // HVRGrabbable
using HurricaneVR.Framework.Core.Grabbers;  // HVRGrabberBase
using HurricaneVR.Framework.Core;
using TMPro;           // versi HVR lain-lain

[DisallowMultipleComponent]
public class ItemHolder : MonoBehaviour
{
    [Header("Data Item")]
    public ItemData itemData;

    /// <summary>Status sedang dipegang oleh tangan HVR.</summary>
    public bool IsHeld { get; private set; }

    /// <summary>Waktu (detik game) kapan terakhir kali status berubah (hold/release).</summary>
    public double LastStateChangeTime { get; private set; }

    /// <summary>Kapan mulai dipegang (detik game). Nol jika tidak sedang dipegang.</summary>
    public double HeldSince { get; private set; }

    [Header("Interactor Terakhir (Read-Only)")]
    public HVRGrabberBase currentInteractor;        // tangan HVR terakhir memegang

    private HVRGrabbable _grabbable;

    public int ForgeCount { get; private set; }

    [SerializeField] private int forgeRequired = 3;   // tampakkan di Inspector
    public int ForgeRequired => forgeRequired;
    public bool IsForgeComplete => ForgeCount >= forgeRequired;


    [SerializeField] private float forgeHitCooldown = 0.15f;
    private double _lastForgeHitTime;


    [Header("UI")]

    [SerializeField]private TMP_Text Memalu;

    /// <summary>Tambah 1 hit tempa. Return true kalau hit dihitung (anti spam).</summary>
    public bool AddForgeHit()
    {
        if (Time.timeAsDouble - _lastForgeHitTime < forgeHitCooldown) return false;

        ForgeCount++;
        _lastForgeHitTime = Time.timeAsDouble;
        Memalu.text = ForgeCount.ToString();
        Debug.Log($"[{name}] ForgeCount = {ForgeCount}/{forgeRequired}");

        if (IsForgeComplete)
        {
            Debug.Log($"[{name}] ✅ Tempa lengkap!");
            // TODO: VFX/SFX selesai, ubah material, dsb.
        }
        return true;
    }
    private void Awake()
    {
        // cari HVRGrabbable di self/child/parent (struktur prefab bisa beda-beda)
        _grabbable = GetComponent<HVRGrabbable>()
                  ?? GetComponentInChildren<HVRGrabbable>()
                  ?? GetComponentInParent<HVRGrabbable>();

        if (_grabbable == null)
            Debug.LogError($"[ItemHolder] {name} butuh HVRGrabbable di prefabnya!");
    }

    private void OnEnable()
    {
        if (_grabbable)
        {
            // Subscribe event (tetap berguna kalau event bekerja normal)
            _grabbable.Grabbed.AddListener(OnGrabbed);
            _grabbable.Released.AddListener(OnReleased);
        }

        ResetState();
        SyncInitialGrabState();  // jika sudah dipegang sebelum enable
    }

    private void OnDisable()
    {
        if (_grabbable)
        {
            _grabbable.Grabbed.RemoveListener(OnGrabbed);
            _grabbable.Released.RemoveListener(OnReleased);
        }
    }

    private void ResetState()
    {
        IsHeld = false;
        currentInteractor = null;
        LastStateChangeTime = Time.timeAsDouble;
        HeldSince = 0;
    }

    /// <summary>Sinkronisasi state awal jika objek sudah dipegang saat komponen diaktifkan.</summary>
    private void SyncInitialGrabState()
    {
        if (_grabbable == null) return;

        try
        {
            var hands = _grabbable.HandGrabbers; // HVR 2.9.x
            bool heldNow = hands != null && hands.Count > 0;
            if (heldNow)
            {
                IsHeld = true;
                currentInteractor = _grabbable.PrimaryGrabber;
                HeldSince = Time.timeAsDouble;
                LastStateChangeTime = Time.timeAsDouble;
            }
        }
        catch
        {
            // Fallback untuk versi HVR lain (aktifkan jika properti ada di versimu)
            // if (_grabbable.IsHandGrabbed) {
            //     IsHeld = true;
            //     currentInteractor = _grabbable.PrimaryGrabber;
            //     HeldSince = Time.timeAsDouble;
            //     LastStateChangeTime = Time.timeAsDouble;
            // }
        }
    }

    private void OnGrabbed(HVRGrabberBase hand, HVRGrabbable _)
    {
        IsHeld = true;
        currentInteractor = hand;
        LastStateChangeTime = Time.timeAsDouble;
        HeldSince = Time.timeAsDouble;  // mulai pegang sekarang
        // Debug.Log($"[ItemHolder] {name} GRABBED by {hand?.name}");
    }

    private void OnReleased(HVRGrabberBase hand, HVRGrabbable _)
    {
        IsHeld = false;
        currentInteractor = null;
        LastStateChangeTime = Time.timeAsDouble;
        HeldSince = 0;
        // Debug.Log($"[ItemHolder] {name} RELEASED");
    }

    /// <summary>
    /// Safety-net: polling state langsung dari HVR tiap frame. 
    /// Jika event tidak terpanggil/terlewat, IsHeld tetap benar mengikuti kondisi real-time.
    /// </summary>
    private void LateUpdate()
    {
        if (_grabbable == null) return;

        bool heldNow = false;
        HVRGrabberBase interactorNow = null;

        try
        {
            // Prefer tangan (kalau ada)
            var handList = _grabbable.HandGrabbers;            // HVR 2.9.x
            if (handList != null && handList.Count > 0)
            {
                heldNow = true;
                interactorNow = _grabbable.PrimaryGrabber;     // harusnya tangan utama
            }
            else
            {
                // Fallback: siapa pun yang lagi pegang (bisa socket / grabber lain)
                var anyList = _grabbable.Grabbers;             // daftar semua grabber (tangan+lain)
                if (anyList != null && anyList.Count > 0)
                {
                    heldNow = true;
                    // PrimaryGrabber kadang masih null sesaat → ambil dari Grabbers[0]
                    interactorNow = _grabbable.PrimaryGrabber != null
                        ? _grabbable.PrimaryGrabber
                        : anyList[0];
                }
            }
        }
        catch
        {
            // Fallback sangat jadul (aktifkan kalau tersedia di versimu)
            // heldNow = _grabbable.IsGrabbed;
            // interactorNow = _grabbable.PrimaryGrabber;
        }

        if (heldNow != IsHeld)
        {
            IsHeld = heldNow;
            LastStateChangeTime = Time.timeAsDouble;

            if (heldNow)
            {
                HeldSince = Time.timeAsDouble;
                currentInteractor = interactorNow;
                // Debug.Log($"[ItemHolder] {name} -> HELD by {currentInteractor?.name} (poll)");
            }
            else
            {
                HeldSince = 0;
                currentInteractor = null;
                // Debug.Log($"[ItemHolder] {name} -> RELEASED (poll)");
            }
        }
        else if (heldNow && currentInteractor == null)
        {
            // Kalau status sudah held tapi interactor kosong, isi ulang dari Primary/Grabbers.
            currentInteractor = interactorNow;
        }
    }


    /// <summary> true kalau sudah dipegang terus-menerus minimal 'seconds' detik. </summary>
    public bool HeldFor(double seconds)
    {
        return IsHeld && (Time.timeAsDouble - HeldSince) >= seconds;
    }
}
