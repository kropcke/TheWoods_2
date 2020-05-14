﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Photon.Pun;
using Photon.Realtime;
using System;

public class WaitingController : MonoBehaviourPunCallbacks
{

    private PhotonView myphotonView;
    [SerializeField]
    private int mainSceneIndex;
    [SerializeField]
    private int menuSceneIndex;
    private int playersCount;
    private int roomSize;

    private bool readyToStart;
    private bool startingGame;
    private bool isMainSceneLoaded =false;
    [SerializeField]
    private GameObject waitingMenu;
    [SerializeField]
    private GameObject videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        myphotonView = GetComponent<PhotonView>();
        //playerCountUpdate();
    }

    public override void OnJoinedRoom()
    {
        print("Loading Waiting Scene");
        print("number of players" + PhotonNetwork.PlayerList.Length);
        if (PhotonNetwork.PlayerList.Length == 3)
        {
            Debug.Log("Player count ==3 on joinedRoom");
            readyToStart = true;
            waitingMenu.SetActive(false);
        }

    }

    private void playerCountUpdate()
    {
        Debug.Log("Player count update");
        
        playersCount = PhotonNetwork.PlayerList.Length;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;
        if (playersCount == roomSize)
        {
            Debug.Log("Player count ==3");
            readyToStart = true;
            waitingMenu.SetActive(false);
        }
        else
        {
            Debug.Log("Player count !=3");
            readyToStart = false;
            waitingMenu.SetActive(true);
            
            

        }
    }
    
    public override void OnPlayerEnteredRoom(Player player)
    {
        Debug.Log("Player entered in Waiting controller: " + player.ActorNumber);
        playerCountUpdate();
        
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }
    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log("Player left the room in Waiting controller: " + player.ActorNumber);
        playerCountUpdate();
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }
    // Update is called once per frame
    void Update()
    {
      
        WaitForthePlayers();
        if (videoPlayer.activeInHierarchy && !isMainSceneLoaded)
        {
            if (!videoPlayer.transform.GetComponent<VideoPlayer>().isPlaying)
            {
            
                isMainSceneLoaded = true;
               
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("Loading Main Scene");
                    PhotonNetwork.LoadLevel(mainSceneIndex);
                }
            }
        }
      
       
    }

    private void WaitForthePlayers()
    {
        if (readyToStart)
        {
            
            if (startingGame)
            {
                return;
            }
            StartGame();
        }
    }

    private void StartGame()
    {
        startingGame = true;
        //if (!PhotonNetwork.IsMasterClient)
        //{
        //    return;
        //}
        print("after if StartGame()");
        PhotonNetwork.CurrentRoom.IsOpen = false;
        // StartCoroutine(playVideo());
        videoPlayer.SetActive(true);



    }

    private IEnumerator playVideo()
    {
        //print("Device Type"+ SystemInfo.deviceType);
        //if(SystemInfo.deviceType == DeviceType.Handheld)
        //{
        //    Handheld.PlayFullScreenMovie("IntroAnimation.mp4", Color.black, FullScreenMovieControlMode.Full);
        //}
        ////Handheld.PlayFullScreenMovie("IntroAnimation.mp4", Color.black, FullScreenMovieControlMode.Full);
        //try
        //{
        //    videoPlayer.GetComponent<VideoPlayer>().Play();
        //}
        //catch
        //{
        //    print("Exception while running video");
        //}
        yield return new WaitForSeconds(10);
        videoPlayer.SetActive(true);
        

    }

   
}