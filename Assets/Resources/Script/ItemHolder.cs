using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ItemHolder : MonoBehaviour
{
    public ItemData itemData;
    public HVRGrabberBase currentInteractor;
    private HVRGrabbable grabbable;
    private void OnEnable()
    {

        grabbable = GetComponent<HVRGrabbable>();

        if (grabbable != null)
        {
            grabbable.Grabbed.AddListener(Memegang);
            grabbable.Released.AddListener(Lepas);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} tidak memiliki komponen HVRGrabbable!");
        }
    }

    private void Memegang(HVRGrabberBase Deteksi , HVRGrabbable grab)
    {
        currentInteractor = Deteksi;
    }
    private void Lepas(HVRGrabberBase Deteksi, HVRGrabbable grab)
    {
        currentInteractor = null;
    }

    private void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.Grabbed.RemoveListener(Memegang);
            grabbable.Released.RemoveListener(Lepas);
        }


    }

 



  

}
