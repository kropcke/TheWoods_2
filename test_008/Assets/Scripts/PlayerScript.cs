using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public GameObject ARCameraPrefab;

    GameObject ARCam, testPos;

    bool oriented = false;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }
        else
        {

        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    [PunRPC]
    void setTag(string text)
    {
        this.tag = text;
    }
    void Start()
    {
        if (photonView.IsMine)
        {
            ARCam = Instantiate(ARCameraPrefab);
            //GameObject.FindWithTag("ARImage").GetPhotonView().RequestOwnership();
            //testPos = PhotonNetwork.Instantiate("Cube", Vector3.zero, Quaternion.identity);
            //GameObject.FindWithTag("ARImage").transform.GetChild(0).gameObject.
            //GetPhotonView().RequestOwnership();
        }

    }

    public void Update()
    {
        if (photonView.IsMine && ARCam)
        {
            if (GameObject.FindWithTag("ARImage").transform.GetChild(0).GetComponent<Renderer>().enabled)
            {
                oriented = true;
            }
            if (oriented)
            {

                // The initial position and rotation for the phone
                //transform.position = -GameObject.FindWithTag("ARImage").transform.GetChild(0).position
                //    + ARCam.transform.position;
                transform.position = ARCam.transform.position;
                transform.rotation = ARCam.transform.rotation;

                // Correct for rotating around
                Vector3 axis1 = new Vector3();
                Vector3 axis2 = new Vector3();
                float angle1, angle2;
                GameObject.FindWithTag("ARImage").transform.rotation.ToAngleAxis(out angle1, out axis1);
                GameObject.FindWithTag("ARImage").transform.GetChild(0).rotation.ToAngleAxis(out angle2, out axis2);
                //transform.RotateAround(Vector3.zero, axis1, -angle1);
                //transform.RotateAround(Vector3.zero, axis2, -angle2);

                GameObject.Find("DebugText").GetComponent<Text>().text =
                    "ARImage Position: " + GameObject.FindWithTag("ARImage").transform.GetChild(0).position + "\n"
                    + "ARImage Rotation: " + GameObject.FindWithTag("ARImage").transform.GetChild(0).rotation + "\n"
                    + "ARCAM position: " + ARCam.transform.position + "\n"
                    + "ARCAM rotation: " + ARCam.transform.rotation + "\n"
                    + "ARImage parent position :" + GameObject.FindWithTag("ARImage").transform.position + "\n"
                    + "ARImage parent rotation :" + GameObject.FindWithTag("ARImage").transform.rotation + "\n";

                //transform.position = ARCam.transform.position;
                //transform.rotation = ARCam.transform.rotation;
                //transform.position = GameObject.FindWithTag("ARImage").transform.GetChild(0).position;
                //transform.rotation = ARCam.transform.rotation;// * 
                //Quaternion.Inverse(GameObject.FindWithTag("ARImage").transform.GetChild(0).rotation);

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
