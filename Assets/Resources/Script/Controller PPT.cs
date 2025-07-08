using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPPT : MonoBehaviour
{
    [SerializeField] private Material[] Ppt;

    private static int index = 0;


    private void Start()
    {
        GetComponent<MeshRenderer>().material = Ppt[0];
    }

    public void NextPPT()
    {
        if (Ppt.Length == 0) return;

        index++;
        if (index >= Ppt.Length)
        {
            index = 0; // kembali ke awal
        }

        GetComponent<MeshRenderer>().material = Ppt[index];
    }

    public void PrevPPT()
    {
        if (Ppt.Length == 0) return;

        index--;
        if (index < 0)
        {
            index = Ppt.Length - 1; // kembali ke slide terakhir
        }

        GetComponent<MeshRenderer>().material = Ppt[index];
    }


}
