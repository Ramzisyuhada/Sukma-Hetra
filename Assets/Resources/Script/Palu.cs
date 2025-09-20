using UnityEngine;

public class Palu : MonoBehaviour
{
    [Header("Deteksi Tumbukan")]
    [SerializeField] private Transform hitPosition;     // titik referensi di kepala palu
    [SerializeField] private float minHammerSpeed = 0.1f;
    public Rigidbody rb;                                // rigidbody palu

    [Header("Filter Target")]
    [SerializeField] private LayerMask targetLayers;    // layer besi
    [SerializeField] private string targetTag = "Besi"; // atau kosongkan jika pakai layer saja

    [Header("FX")]
    [SerializeField] private AudioSource sparksSfx;
    [SerializeField] private GameObject sparksParticle;
    [SerializeField] private float fxLifetime = 0.6f;

    [Header("Deform (contoh sederhana)")]
    [SerializeField] private float shrink = 0.03f;
    [SerializeField] private float expand = 0.02f;
    [SerializeField] private Vector2 clampRange = new Vector2(0.05f, 0.5f);

    private void Reset() { rb = GetComponent<Rigidbody>(); }
    private void Awake()
    {
       Debug.Log(sparksSfx.gameObject.name);
    }
    private void OnTriggerEnter(Collider other)
    {
        bool layerOK = (targetLayers.value & (1 << other.gameObject.layer)) != 0;
        if (!(layerOK || (!string.IsNullOrEmpty(targetTag) && other.CompareTag(targetTag)))) return;
        if (!rb || rb.velocity.magnitude < minHammerSpeed) return;

        // WAJIB: besi harus sedang panas
        var heat = other.GetComponent<HeatedMetal>();
        if (heat == null || !heat.IsHot) return;

        // Register tempa ke ItemHolder
        var holder = other.GetComponentInParent<ItemHolder>();
        if (holder != null && holder.AddForgeHit())
        {
            // FX & deform sesudah hit tercatat
            Vector3 contact = other.ClosestPoint(hitPosition ? hitPosition.position : transform.position);
            PlayFx(contact);
            if (other.CompareTag("BesiPanjang")) return;
            Deform(other.transform, contact);
        }
        else
        {
            // fallback: tetap mainkan FX jika mau
            Vector3 contact = other.ClosestPoint(hitPosition ? hitPosition.position : transform.position);
            PlayFx(contact);
        }
    }

    private void PlayFx(Vector3 contact)
    {
        if (sparksParticle)
            Destroy(Instantiate(sparksParticle, contact, Quaternion.identity), fxLifetime);

        if (!sparksSfx)
        {
            Debug.LogWarning("[Palu] sparksSfx belum di-assign di Inspector.");
            return;
        }

        // Pastikan ada clip. Kalau tidak ada, log supaya kelihatan di Console.
        if (!sparksSfx.clip)
        {
            Debug.LogWarning("[Palu] AudioSource tidak punya AudioClip. Isi 'Clip' di komponen AudioSource.");
            return;
        }

        // Tempatkan sumber suara di titik kontak (untuk 3D audio)
        sparksSfx.transform.position = contact;

        // Randomisasi pitch & volume
        float pitch = Random.Range(0.9f, 1.3f);
        float vol = Random.Range(0.6f, 1.0f);
        sparksSfx.pitch = pitch;

        // PlayOneShot pakai clip bawaan AudioSource, aman meski AudioSource sedang play
        sparksSfx.PlayOneShot(sparksSfx.clip, vol);
    }


    private void Deform(Transform tr, Vector3 contactWorld)
    {
        var scale = tr.localScale;
        Vector3 localHit = tr.InverseTransformPoint(contactWorld).normalized;

        if (Mathf.Abs(localHit.y) > Mathf.Abs(localHit.x) && Mathf.Abs(localHit.y) > Mathf.Abs(localHit.z))
        {
            scale.y -= shrink; scale.x += expand; scale.z += expand;
        }
        else if (Mathf.Abs(localHit.x) > Mathf.Abs(localHit.z))
        {
            scale.x -= shrink; scale.y += expand; scale.z += expand;
        }
        else
        {
            scale.z -= shrink; scale.x += expand; scale.y += expand;
        }

        scale.x = Mathf.Clamp(scale.x, clampRange.x, clampRange.y);
        scale.y = Mathf.Clamp(scale.y, clampRange.x, clampRange.y);
        scale.z = Mathf.Clamp(scale.z, clampRange.x, clampRange.y);
        tr.localScale = scale;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!hitPosition) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPosition.position, 0.025f);
    }
#endif
}
