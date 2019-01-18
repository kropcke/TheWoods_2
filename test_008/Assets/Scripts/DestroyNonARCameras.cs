using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyNonARCameras : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (Network.isServer)
        {
            print("Destroying self");
            Network.Destroy(transform.parent.gameObject);
        }
        while (Camera.main.gameObject != gameObject)
        {
            Camera.main.tag = "Untagged";
            transform.tag = "MainCamera";
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
