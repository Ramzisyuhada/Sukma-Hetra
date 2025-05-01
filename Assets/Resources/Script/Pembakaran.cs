using System.Collections;
using System.Collections.Generic;
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
            other.GetComponent<Besi>().PemanasanBesi();
            ItemData[] allItems = Resources.LoadAll<ItemData>("Script/ItemType");

            foreach (ItemData item in allItems)
            {
                if(item.Equals("BesiPanas"))other.GetComponent<Besi>().item.item = item; 
              Debug.Log("Nama Barang: " + item.itemType);
            }            
            isHeatingStarted = true; 
        }
    }
    private void OnTriggerEnter(Collider other)
    {

       
    }
}
