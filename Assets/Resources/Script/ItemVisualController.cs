using System.Collections;
using UnityEngine;

/// Kontrol visual generik untuk SEMUA item (besi, kayu, dll.) berbasis profil.
/// - MaterialPropertyBlock (tanpa bikin material baru).
/// - Bisa jalan tanpa profil (pakai field fallback).
/// - Aksi: ApplyDefault(), Heat(), SetColdImmediately(), SetHotImmediately().
[RequireComponent(typeof(Renderer))]
public class ItemVisualController : MonoBehaviour
{
    [Header("Profil (disarankan)")]
    public ItemVisualProfile profile;

    [Header("Fallback (dipakai jika profile kosong)")]
    public Material coldMaterial;
    public Material hotMaterial;
    public Color coldColor = Color.white;
    public Color hotColor = new Color(1f, 0.3f, 0f, 1f);
    [Min(0)] public float emissionIntensity = 10f;
    [Min(0)] public float heatingTime = 1.5f;
    public bool supportsHeating = true;

    private Renderer _rend;
    private MaterialPropertyBlock _mpb;
    private Coroutine _heatRoutine;

    private static readonly int _ColorId = Shader.PropertyToID("_Color");
    private static readonly int _EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _rend = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        if (profile)
        {
            if (profile.coldMaterial) coldMaterial = profile.coldMaterial;
            if (profile.hotMaterial) hotMaterial = profile.hotMaterial;
            coldColor = profile.coldColor;
            hotColor = profile.hotColor;
            emissionIntensity = profile.emissionIntensity;
            heatingTime = profile.heatingTime;
            supportsHeating = profile.supportsHeating;
        }

        ApplyDefault();
    }

    /// Set visual default (dingin).
    public void ApplyDefault() => SetColdImmediately();

    /// Mulai animasi pemanasan (kalau supportsHeating = true).
    public void Heat()
    {
        if (!supportsHeating) return;
        if (_heatRoutine != null) StopCoroutine(_heatRoutine);
        _heatRoutine = StartCoroutine(HeatRoutine());
    }

    /// Paksa dingin (reset visual).
    public void SetColdImmediately()
    {
        if (_heatRoutine != null) { StopCoroutine(_heatRoutine); _heatRoutine = null; }

        if (coldMaterial) _rend.sharedMaterial = coldMaterial;

        _rend.GetPropertyBlock(_mpb);
        _mpb.SetColor(_ColorId, coldColor);
        _mpb.SetColor(_EmissionColorId, Color.black);
        _rend.SetPropertyBlock(_mpb);

        SetEmissionKeyword(false);
    }

    /// Paksa panas (tanpa animasi).
    public void SetHotImmediately()
    {
        if (_heatRoutine != null) { StopCoroutine(_heatRoutine); _heatRoutine = null; }

        if (hotMaterial) _rend.sharedMaterial = hotMaterial;

        _rend.GetPropertyBlock(_mpb);
        _mpb.SetColor(_ColorId, hotColor);
        _mpb.SetColor(_EmissionColorId, hotColor * emissionIntensity);
        _rend.SetPropertyBlock(_mpb);

        SetEmissionKeyword(true);
    }

    private IEnumerator HeatRoutine()
    {
        float t = 0f;
        Color fromCol = coldColor;

        SetEmissionKeyword(true);

        while (t < heatingTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / heatingTime);

            Color current = Color.Lerp(fromCol, hotColor, k);
            Color emission = Color.Lerp(Color.black, hotColor, k) * emissionIntensity;

            _rend.GetPropertyBlock(_mpb);
            _mpb.SetColor(_ColorId, current);
            _mpb.SetColor(_EmissionColorId, emission);
            _rend.SetPropertyBlock(_mpb);

            yield return null;
        }

        SetHotImmediately();
        _heatRoutine = null;
    }

    private void SetEmissionKeyword(bool on)
    {
        var mat = _rend.sharedMaterial;
        if (!mat) return;
        if (on) mat.EnableKeyword("_EMISSION");
        else mat.DisableKeyword("_EMISSION");
    }

#if UNITY_EDITOR
    [ContextMenu("Preview/Set Cold")]
    private void _PreviewCold() => SetColdImmediately();
    [ContextMenu("Preview/Set Hot")]
    private void _PreviewHot() => SetHotImmediately();
    [ContextMenu("Preview/Heat Animate")]
    private void _PreviewHeat() => Heat();

    // Helper untuk memanaskan + tandai status gameplay (butuh HeatedMetal di object yg sama)
    [ContextMenu("Preview/Heat + Mark Hot (Self)")]
    private void _PreviewHeatAndMark()
    {
        var heat = GetComponent<HeatedMetal>();
        if (heat) heat.Heat();
        Heat();
    }
#endif
}
