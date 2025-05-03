using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ItemHolder : MonoBehaviour
{
    public ItemData itemData;
    public XRBaseInteractor currentInteractor;

    private void OnEnable()
    {
        var interactable = GetComponent<XRGrabInteractable>();  
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnGrab);
            interactable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnDisable()
    {
        var interactable = GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnGrab);
            interactable.selectExited.RemoveListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject.transform.GetComponent<XRBaseInteractor>();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        currentInteractor = null;
    }

  

}
