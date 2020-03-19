using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviourPunCallbacks
{
    static public GameController Instance;
    public string startScene;
    private GameObject instance;

    [Tooltip("The prefab to use for representing the player")]
    [SerializeField]
    public GameObject playerPrefab;

    [Tooltip("The prefab to use for representing the server")]
    [SerializeField]
    private GameObject serverPrefab;

    void Start()
    {
        Instance = this;
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(startScene);
            return;
        }

        if (playerPrefab == null)
        {
            print("playerPrefab reference null. Please set it up in GameObject Game Controller");
        }
        else if (serverPrefab == null)
        {
            print("serverPrefab reference null. Please set it up in GameObject Game Controller");
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            if (ServerScript.LocalServerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalServer from {0}", SceneManagerHelper.ActiveSceneName);
                GameObject g = PhotonNetwork.Instantiate(this.serverPrefab.name, new Vector3(0f, 3.5f, 0f), Quaternion.identity, 0);
                g.transform.eulerAngles = new Vector3(90, 0, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
        else
        {
            if (PlayerScript.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                GameObject p = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
                p.GetPhotonView().RPC("SetTag", RpcTarget.All, "Player");

            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }
    }
    public override void OnPlayerEnteredRoom(Player player)
    {
        Debug.Log("OnPlayerEnteredRoom() " + player.ActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log("OnPlayerLeftRoom() " + player.ActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }

    public override void OnLeftRoom()
    {
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void QuitApplication()
    {
        PhotonNetwork.LeaveRoom();
        UnityPD.Deinit();
        Application.Quit();
    }

}