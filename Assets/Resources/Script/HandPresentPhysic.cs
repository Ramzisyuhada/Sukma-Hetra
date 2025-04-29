using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandPresentPhysic : MonoBehaviour
{

   
    public Transform target;
    public Rigidbody rb;
    public Renderer nonPhysicalHand;
    public float showNonPhysicalHandDistance = 0.05f;
    private Collider[] handColider;
    void Start()
    {
       rb = GetComponent<Rigidbody>();
       handColider = GetComponentsInChildren<Collider>();    

    }
    
    public void EnabledHandCollider()
    {
        foreach (Collider col in handColider) 
        {
                col.enabled = true;
        }
    }

    public void DisabledHandCollider()
    {
        foreach (Collider col in handColider)
        { 
                col.enabled=false;
        }
    }
    private void Update()
    {
        float distance = Vector3.Distance(transform.position , target.position);
        if (distance > showNonPhysicalHandDistance) {
            nonPhysicalHand.enabled = true;
        }
        else
        {
            nonPhysicalHand.enabled = false;
        }
    }
    private void FixedUpdate()
    {
        // Position
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;
        // Rotation
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float AngleToDegree, out Vector3 rotationAxis);
        Vector3 rotationInDiffrence = AngleToDegree * rotationAxis;
        rb.angularVelocity = (rotationInDiffrence * Mathf.Deg2Rad / Time.fixedDeltaTime);


    }
    void F()
    {
    }
}