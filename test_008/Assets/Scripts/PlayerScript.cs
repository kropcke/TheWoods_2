using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    public float cubeSize = 0.1f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    GameObject lightningEndpoint;

    GameObject ARCam, testPos;

    bool oriented = false;
    GameObject middleBranch;
    bool gameOver = false;

    public Texture restartButtonTexture;
    public Texture newGameTexture;
    GameObject server;

    private GameObject newGameButton;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
            newGameButton = GameObject.Find("NewGameButton");
            
        }
       
    }

    //TODO: Refactor this 
    private void OnGUI()
    {
        if (photonView.IsMine)
        {

            if (gameOver)
            {
                Rect position = new Rect((Screen.width / 2)-200, (Screen.height / 2), 350, 80);
                bool newGame = GUI.Button(position, newGameTexture);
                if (newGame)
                {

                    server.GetComponent<PhotonView>().RPC("UpdateVariableInServer", RpcTarget.All,
                               "NewGame");
                    PhotonNetwork.LoadLevel(0);


                }
            }
            //else
            //{
            //    Rect position;

            //    if (Screen.orientation == ScreenOrientation.Landscape)
            //    {
            //        position = new Rect((Screen.width / 2) + 750, 10, 100, 100);
            //    }
            //    else if (Screen.orientation == ScreenOrientation.Portrait)
            //    {
            //        position = new Rect((Screen.width / 2) + 300, 10, 100, 100);
            //    }
            //    else
            //    {
            //        position = new Rect((Screen.width / 2) + 300, 70, 100, 100);
            //    }
            //    bool restartGame = GUI.Button(position, restartButtonTexture);

            //    if (restartGame)
            //    {

            //        gameOver = false;
            //        server.GetComponent<PhotonView>().RPC("UpdateVariableInServer", RpcTarget.All,
            //                  "RestartGame");
                   




            //    }
            //}
               
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
        }
    }


    [PunRPC]
    public void PlayVoiceMail(string soundClip)
    {
        if (photonView.IsMine)
        {
            GameObject g1 = GameObject.Find(soundClip);
            
            g1.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.SetFloat("VoiceMailVolume", 0.5f);
            g1.GetComponent<AudioSource>().panStereo = -1.0f;
            g1.GetComponent<AudioSource>().Play();
            StartCoroutine(disableParticleSystem(g1.GetComponent<AudioSource>().clip.length));
        }
    }

    IEnumerator disableParticleSystem(float length)
    {
        yield return new WaitForSeconds(length);
        Debug.Log("AudioPlayed");

        for(int i=0; i<4; i++)
        {
            if (middleBranch.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                middleBranch.transform.GetChild(i).gameObject.transform.GetChild(3).gameObject.SetActive(false);
            }
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
            ARCam = GameObject.Find("ARCamera");
            ARCam.GetComponent<Vuforia.VuforiaBehaviour>().enabled = true;
            //GameObject.FindWithTag("ARImage").GetPhotonView().RequestOwnership();
            //testPos = PhotonNetwork.Instantiate("Cube", Vector3.zero, Quaternion.identity);
            //GameObject.FindWithTag("ARImage").transform.GetChild(0).gameObject.
            //GetPhotonView().RequestOwnership();
            transform.localScale = Vector3.one * cubeSize;

            //lineRenderer = transform.GetComponent<LineRenderer>();




        }


    }

    public void Update()
    {
        if (photonView.IsMine )
        {
            Debug.Log("gameOver = " + gameOver);
            if (gameOver)
            {
                newGameButton.SetActive(true);
            }
            if (ARCam)
            {
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
                    
                    //if (flag)
                    //{
                    //    lineRenderer.SetPosition(0, branchTip.transform.position);
                    //    lineRenderer.SetPosition(1, transform.GetChild(1).position);
                    //    if (enableDots)
                    //    {
                    //        lineRenderer.enabled = false;
                    //        createDots(branchTip.transform.position, transform.GetChild(1).position);
                    //    }
                    //    else
                    //    {
                    //        lineRenderer.enabled = true;
                    //    }

                    //}


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
    }

    //void createDots(Vector3 start, Vector3 end)
    //{
    //    foreach (var d in dots)
    //    {
    //        Destroy(d);
    //    }
    //    dots.Clear();

    //    Vector3 distance = end - start;
    //    Vector3 step = distance * delta;

    //    Vector3 direction = (end - start).normalized;
    //    Vector3 pointer = start;

    //    while ((end - start).magnitude > (pointer - start).magnitude)
    //    {
    //        var g = GetOneDot();
    //        g.transform.position = pointer;
    //       // g.transform.forward = ARCam.transform.forward;
    //        dots.Add(g);
    //        pointer += (direction * delta);
    //    }
    //}
    //GameObject GetOneDot()
    //{
    //    var dotObject = new GameObject();
    //    dotObject.transform.localScale = Vector3.one * size;
    //    dotObject.transform.parent = transform;

    //    var sr = dotObject.AddComponent<SpriteRenderer>();
    //    sr.sprite = dot;
    //    return dotObject;
    //}
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
