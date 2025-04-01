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
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Memalu();
        Debug.Log(rb.velocity.magnitude);

    }


    void Memalu()
    {


        if (Physics.CheckSphere(Position.position, Radius, Layer) && rb.velocity.magnitude > 0.1f) 
        {
            if (!Sparks.isPlaying)
            {
                Debug.Log("Hello world");
                Sparks.volume = Random.Range(0.3f, 1.3f);
                Sparks.pitch = Random.Range(0.5f, 1.5f);
                Sparks.Play();
            }
        }
    }
}
