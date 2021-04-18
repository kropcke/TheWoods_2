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
    private int roomSize = 2;



    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }


    private void Start()
    {
        startButton.SetActive(false);
        Connect();
    }
    IEnumerator delayStart()
    {
        yield return new WaitForSeconds(5f);
        PhotonNetwork.JoinRandomRoom();
    }
    public void Connect()
    {
        isConnecting = true;
        //This is called byServer(laptop)
        if (PhotonNetwork.IsConnected)
        {
            print("Joining Room...");
            if(SystemInfo.deviceType == DeviceType.Desktop)
            {
                PhotonNetwork.LeaveRoom();
                waitingMenu.SetActive(true);
                StartCoroutine(delayStart());
                
            }
            else
            {
                PhotonNetwork.Disconnect();
                startButton.SetActive(true);
            }
            
            
            //PhotonNetwork.JoinRandomRoom();
        
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
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (isConnecting)
            {
                waitingMenu.SetActive(true);
                PhotonNetwork.JoinRandomRoom();
            }
        }
        else
        {
            if (isConnecting)
            {
                print("OnConnectedToMaster: Next -> try to Join Random Room");

                loadingButton.SetActive(false);
                startButton.SetActive(true);
                waitingMenu.SetActive(false);


            }
        }
        //    if (isConnecting)
        //    {
        //        print("OnConnectedToMaster: Next -> try to Join Random Room");

        //        loadingButton.SetActive(false);
        //        startButton.SetActive(true);
        //        waitingMenu.SetActive(false);


        //}
       

        
        
    }

    

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("OnJoinRandomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom("TheWoods", new RoomOptions { MaxPlayers = 2});
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("OnCreateRoomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom("TheWoods", new RoomOptions { MaxPlayers = 2 });
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
