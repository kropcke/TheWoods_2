using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    public static float gameAreaSize = 2f;
    public float magneticRadius = 1;
    public float magneticSpeed = 0.5f;
    public float collectionRadius = 0.25f;
    public float timeBetweenSpawns = 5f;
    public int numAudioBubblesAtOneTime = 2;
    public int numDistractionsAtOneTime = 5;
    public GameObject audioBubblePrefab, distractionPrefab;
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;

    GameObject lightening;

    int distractionsPickedUp = 0;
    int clipsCollected = 0;
    int totalClips = 8;

    float lastSpawnTime = 0;

    List<GameObject> audioVisualizations;

    List<GameObject> distractions;
    List<GameObject> audioBubbles;
    List<Vector3> movementVectors;
    List<Vector3> distractionMoveVectors;
    List<Vector3> targetPositions;
    List<Vector3> distractionTargetPositions;
    float distToChangeTarget = 0.25f;
    float moveSpeed = 0.001f;
    List<int> audioNumbersLeft;

    Dictionary<GameObject, int> audioBubbleToAudioClipNumber;
    GameObject[] players;
    public void Awake()
    {
        if (photonView.IsMine)
        {
            Destroy(GameObject.Find("ARCamera"));
            LocalServerInstance = gameObject;
            lightening = GameObject.Find("Lightening");
            audioVisualizations = new List<GameObject>();
            for(int i = 0; i < GameObject.FindGameObjectsWithTag("AudioVisualization").Length; i++)
            {
                audioVisualizations.Add(GameObject.FindGameObjectsWithTag("AudioVisualization")[i]);
            }
            distractions = new List<GameObject>();
            audioBubbles = new List<GameObject>();
            movementVectors = new List<Vector3>();
            distractionMoveVectors = new List<Vector3>();
            targetPositions = new List<Vector3>();
            distractionTargetPositions = new List<Vector3>();
            audioBubbleToAudioClipNumber = new Dictionary<GameObject, int>();
            audioNumbersLeft = new List<int>();
            for(int i = 0; i < audioVisualizations.Count; i++)
            {
                audioNumbersLeft.Add(i);
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
                audioVisualizations[i].GetComponent<SoundClipGameObjects>().PositionInCircle(i * totalClips, 64, gameAreaSize);
            }
        }
    }

    public void Update()
    {
        if (photonView.IsMine)
        {
            players = GameObject.FindGameObjectsWithTag("Player");

            if (players.Length == 2)
            {
                if (lightening)
                {
                    lightening.SetActive(true);
                    lightening.transform.GetChild(0).GetChild(0).transform.position = players[0].transform.position;
                    lightening.transform.GetChild(0).GetChild(1).transform.position = players[1].transform.position;
                }
            }
            else
            {
                if(lightening) lightening.SetActive(false);
            }
            UpdateSpawning();
            UpdateAudioClipMovement();
            UpdateDistractionMovement();
            CheckAudioPickups();
            CheckDistractionPickups();
            MagneticAudioMovement();
            // Debug stuff that can be commented out eventually.
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                audioVisualizations[0].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                audioVisualizations[1].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                audioVisualizations[2].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                audioVisualizations[3].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                audioVisualizations[4].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                audioVisualizations[5].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                audioVisualizations[6].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                audioVisualizations[7].GetComponent<SoundClipGameObjects>().StartPlaying();
            }
        }
    }

    void UpdateAudioClipMovement()
    {

        for(int i = 0; i < audioBubbles.Count; i++)
        {
            audioBubbles[i].transform.position += movementVectors[i];

            movementVectors[i] = Vector3.Lerp(movementVectors[i],
            (targetPositions[i] - audioBubbles[i].transform.position).normalized * moveSpeed,
                0.1f * Time.deltaTime);
            if (Vector3.Distance(audioBubbles[i].transform.position, targetPositions[i]) < distToChangeTarget)
            {
                targetPositions[i] = CreateNewRandomPosition();
            }
        }
    }
    void UpdateDistractionMovement()
    {
        for (int i = 0; i < distractions.Count; i++)
        {
            distractions[i].transform.position += distractionMoveVectors[i];

            distractionMoveVectors[i] = Vector3.Lerp(distractionMoveVectors[i],
            (distractionTargetPositions[i] - distractions[i].transform.position).normalized * moveSpeed / 2f,
                0.1f * Time.deltaTime);
            if (Vector3.Distance(distractions[i].transform.position, distractionTargetPositions[i]) < distToChangeTarget)
            {
                distractionTargetPositions[i] = CreateNewRandomPosition();
            }
        }
    }
    void UpdateSpawning()
    {
        if(audioBubbles.Count < numAudioBubblesAtOneTime && Time.time - lastSpawnTime > timeBetweenSpawns
        && clipsCollected < totalClips)
        {
            CreateAudioBubbleRandomClip();
            lastSpawnTime = Time.time;
        }
        if(distractions.Count < numDistractionsAtOneTime)
        {
            CreateDistraction();
        
        }
    }

    void MagneticAudioMovement()
    {
        if (players.Length == 2)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) *0.5f;
            for(int i = 0; i < audioBubbles.Count; i++)
            {
                if(Vector3.Distance(center, audioBubbles[i].transform.position) < magneticRadius)
                {
                    audioBubbles[i].transform.position +=
                        (center - audioBubbles[i].transform.position).normalized * magneticSpeed * Time.deltaTime;
                }
            }
        }
    }
    void CheckAudioPickups()
    {
        if(players.Length == 2) {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            for (int i = 0; i < audioBubbles.Count; i++)
            {
                if (Vector3.Distance(center, audioBubbles[i].transform.position) < collectionRadius)
                {
                    AudioPickedUp(audioBubbles[i]);
                }
            }
        }

    }
    void CheckDistractionPickups()
    {
        if (players.Length == 2)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            for (int i = 0; i < distractions.Count; i++)
            {
                if (Vector3.Distance(center, distractions[i].transform.position) < collectionRadius)
                {
                    DistractionPickedUp(distractions[i]);
                }
            }
        }
    }

    void DistractionPickedUp(GameObject distraction)
    {
        if (photonView.IsMine)
        {
            distractionsPickedUp++;
            //Do some other stuff? Play scary noise?
            int spot = distractions.IndexOf(distraction);
            distractions.Remove(distraction);
            distractionMoveVectors.RemoveAt(spot);
            distractionTargetPositions.RemoveAt(spot);
            PhotonNetwork.Destroy(distraction);
        }
    }

    void AudioPickedUp(GameObject audioBubble)
    {
        if (photonView.IsMine)
        {
            clipsCollected++;
            audioVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StartPlaying();

            audioBubbleToAudioClipNumber.Remove(audioBubble);
            int spot = audioBubbles.IndexOf(audioBubble);
            audioBubbles.Remove(audioBubble);
            movementVectors.RemoveAt(spot);
            targetPositions.RemoveAt(spot);
            PhotonNetwork.Destroy(audioBubble);
        }
    }
    void CreateAudioBubbleRandomClip()
    {

        int clipNumber = audioNumbersLeft[(int)Random.Range(0, audioNumbersLeft.Count - 1)];
        GameObject bubble = PhotonNetwork.Instantiate("AudioBubblePrefab", CreateNewRandomPosition(), Quaternion.identity);
        audioNumbersLeft.Remove(clipNumber);
        audioBubbles.Add(bubble);
        audioBubbleToAudioClipNumber.Add(bubble, clipNumber);
        movementVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f)).normalized * moveSpeed);
        targetPositions.Add(CreateNewRandomPosition());
        
    }
    void CreateDistraction()
    {
        GameObject d = PhotonNetwork.Instantiate("DistractionPrefab", CreateNewRandomPosition(), Quaternion.identity);
        distractions.Add(d);
        distractionMoveVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f)).normalized * moveSpeed / 2f);
        distractionTargetPositions.Add(CreateNewRandomPosition());
    }
    Vector3 CreateNewRandomPosition()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        float dist = Random.Range(0, gameAreaSize);
        float randomHeight = Random.Range(0.2f, 1.25f);
        return new Vector3(dist * Mathf.Cos(angle), randomHeight, dist * Mathf.Sin(angle));
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
