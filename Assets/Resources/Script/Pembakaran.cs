using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pembakaran : MonoBehaviour
{
    private bool isHeatingStarted = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Besi") && !isHeatingStarted)
        {
            // Panggil PemanasanBesi hanya sekali
            other.GetComponent<Besi>().PemanasanBesi();
            isHeatingStarted = true; // Set isHeatingStarted menjadi true agar tidak dipanggil lagi
        }
    }
    private void OnTriggerEnter(Collider other)
    {

       
    }
}
