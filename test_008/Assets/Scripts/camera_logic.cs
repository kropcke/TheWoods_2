using System.Collections;
//using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class camera_logic : NetworkBehaviour {

    public GameObject cam_server;
    public GameObject cam_client;

	// Use this for initialization
	void Start () {


		
	}
	
	// Update is called once per frame
	void Update () {

        if (isServer)
        {
            cam_client.SetActive(false);
            return;
        }

        if (!isServer)
        {
            cam_server.SetActive(false);
            return;
        }
		
	}
}
