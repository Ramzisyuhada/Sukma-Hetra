using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pembakaran : MonoBehaviour
{
    [Tooltip("Durasi panas override (<=0 = pakai default HeatedMetal)")]
    public float overrideDuration = -1f;

    public void TriggerHeat(GameObject target)
    {
        if (!target) return;

        var heat = target.GetComponent<HeatedMetal>();
        if (heat) heat.Heat(overrideDuration);

        var vis = target.GetComponent<ItemVisualController>();
        if (vis && vis.supportsHeating) vis.Heat();
    }

    // Contoh trigger otomatis:
    private void OnTriggerEnter(Collider other)
    {

        other.GetComponent<HeatedMetal>().IsHot = true;
        // filter sesuai kebutuhanmu (tag/layer)
        TriggerHeat(other.gameObject);
    }
}
