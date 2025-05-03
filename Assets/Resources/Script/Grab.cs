using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Grab : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;
    private InputDevice targetDevice;

    public XRBaseInteractor interactor; // Assign di inspector (misalnya controller kiri/kanan)

    void Start()
    {

    }

    void TryInitialize()
    {
      
    }

    void UpdateHandAnimation()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        if (interactor.selectTarget != null)
        {
            GameObject child = transform.GetChild(0).gameObject;
            child.gameObject.SetActive(false);
        }
        else
        {
            GameObject child = transform.GetChild(0).gameObject;
            child.gameObject.SetActive(true);
        }
    }
}
