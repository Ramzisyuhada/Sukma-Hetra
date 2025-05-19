using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Palu : MonoBehaviour
{
    [SerializeField] private Transform Position;
    [SerializeField] private float Radius = 0.1f;
    [SerializeField] private LayerMask Layer;
    [SerializeField] private AudioSource Sparks;
    [SerializeField] private float KekuatanMemalu = 0.1f;
    [SerializeField] private GameObject particle;
    public Rigidbody rb;

    void Start()
    {
      
    }

    private void OnTriggerEnter(Collider other)
    {

        if (((1 << other.gameObject.layer) & Layer) != 0 || other.gameObject.CompareTag("Besi"))
        {

            if (rb.velocity.magnitude > KekuatanMemalu)
            {

                Vector3 contactPoint = other.ClosestPoint(Position.position);
                Vector3 center = other.bounds.center;
                Vector3 localHitPoint = other.transform.InverseTransformPoint(contactPoint);
                Vector3 hitDirection = localHitPoint.normalized;
                if (particle != null)
                {
                    Destroy(Instantiate(particle, contactPoint, Quaternion.identity), 0.5f);
                }

                if (Sparks != null)
                {
                    Sparks.transform.position = contactPoint;
                    Sparks.volume = Random.Range(0.3f, 1.3f);
                    Sparks.pitch = Random.Range(0.5f, 1.5f);
                    Sparks.Play();
                }

                Vector3 scale = other.transform.localScale;

                if (Mathf.Abs(hitDirection.y) > Mathf.Abs(hitDirection.x) && Mathf.Abs(hitDirection.y) > Mathf.Abs(hitDirection.z))
                {
                    scale.y -= 0.03f;
                    scale.x += 0.02f;
                    scale.z += 0.02f;
                }
                else if (Mathf.Abs(hitDirection.x) > Mathf.Abs(hitDirection.z))
                {
                    scale.x -= 0.03f;
                    scale.y += 0.02f;
                    scale.z += 0.02f;
                }
                else
                {
                    scale.z -= 0.03f;
                    scale.x += 0.02f;
                    scale.y += 0.02f;
                }

                // Batasi ukuran minimum dan maksimum
                scale.x = Mathf.Clamp(scale.x, 0.05f, 0.5f);
                scale.y = Mathf.Clamp(scale.y, 0.05f, 0.5f);
                scale.z = Mathf.Clamp(scale.z, 0.05f, 0.5f);

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
