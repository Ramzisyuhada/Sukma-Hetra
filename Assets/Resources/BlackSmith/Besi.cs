using UnityEngine;

/// Contoh “Besi” yang bisa dipanaskan. Mengatur visual & status HeatedMetal.
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(HeatedMetal))]
public class Besi : MonoBehaviour
{
    [Header("Material Referensi (opsional)")]
    public Material matDingin;       // referensi material dingin (boleh null)
    public Material matPanas;        // referensi material panas (boleh null)

    [Header("Visual Pemanasan")]
    [SerializeField] private float heatingTime = 1.25f;
    [SerializeField] private Color hotColor = new Color(1f, 0.35f, 0.05f, 1f);
    [SerializeField] private float emissionIntensity = 8f;

    private Renderer _rend;
    private MaterialPropertyBlock _mpb;
    private HeatedMetal _heat;
    private static readonly int _ColorId = Shader.PropertyToID("_Color");
    private static readonly int _EmissionId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _rend = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        _heat = GetComponent<HeatedMetal>();

        // tampilan awal = dingin
        ApplyColdVisual();
    }

    /// Panggil ini saat besi dimasukkan ke tungku / kena api.
    public void PemanasanBesi(float overrideDuration = -1f)
    {
        // 1) set status panas (logika gameplay)
        _heat.Heat(overrideDuration);

        // 2) animasi visual sederhana (lerp warna + emission)
        StopAllCoroutines();
        StartCoroutine(HeatRoutine());
    }

    private System.Collections.IEnumerator HeatRoutine()
    {
        // optional: ganti shared material agar shader keyword _EMISSION aktif
        if (matPanas) _rend.sharedMaterial = matPanas;
        EnableEmissionKeyword(true);

        float t = 0f;
        Color baseCold = matDingin ? matDingin.color : Color.gray;

        while (t < heatingTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / heatingTime);

            Color c = Color.Lerp(baseCold, hotColor, k);
            Color e = Color.Lerp(Color.black, hotColor, k) * emissionIntensity;

            _rend.GetPropertyBlock(_mpb);
            _mpb.SetColor(_ColorId, c);
            _mpb.SetColor(_EmissionId, e);
            _rend.SetPropertyBlock(_mpb);

            yield return null;
        }
    }

    private void LateUpdate()
    {
        // while still hot, keep emission proportionally (optional cooldown visual)
        if (_heat.IsHot)
        {
            float k = Mathf.Pow(_heat.Heat01, 0.6f);
            _rend.GetPropertyBlock(_mpb);
            _mpb.SetColor(_EmissionId, hotColor * (emissionIntensity * k));
            _rend.SetPropertyBlock(_mpb);
        }
        else
        {
            // kalau sudah dingin, kembali ke visual dingin
            ApplyColdVisual();
        }
    }

    private void ApplyColdVisual()
    {
        if (matDingin) _rend.sharedMaterial = matDingin;

        _rend.GetPropertyBlock(_mpb);
        _mpb.SetColor(_ColorId, matDingin ? matDingin.color : Color.gray);
        _mpb.SetColor(_EmissionId, Color.black);
        _rend.SetPropertyBlock(_mpb);

        EnableEmissionKeyword(false);
    }

    private void EnableEmissionKeyword(bool on)
    {
        if (_rend.sharedMaterial == null) return;
        if (on) _rend.sharedMaterial.EnableKeyword("_EMISSION");
        else _rend.sharedMaterial.DisableKeyword("_EMISSION");
    }
}
