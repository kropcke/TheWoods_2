using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public string sceneToLoad;

    bool isConnecting = false;
    string gameVersion = "1";

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
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
            PhotonNetwork.JoinRandomRoom();
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("OnJoinRandomFailed() called, creating a new room with room for 3 players");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 3 });
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        print("OnDisconnected() called");
        isConnecting = false;
    }
    public override void OnJoinedRoom()
    {
        print("OnJoinedRoom() called");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            print("Loading MainScene");
            PhotonNetwork.LoadLevel(sceneToLoad);
        }
    }
}
