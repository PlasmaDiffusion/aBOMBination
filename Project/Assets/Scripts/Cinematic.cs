﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Cinematic : MonoBehaviour {


   public VideoClip clip;
    private VideoPlayer videoPlayer;

    void Start()
    {
        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        videoPlayer.clip = clip;

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our scene to be visible through the video being played.
        //videoPlayer.targetCameraAlpha = 0.5F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        //videoPlayer.url = "/Users/graham/movie.mov";

        // Skip the first 100 frames.
        //videoPlayer.frame = 10;

        // Restart from beginning when done.
        videoPlayer.isLooping = true;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        //videoPlayer.loopPointReached += EndReached;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        
        videoPlayer.Play();
    }

    void Update()
    {
        if ((ulong)videoPlayer.frame == videoPlayer.frameCount || Input.GetKeyUp(KeyCode.Escape))
        {
            //Video has finshed playing!
            SceneManager.LoadScene(1);
        }
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {

        SceneManager.LoadScene(1);
        vp.playbackSpeed = vp.playbackSpeed / 10.0F;

    }
}

