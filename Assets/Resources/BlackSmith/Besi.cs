using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Besi : MonoBehaviour
{
    public Material BesiDinginMaterial, BesiPanasMaterial;

    [SerializeField] private float heatingTime = 1.5f;

    private Color hotColor = new Color(1f, 0.3f, 0f, 1f); // Warna besi saat panas
    private Material activeMaterial;

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
        
    }
}
