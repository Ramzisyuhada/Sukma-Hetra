using UnityEngine;

/// Status “panas” (gameplay) untuk logam; per-objek.
[DisallowMultipleComponent]
public class HeatedMetal : MonoBehaviour
{
    [Header("Durasi panas (detik)")]
    public float hotDuration = 6f;
    public bool IsHot;
    //public bool IsHot => Time.time < _hotUntil;
    /// 0..1 (1 = baru dipanaskan); bisa dipakai untuk efek visual tambahan 
    public float Heat01 => Mathf.Clamp01((_hotUntil - Time.time) / Mathf.Max(0.0001f, hotDuration));

    private float _hotUntil;

    /// Tandai panas untuk durasi (<=0 → pakai default hotDuration).
    public void Heat(float duration = -1f)
    {
        if (duration <= 0f) duration = hotDuration;
        _hotUntil = Time.time + duration;
        Debug.Log($"now={Time.time:F2}  until={_hotUntil:F2}  IsHot={IsHot}");

    }

    /// Paksa dingin.
    public void ForceCold() => _hotUntil = 0f;
}
