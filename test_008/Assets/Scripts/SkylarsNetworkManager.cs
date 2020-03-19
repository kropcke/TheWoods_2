using UnityEngine;
using UnityEngine.Networking;

public class SkylarsNetworkManager : NetworkManager
{
    public GameObject server;

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("OnPlayerConnected");
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject p = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        if (!GameObject.FindWithTag("Server"))
        {
            p.tag = "Server";
            p.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = Color.white;
            print("Spawning server");
        }
        else if (!GameObject.FindWithTag("Player1"))
        {
            p.tag = "Player1";
            p.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = Color.red;
            print("Spawning player1");
        }
        else if (!GameObject.FindWithTag("Player2"))
        {
            p.tag = "Player2";
            p.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = Color.blue;
            print("Spawning player2");
        }
        else
        {
            print("Spawning player3???");
        }
        NetworkServer.AddPlayerForConnection(conn, p, playerControllerId);

    }

}