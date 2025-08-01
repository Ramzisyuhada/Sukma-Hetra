using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class InteraksiPembelajaran : MonoBehaviour
{
    private VideoPlayer player;
    private GameObject tv;
    bool isPlayingVid = false;
    public RenderTexture renderTexture;

    private void Start()
    {
        player = GetComponent<VideoPlayer>();
        renderTexture = new RenderTexture(640, 360, 0); // width, height, depth
        player.targetTexture = renderTexture;
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
