using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public string sceneToLoad;

    bool isConnecting = false;

    string gameVersion = "1";

    [SerializeField]
    private GameObject startButton;

    [SerializeField]
    private GameObject loadingButton;

    [SerializeField]
    private GameObject waitingMenu;

    [SerializeField]
    private int roomSize = 3;



    void Awake()
    {

        PhotonNetwork.AutomaticallySyncScene = true;
        
    }


    private void Start()
    {
        startButton.SetActive(false);
        Connect();
    }

    public void Connect()
    {
        isConnecting = true;

        if (PhotonNetwork.IsConnected)
        {
            print("Joining Room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            print("Connecting...");
            PhotonNetwork.GameVersion = this.gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {

        if (isConnecting)
        {
            print("OnConnectedToMaster: Next -> try to Join Random Room");

            loadingButton.SetActive(false);
            startButton.SetActive(true);
            waitingMenu.SetActive(false);


        }
        
    }

    

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("OnJoinRandomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 3 });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("OnCreateRoomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 3 });
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        print("OnDisconnected() called");
        isConnecting = false;
    }

    public void onClickStart()
    {
        Debug.Log("onClickStart");
        startButton.SetActive(false);
        waitingMenu.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }


}
