using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pembakaran : MonoBehaviour
{
    private bool isHeatingStarted = false;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if (!isHeatingStarted && other.GetComponent<Besi>() != null)
        {
            ItemHolder itemHolder = other.GetComponent<ItemHolder>();
            if (itemHolder != null)
            {
                // 🔥 Ubah itemData ke BesiPanas sebelum masuk ke container
                itemHolder.itemData = ItemDatabase.Instance.GetByType(JenisBarangEnum.BesiPanas);
                Debug.Log("♨️ Item diubah ke: " + itemHolder.itemData.name);
            }

            // Panggil efek pemanasan dll
            other.GetComponent<Besi>().PemanasanBesi();

            isHeatingStarted = true;
        }
    }




    private void OnTriggerEnter(Collider other)
    {

       
    }
}
