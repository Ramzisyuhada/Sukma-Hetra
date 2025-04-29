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
        if (!isHeatingStarted && other.GetComponent<Besi>() != null)
        {
            other.GetComponent<Besi>().PemanasanBesi();
            other.GetComponent<Besi>().item.item.NamaBarang = JenisBarangEnum.BesiPanas;

            //other.GetComponent<Besi>().Jenis = JenisBarangEnum.BesiPanas;
            isHeatingStarted = true; // Set isHeatingStarted menjadi true agar tidak dipanggil lagi
        }
    }
    private void OnTriggerEnter(Collider other)
    {

       
    }
}
