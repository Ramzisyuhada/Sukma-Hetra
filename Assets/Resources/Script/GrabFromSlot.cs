using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Shared;
using HurricaneVR.Framework.ControllerInput;
using UnityEngine;

public class GrabFromSlot : MonoBehaviour
{
    [Header("Objek yang Akan Diambil Otomatis")]
    public HVRGrabbable grabbableObject;

    [Header("Input")]
    public HVRGlobalInputs inputs;
    [Header("Warna & Ukuran Gizmo")]
    public Color gizmoColor = Color.green;
    public Vector3 gizmoSize = new Vector3(0.51f, 0.51f, 0.51f);

    private void OnTriggerStay(Collider other)
    {
        if(inputs.LeftGripButtonState.Active || inputs.RightGripButtonState.Active)
        {

            Instantiate(grabbableObject.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, gizmoSize);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, gizmoSize * 0.9f);
    }
}
