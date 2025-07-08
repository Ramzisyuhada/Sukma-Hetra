using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class InteraksiPembelajaran : MonoBehaviour
{
    private VideoPlayer player;
    private GameObject tv;
    bool isPlayingVid = false;
    private void Start()
    {
        player = GetComponent<VideoPlayer>();
    }
    public void PlayVideo()
    {

        isPlayingVid = !isPlayingVid;

        if (isPlayingVid)
        {
            
            player.Play();
        }
        else
        {

            player.Pause();

        }
    }

    public void ResetVideo()
    {
        player.frame = 0;
        player.Play();
    }
}
