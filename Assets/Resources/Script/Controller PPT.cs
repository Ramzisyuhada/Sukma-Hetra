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
        if (index < Ppt.Length)
        {
            index++;
            GetComponent<MeshRenderer>().material = Ppt[index];
        }
        else
        {
            index = 0;
        }
    }


}
