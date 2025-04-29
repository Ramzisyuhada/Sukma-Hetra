using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Besi : MonoBehaviour
{
    public Material BesiDinginMaterial, BesiPanasMaterial;

   
    [SerializeField] private float heatingTime = 1.5f;

    private Color hotColor = new Color(1f, 0.3f, 0f, 1f); 
    private Material activeMaterial;

    public Ingredient item;
    public void PemanasanBesi()
    {
        activeMaterial = new Material(BesiPanasMaterial);
        GetComponent<MeshRenderer>().material = activeMaterial;

        LeanTween.value(gameObject, 0, 1, heatingTime).setOnUpdate((float val) =>
        {
            activeMaterial.color = Color.Lerp(BesiDinginMaterial.color, hotColor, val);
            activeMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, hotColor, val) * 10f);
        }).setOnComplete(() =>
        {
            activeMaterial.color = hotColor;
            //Destroy(activeMaterial);
        }); ;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 center = transform.position + Vector3.up * 1.0f;
        float radius = 0.3f;

        if (Physics.CheckSphere(center, radius))
        {
            Debug.Log("Ada objek di atas!");
        }


    }
}
