using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Palu : MonoBehaviour
{

    [SerializeField] private Transform Position;
    [SerializeField] private float Radius = 0.1f;
    [SerializeField] private LayerMask Layer;

    [SerializeField] private AudioSource Sparks;

    [SerializeField] private float KekuatanMemalu = 0.1f;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Memalu();
      


    }


    void Memalu()
    {


        //if (Physics.CheckSphere(Position.position, Radius, Layer) && rb.velocity.magnitude > KekuatanMemalu) 
        //{

        //        Debug.Log("Hello world");
        //        Sparks.volume = Random.Range(0.3f, 1.3f);
        //        Sparks.pitch = Random.Range(0.5f, 1.5f);
        //        Sparks.Play();

        //}

        Collider[] hits = Physics.OverlapSphere(Position.position, Radius, Layer);

        foreach (Collider col in hits)
        {
            if (rb.velocity.magnitude > KekuatanMemalu)
            {
                Debug.Log("Palu mengenai: " + col.name);

                Sparks.volume = Random.Range(0.3f, 1.3f);
                Sparks.pitch = Random.Range(0.5f, 1.5f);
                if (!Sparks.isPlaying) Sparks.Play();

                Vector3 scale = col.transform.localScale;

                scale.x += 0.05f;
                scale.y -= 0.03f;
                scale.z += 0.05f;

                scale.x = Mathf.Min(scale.x, 0.5f);  
                scale.y = Mathf.Max(scale.y, 0.05f); 
                scale.z = Mathf.Min(scale.z, 0.5f);  

                col.transform.localScale = scale;

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
