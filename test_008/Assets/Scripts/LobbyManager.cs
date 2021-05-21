using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks {
    #region Singleton
    public static LobbyManager instance;

    void Awake() {
        instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    #endregion

    public string sceneToLoad;

    bool isConnecting = false;

    string gameVersion = "1";


    [SerializeField]
    private GameObject loadingButton;

    [SerializeField]
    private int roomSize = 3;

    public string roomName = null; // The Photon room we eventually join.
    public System.Action<bool, string> roomJoinCallbackUI = null; // Callback to update UI on room join succeed/fail.

    private void Start() {
        // Connect to the Photon Network
        print("PUN: Connecting to the Photon Network...");
        PhotonNetwork.ConnectUsingSettings();
    }

    // Callback executed when PhotonNetwork.ConnectUsingSettings() succeeds
    public override void OnConnectedToMaster() {
        print("PUN: Connected successfully to the Photon Network.");
    }

    // Called by UI/CreateRoomButton
    public void CreateRoomClicked(string roomName) {
        this.roomName = roomName;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;
        print("PUN: Create room '" + roomName + "'.");
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    // Called by UI/EnterRoomButton
    public void EnterRoomClicked(string roomName, System.Action<bool, string> callback) {
        this.roomJoinCallbackUI = callback; // save the callback for updating the UI later
        this.roomName = roomName;
        print("PUN: Join room '" + roomName + "'.");
        PhotonNetwork.JoinRoom(roomName);
    }


    // Callback executed when PhotonNetwork.JoinOrCreateRoom() succeeds
    public override void OnJoinedRoom() {
        print("PUN: Joined room '" + roomName + "'.");
        print("Device joined: " + SystemInfo.deviceType);

        if (roomJoinCallbackUI != null) {
            roomJoinCallbackUI(true, roomName);
        }

    }

    // Callback executed when PhotonNetwork.JoinOrCreateRoom() fails
    public override void OnJoinRoomFailed(short returnCode, string message) {
        print("PUN: failed to join room '" + roomName + "'.");
        roomJoinCallbackUI(false, message);
        // TODO: tell users that the room they tried to join is already full
    }

    public override void OnPlayerEnteredRoom(Player player) {
        print("player: " + player.ActorNumber);
    }



    public override void OnJoinRandomFailed(short returnCode, string message) {
        print("OnJoinRandomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom("TheWoods", new RoomOptions { MaxPlayers = 3 });
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        print("OnCreateRoomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom("TheWoods", new RoomOptions { MaxPlayers = 3 });
        // Todo: fix this and add an error screen
    }
    public override void OnDisconnected(DisconnectCause cause) {
        print("OnDisconnected() called");
        isConnecting = false;
    }


}