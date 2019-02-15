using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class PlayerSpawn : NetworkBehaviour {

    public Camera c;
    public GameObject ARCameraManager;
    // Use this for initialization
    void Start()
    {
        if(ConnectionInfo.connectedAlready)
        {
            return;
        }
        ConnectionInfo.connectionName = transform.tag;
        ConnectionInfo.connectedAlready = true;


        if (ConnectionInfo.connectionName == "Server")
        {
            NetworkServer.Destroy(gameObject);
        }
        else
        {
            Instantiate(ARCameraManager);
            c.tag = "MainCamera";
            int i = 0;
            while (Camera.allCameras.Length > 1)
            {
                if (Camera.allCameras[i] == c)
                {
                    i++;
                }
                else
                {
                    Destroy(Camera.allCameras[i].gameObject);
                    i = 0;
                }
            }
        }

    }

    public override void OnStartClient()
    {
        //print("Client!!!!");
    }
    public override void OnStartServer()
    {
        //print("Server!!!!");
    }

}
