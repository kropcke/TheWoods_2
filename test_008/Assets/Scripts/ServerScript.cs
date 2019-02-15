using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;
    GameObject lightening;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalServerInstance = gameObject;
            lightening = GameObject.Find("Lightening");
        }
        else
        {

        }
        //DontDestroyOnLoad(gameObject);
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    void Start()
    {
        lightening.GetComponent<PhotonView>().RequestOwnership();

        lightening.transform.GetChild(0).GetChild(0).GetComponent<PhotonView>().RequestOwnership();
        lightening.transform.GetChild(0).GetChild(1).GetComponent<PhotonView>().RequestOwnership();

    }

    public void Update()
    {
        if (photonView.IsMine)
        {
            GameObject p1 = GameObject.FindWithTag("Player1");
            GameObject p2 = GameObject.FindWithTag("Player2");
            if(p1 && p2)
            {
                if (lightening)
                {
                    lightening.SetActive(true);
                    lightening.transform.GetChild(0).GetChild(0).transform.position = p1.transform.position;
                    lightening.transform.GetChild(0).GetChild(1).transform.position = p2.transform.position;
                }
            }
            else
            {
                if(lightening) lightening.SetActive(false);
            }
        }
    }

    void ProcessInputs()
    {

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

        }
        else
        {

        }
    }
}
