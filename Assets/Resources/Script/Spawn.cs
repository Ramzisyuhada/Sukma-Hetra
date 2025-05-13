using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{

    [SerializeField] private Transform SpawnPoint;
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject.FindWithTag("Player").transform.position = SpawnPoint.position;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
