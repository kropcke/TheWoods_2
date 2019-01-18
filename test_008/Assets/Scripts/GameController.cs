using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : MonoBehaviour {

    public GameObject lightning, lightningEnd1, lightningEnd2;

   

    private void Update()
    {
        if(GameObject.FindWithTag("Player1") && GameObject.FindWithTag("Player2"))
        {
            lightning.SetActive(true);
            lightningEnd1.transform.position = GameObject.FindWithTag("Player1").transform.position;
            lightningEnd2.transform.position = GameObject.FindWithTag("Player2").transform.position;

        }
        else
        {
            lightning.SetActive(false);
        }
    }

}
