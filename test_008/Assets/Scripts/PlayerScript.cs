using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    public float cubeSize = 0.1f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    GameObject lightningEndpoint;

    GameObject ARCam, testPos;

    bool oriented = false;
    GameObject middleBranch;
    string deviceName = null;
    bool gameOver = false;

    public Texture buttonTexture;
    GameObject server;

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


    private void OnGUI()
    {
        if (photonView.IsMine)
        {
            Rect position;
   
                if (Screen.orientation == ScreenOrientation.Landscape)
                {
                    position = new Rect((Screen.width / 2) + 750, 10, 100, 100);
                }
                else if (Screen.orientation == ScreenOrientation.Portrait)
                {
                    position = new Rect((Screen.width / 2) + 300, 10, 100, 100);
                }
                else
                {
                    position = new Rect((Screen.width / 2) + 300, 70, 100, 100);
                }
                
                
            bool restartGame = GUI.Button(position, buttonTexture);

            if (restartGame)
            {
               
                gameOver = false;
                server.GetComponent<PhotonView>().RPC("UpdateVariableInServer", RpcTarget.All,
                           restartGame);
            

            }
        }


    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    [PunRPC]
    public void SetTag(string text)
    {
        this.tag = text;
    }
    [PunRPC]
    public void TakeControl(string s)
    {
        if (photonView.IsMine)
        {
            GameObject g = GameObject.Find(s);
            g.GetComponent<PhotonView>().RequestOwnership();
            lightningEndpoint = g;
            //OnGUI();
        }
    }

    [PunRPC]
    public void PlayVoiceMail(string soundClip)
    {
        if (photonView.IsMine)
        {
            GameObject g1 = GameObject.Find(soundClip);
            
            g1.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.SetFloat("VoiceMailVolume", 0.5f);
            //g1.GetComponent<AudioSource>().volume = 0.5f;
            g1.GetComponent<AudioSource>().panStereo = -1.0f;
            g1.GetComponent<AudioSource>().Play();
        }
    }

    [PunRPC]
    public void StopVoiceMail(string soundClip)
    {
        if (photonView.IsMine)
        {

            GameObject g1 = GameObject.Find(soundClip);
            if (g1.GetComponent<AudioSource>().isPlaying)
            {
                g1.GetComponent<AudioSource>().Stop();
            }

        }
    }
    [PunRPC]
    public void ShowMenu(string option)
    {
        if (photonView.IsMine)
        {
            //if (option == "gameOver") {
            //    gameOver = true;
            //}
            //else
            //{
            //    gameOver = false;
            //}
            gameOver = true;

        }
    }

    [PunRPC]
    public void EnableBird(int index)
    {
        if (photonView.IsMine)
        {
            GameObject[] birds = GameObject.FindGameObjectsWithTag("Bird");
            for (int i = 0; i < birds.Length; i++)
            {
                birds[i].SetActive(false);
            }
            middleBranch.transform.GetChild(index).gameObject.SetActive(true);
        }
    }
    [PunRPC]
    public void DisableBirds(string name)
    {
        if (photonView.IsMine)
        {
            for (int index = 0; index < middleBranch.transform.childCount; index++)
            {
                middleBranch.transform.GetChild(index).gameObject.SetActive(false);
            }
        }

    }
    void Start()
    {
        if (photonView.IsMine)
        {
            //TOOD: Pass from serverscript
            middleBranch = GameObject.Find("NewMiddleBranch");
            server = GameObject.FindGameObjectWithTag("Server");
            //foreach (var device in Microphone.devices)
            //{
            //    deviceName = device;
            //}
            ARCam = GameObject.Find("ARCamera");
            ARCam.GetComponent<Vuforia.VuforiaBehaviour>().enabled = true;
            //GameObject.FindWithTag("ARImage").GetPhotonView().RequestOwnership();
            //testPos = PhotonNetwork.Instantiate("Cube", Vector3.zero, Quaternion.identity);
            //GameObject.FindWithTag("ARImage").transform.GetChild(0).gameObject.
            //GetPhotonView().RequestOwnership();
            transform.localScale = Vector3.one * cubeSize;


        }


    }

    public void Update()
    {
        if (photonView.IsMine && ARCam)
        {
            /*
            if (GameObject.FindWithTag("ARImage").transform.GetChild(0).GetComponent<Renderer>().enabled)
            {
                oriented = true;
            }
            */
            oriented = true;

            if (oriented)
            {

                // The initial position and rotation for the phone
                //transform.position = -GameObject.FindWithTag("ARImage").transform.GetChild(0).position
                //    + ARCam.transform.position;
                transform.position = ARCam.transform.position;
                transform.rotation = ARCam.transform.rotation;
                if (lightningEndpoint)
                {
                    lightningEndpoint.transform.position = transform.GetChild(1).position;
                }

                // Correct for rotating around
                /*
                Vector3 axis1 = new Vector3();
                Vector3 axis2 = new Vector3();
                float angle1, angle2;
                GameObject.FindWithTag("ARImage").transform.rotation.ToAngleAxis(out angle1, out axis1);
                GameObject.FindWithTag("ARImage").transform.GetChild(0).rotation.ToAngleAxis(out angle2, out axis2);
                */
                //transform.RotateAround(Vector3.zero, axis1, -angle1);
                //transform.RotateAround(Vector3.zero, axis2, -angle2);
                /*
                GameObject.Find("DebugText").GetComponent<Text>().text =
                    "ARImage Position: " + GameObject.FindWithTag("ARImage").transform.GetChild(0).position + "\n"
                    + "ARImage Rotation: " + GameObject.FindWithTag("ARImage").transform.GetChild(0).rotation + "\n"
                    + "ARCAM position: " + ARCam.transform.position + "\n"
                    + "ARCAM rotation: " + ARCam.transform.rotation + "\n"
                    + "ARImage parent position :" + GameObject.FindWithTag("ARImage").transform.position + "\n"
                    + "ARImage parent rotation :" + GameObject.FindWithTag("ARImage").transform.rotation + "\n";
                    */
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
