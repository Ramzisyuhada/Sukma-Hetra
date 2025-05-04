using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStone : MonoBehaviour
{
    [SerializeField] private GameObject particleEffect;
    [SerializeField] private AudioSource audioSource;

    private bool shouldPlayAudio = false;

    private void Start()
    {
        if (particleEffect != null)
            particleEffect.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Besi"))
        {
            shouldPlayAudio = true;

            if (particleEffect != null)
                particleEffect.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Besi"))
        {
            shouldPlayAudio = false;

            if (particleEffect != null)
                particleEffect.SetActive(false);
            audioSource.Stop();

        }
    }

    private void Update()
    {
        if (shouldPlayAudio && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
