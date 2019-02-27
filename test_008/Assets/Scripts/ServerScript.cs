using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    public static float gameAreaSize = 10f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;

    GameObject lightening;

    int clipsCollected = 0;
    int totalClips = 10;

    List<GameObject> audioVisualizations;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalServerInstance = gameObject;
            lightening = GameObject.Find("Lightening");
            audioVisualizations = new List<GameObject>();
            for(int i = 0; i < GameObject.FindGameObjectsWithTag("AudioVisualization").Length; i++)
            {
                audioVisualizations.Add(GameObject.FindGameObjectsWithTag("AudioVisualization")[i]);
            }
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
        if (photonView.IsMine)
        {
            lightening.GetComponent<PhotonView>().RequestOwnership();

            lightening.transform.GetChild(0).GetChild(0).GetComponent<PhotonView>().RequestOwnership();
            lightening.transform.GetChild(0).GetChild(1).GetComponent<PhotonView>().RequestOwnership();

            for (int i = 0; i < audioVisualizations.Count; i++)
            {
                audioVisualizations[i].GetComponent<SoundClipGameObjects>().Init();
                audioVisualizations[i].GetComponent<SoundClipGameObjects>().PositionInCircle(i * 8, 64, gameAreaSize);
            }
        }
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                audioVisualizations[7].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
        }
    }

    void ProcessInputs()
    {

    }

    void AudioPickedUp()
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
