using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    private GameObject textInput;
    [SerializeField]
    private GameObject joinRoomButton;
    [SerializeField]
    private GameObject userID;
    Button btn;
    InputField input;
    
    

    [SerializeField]
    private int roomSize = 3;

    
    string roomName = "test";



    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }


    private void Start()
    {
        btn = joinRoomButton.GetComponent<Button>();
        input = textInput.GetComponent<InputField>();
        startButton.SetActive(false);
        // Connect();
        
        // Connect to the Photon Network
        print("PUN: Connecting to the Photon Network...");
        PhotonNetwork.ConnectUsingSettings();
    }

    // Callback executed when PhotonNetwork.ConnectUsingSettings() succeeds
    public override void OnConnectedToMaster()
    {
        print("PUN: Connected successfully to the Photon Network.");
        
        // All users are prompted to enter the same custom room name.
        btn.onClick.AddListener(JoinRoomButtonCallback);


        
        
        waitingMenu.SetActive(true);


        
       
    }

    void JoinRoomButtonCallback() {
        roomName = input.text;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;
        print("PUN: Join or create room '" + roomName + "'.");
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    // Callback executed when PhotonNetwork.JoinOrCreateRoom() succeeds
    public override void OnJoinedRoom()
    {
        print("PUN: Joined room '" + roomName + "'.");
        print("Device joined: " + SystemInfo.deviceType);

        Text text = userID.GetComponent<Text>();

        if(PhotonNetwork.PlayerList.Length == 1) {
            text.text = "the server";
        } else {
            text.text = "client number " +  PhotonNetwork.PlayerList.Length;
        }

        // print(PhotonNetwork.CurrentRoom.Players);
        // text.text = GetComponent<PhotonView>().ownerId




        // if(SystemInfo.deviceType == DeviceType.Desktop) {
        //     // Query the desktop application for a custom room name
        //     roomName = "myRoom"; // TODO add a textbox or something
        //     print("PUN: Creating room '" + roomName + "'.");

        //     // Create a "room" on the Photon Network 
        //     print(PhotonNetwork.CountOfRooms);
        //     PhotonNetwork.CreateRoom(roomName);
        //     print(PhotonNetwork.CountOfRooms);

        // } else {
        //     // Ask the phone user for the room to join
        //     roomName = "myRoom"; // TODO add a textbox to the phones

        //     PhotonNetwork.JoinRoom(roomName);
        // }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        print("player: " + player.ActorNumber);
    }

    // public override void OnRoomListUpdate(List<RoomInfo> roomList)
    // {
    //     print("PUN: Room list updated.");
    //     print(roomList.ToArray());
    // }


    IEnumerator delayStart()
    {
        yield return new WaitForSeconds(5f);
        // PhotonNetwork.JoinRandomRoom();
        PhotonNetwork.JoinRoom(roomName);
    }
    public void Connect()
    {



        if (PhotonNetwork.IsConnected)
        {
            print("Joining Room...");
            print(SystemInfo.deviceType);
            print("ee");
            if(SystemInfo.deviceType == DeviceType.Desktop)
            {
                PhotonNetwork.CreateRoom(roomName);
                // PhotonNetwork.LeaveRoom();
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

    // public override void OnConnectedToMaster()
    // {
    //     if (SystemInfo.deviceType == DeviceType.Desktop)
    //     {
    //         if (isConnecting)
    //         {
    //             waitingMenu.SetActive(true);
    //             PhotonNetwork.JoinRandomRoom();
    //         }
    //     }
    //     else
    //     {
    //         if (isConnecting)
    //         {
    //             print("OnConnectedToMaster: Next -> try to Join Random Room");

    //             loadingButton.SetActive(false);
    //             startButton.SetActive(true);
    //             waitingMenu.SetActive(false);


    //         }
    //     }
    //     //    if (isConnecting)
    //     //    {
    //     //        print("OnConnectedToMaster: Next -> try to Join Random Room");

    //     //        loadingButton.SetActive(false);
    //     //        startButton.SetActive(true);
    //     //        waitingMenu.SetActive(false);


    //     //}
       

        
        
    // }

    

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("OnJoinRandomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom("Matt_Room", new RoomOptions { MaxPlayers = 3 });
        // PhotonNetwork.JoinRoom
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("OnCreateRoomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom("Matt_Room_2", new RoomOptions { MaxPlayers = 3 });
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
