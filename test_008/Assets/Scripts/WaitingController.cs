using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Photon.Pun;
using Photon.Realtime;
using System;

public class WaitingController : MonoBehaviourPunCallbacks
{

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
    [SerializeField]
    private GameObject backgroundAudio;

    // Start is called before the first frame update
    void Start()
    {
        //playerCountUpdate();
        
    }

    public override void OnJoinedRoom()
    {
        print("number of players" + PhotonNetwork.PlayerList.Length);
        if (PhotonNetwork.PlayerList.Length == 3)
        {
            readyToStart = true;
            waitingMenu.SetActive(false);
        }

    }

    private void playerCountUpdate()
    {
        
        playersCount = PhotonNetwork.PlayerList.Length;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;
        if (playersCount == roomSize)
        {
            readyToStart = true;
            waitingMenu.SetActive(false);
        }
        else
        {
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
            backgroundAudio.GetComponent<AudioSource>().volume = 0.5f;
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
        PhotonNetwork.CurrentRoom.IsOpen = false;
        videoPlayer.SetActive(true);



    }

   

   
}
