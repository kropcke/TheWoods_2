using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Photon.Pun;
using Photon.Realtime;
using System;

public class GameEndController : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private int startSceneIndex;
	[SerializeField]
	private GameObject videoPlayer;

	private bool readyToEnd;
	private bool endingGame;
	private bool isStartSceneLoaded = false;

	// Start is called before the first frame update
	void Start()
	{
		videoPlayer.SetActive(true);
	}

	
	// Update is called once per frame
	void Update()
	{

        if (videoPlayer.activeInHierarchy && !isStartSceneLoaded)
		{
			videoPlayer.transform.GetComponent<VideoPlayer>().audioOutputMode = VideoAudioOutputMode.None;
			if (!videoPlayer.transform.GetComponent<VideoPlayer>().isPlaying)
			{

				isStartSceneLoaded = true;

                if (!PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("Loading Start Scene");
                    PhotonNetwork.LoadLevel(0);

                }
            }
		}


	}

	
}
