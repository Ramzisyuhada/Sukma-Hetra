using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Palu : MonoBehaviour
{
    [SerializeField] private Transform Position;
    [SerializeField] private float Radius = 0.1f;
    [SerializeField] private LayerMask Layer;
    [SerializeField] private AudioSource Sparks;
    [SerializeField] private float KekuatanMemalu = 0.1f;

    public Rigidbody rb;

    void Start()
    {
      
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (rb == null) return;

        if (((1 << other.gameObject.layer) & Layer) != 0)
        {
            Debug.Log("Hello world");
            if (rb.velocity.magnitude > KekuatanMemalu)
            {
                Debug.Log("Palu mengenai: " + other.name);

                // Putar suara jika belum diputar
                if (Sparks != null)
                {
                    Sparks.volume = Random.Range(0.3f, 1.3f);
                    Sparks.pitch = Random.Range(0.5f, 1.5f);
                    Sparks.Play();
                }

                // Ubah skala objek yang terkena
                Vector3 scale = other.transform.localScale;

                scale.x += 0.05f;
                scale.y -= 0.03f;
                scale.z += 0.05f;

                // Batasi ukuran maksimal/minimal
                scale.x = Mathf.Min(scale.x, 0.5f);
                scale.y = Mathf.Max(scale.y, 0.05f);
                scale.z = Mathf.Min(scale.z, 0.5f);

                other.transform.localScale = scale;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Position != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position.position, Radius);
        }
    }
}
