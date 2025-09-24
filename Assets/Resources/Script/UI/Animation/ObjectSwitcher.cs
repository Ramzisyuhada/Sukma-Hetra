using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ObjectSwitcher : MonoBehaviour
{
    [Header("Data UI (urutan harus sejajar dengan objects)")]
    public List<string> Judul = new List<string>();
    public List<string> Pengertian = new List<string>();

    [Header("Objek yang akan diputar (urutan awal sesuai posisi awal)")]
    public List<Transform> objects = new List<Transform>();

    [Header("Posisi Lokal Target (untuk Left, Center, Right)")]
    public Vector3 leftPos, centerPos, rightPos; // localPositions

    [Header("Skala")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 bigScale = new Vector3(1.5f, 1.5f, 1.5f);

    [Header("UI Navigasi")]
    public Button btnLeft;
    public Button btnRight;

    [Header("UI Teks")]
    [SerializeField] private TMP_Text JudulUI;
    [SerializeField] private TMP_Text Deskripsi;

    [Header("Animasi")]
    [SerializeField, Min(0.01f)] private float tweenDuration = 0.5f;
    [SerializeField] private LeanTweenType ease = LeanTweenType.easeInOutSine;

    private int centerIndex = 0; // index objek yang berada di tengah (0..n-1)

    void Awake()
    {
        // Validasi minimal
        if (objects == null) objects = new List<Transform>();
        if (Judul == null) Judul = new List<string>();
        if (Pengertian == null) Pengertian = new List<string>();

        // Sinkronisasi panjang list data teks supaya nggak out of range
        int n = objects.Count;
        if (n == 0)
        {
            Debug.LogWarning("[ObjectSwitcher] 'objects' kosong. Tidak ada yang diatur.");
        }
        if (Judul.Count < n)
        {
            while (Judul.Count < n) Judul.Add(string.Empty);
        }
        if (Pengertian.Count < n)
        {
            while (Pengertian.Count < n) Pengertian.Add(string.Empty);
        }

        // Hook tombol jika ada
        if (btnLeft) btnLeft.onClick.AddListener(SwitchLeft);
        if (btnRight) btnRight.onClick.AddListener(SwitchRight);

        // Nonaktifkan tombol kalau jumlah objek < 2 (nggak ada yang diputar)
        bool canSwitch = n >= 2;
        if (btnLeft) btnLeft.interactable = canSwitch;
        if (btnRight) btnRight.interactable = canSwitch;
    }

    void Start()
    {
        ClampCenterIndex();
        UpdateLayoutAndUI();
    }

    void Update()
    {
        // Shortcut keyboard
        if (Input.GetKeyDown(KeyCode.E)) SwitchRight();
        if (Input.GetKeyDown(KeyCode.Q)) SwitchLeft();
    }

    public void SwitchLeft()
    {
        if (objects.Count < 2) return;
        centerIndex = Mod(centerIndex - 1, objects.Count);
        UpdateLayoutAndUI();
    }

    public void SwitchRight()
    {
        if (objects.Count < 2) return;
        centerIndex = Mod(centerIndex + 1, objects.Count);
        UpdateLayoutAndUI();
    }

    private void UpdateLayoutAndUI()
    {
        UpdatePositionAndScale();
        UpdateTextUI();
    }

    private void UpdatePositionAndScale()
    {
        int n = objects.Count;
        if (n == 0) return;

        for (int i = 0; i < n; i++)
        {
            var t = objects[i];
            if (!t) continue;

            // Batalkan tween lama supaya tidak konflik
            LeanTween.cancel(t.gameObject);

            // posIndex: 0 = Left, 1 = Center, 2 = Right, lainnya = Off (kalau n>3, yang lain disimpan ke kiri/kanan terluar)
            int rel = Mod(i - centerIndex, n);
            Vector3 targetPos;
            Vector3 targetScale;

            if (rel == 0)
            {
                // i adalah centerIndex
                targetPos = centerPos;
                targetScale = bigScale;
            }
            else
            {
                // Tentukan “arah” relatif terhadap center
                // rel kecil -> di kanan berdekatan, rel besar -> di kiri
                // Untuk n==2 : satu di center, satu lagi kita taruh Right
                if (n == 2)
                {
                    targetPos = rightPos;
                    targetScale = normalScale;
                }
                else
                {
                    // Untuk n >= 3: posisi ringkas 3 slot (Left, Center, Right)
                    // Objektif: item paling dekat di kiri -> leftPos, paling dekat di kanan -> rightPos
                    // Heuristik: Jika rel <= n/2, anggap di kanan; jika rel > n/2, anggap di kiri
                    bool toRight = rel <= n / 2;
                    targetPos = toRight ? rightPos : leftPos;
                    targetScale = normalScale;
                }
            }

            // Terapkan di local space
            LeanTween.moveLocal(t.gameObject, targetPos, tweenDuration).setEase(ease);
            LeanTween.scale(t.gameObject, targetScale, tweenDuration).setEase(ease);
        }
    }

    private void UpdateTextUI()
    {
        if (objects.Count == 0) return;

        // Pastikan index aman
        ClampCenterIndex();

        // Ambil judul & pengertian berdasarkan centerIndex
        string title = SafeGet(Judul, centerIndex);
        string desc = SafeGet(Pengertian, centerIndex);

        if (JudulUI) JudulUI.text = title ?? string.Empty;
        if (Deskripsi) Deskripsi.text = desc ?? string.Empty;
    }

    private void ClampCenterIndex()
    {
        if (objects.Count <= 0) { centerIndex = 0; return; }
        centerIndex = Mod(centerIndex, objects.Count);
    }

    private static int Mod(int a, int b)
    {
        if (b == 0) return 0;
        int r = a % b;
        return r < 0 ? r + b : r;
    }

    private static string SafeGet(List<string> list, int idx)
    {
        if (list == null || list.Count == 0) return string.Empty;
        idx = idx < 0 ? 0 : (idx >= list.Count ? list.Count - 1 : idx);
        return list[idx];
    }

#if UNITY_EDITOR
    // Gizmos bantu lihat target posisi di editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(leftPos), 0.02f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(centerPos), 0.02f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.TransformPoint(rightPos), 0.02f);
    }
#endif
}
