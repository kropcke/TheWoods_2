using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ServerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    GameConfiguration variables;
    DistractionController distractionController;
    public static float gameAreaSize = 2f;

    // Bird variables
    public float magneticSpeed = 0.5f;
    public float collectionRadius = 0.25f;
    public int numAudioBubblesAtOneTime = 1;
    public string audioBubblePrefab;

    public List<GameObject> audioBubbles;
    List<Vector3> audioBubbleMovementVectors;
    List<Vector3> audioBubbleTargetPositions;
    public List<GameObject> holdingAudioClips;
    public float audioBubbleMoveSpeed = 0.02f;
    public List<int> audioNumbersLeft; // what is this?

    public Dictionary<GameObject, int> audioBubbleToAudioClipNumber;
    float distToChangeTarget = 0.25f; // for both cloud and bird

    public List<GameObject> audioVisualizations; // indirectly belong to bird

    // player variables
    GameObject lightening;
    GameObject[] players;
    bool lighteningAssigned = false;

    // gameplay/state/debug variables
    public float timeBetweenSpawns = 5f;
    float lastSpawnTime = 0;
    public int clipsCollected = 0;
    int totalClips = 8;
    bool startedGameOver = false;
    bool gameOver = false;
    bool debugMode = false;

    // distraction (clouds) variables
    public string[] distractionPrefabs;

    public AudioSource distractionAudioPlayer;
    public int numDistractionsAtOneTime = 1;
    int distractionsPickedUp = 0;
    List<GameObject> distractions;
    List<Vector3> distractionMoveVectors;
    List<Vector3> distractionTargetPositions;
    List<float> distractionSpawnTimes;
    List<float> distractionDestroyTimes;
    float distractionMoveSpeed = 0.3f;
    //public List<AudioClip> distractionSounds;

    // Shield variables
    GameObject shield;
    public GameObject ShieldPrefab;
    bool invincible = false;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;


    public void Awake()
    {
        if (photonView.IsMine)
        {
            Destroy(GameObject.Find("ARCamera"));
            LocalServerInstance = gameObject;
            lightening = GameObject.Find("Lightening");
            audioVisualizations = new List<GameObject>();
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("AudioVisualization").Length; i++)
            {
                audioVisualizations.Add(GameObject.FindGameObjectsWithTag("AudioVisualization")[i]);
            }
            distractions = new List<GameObject>();
            audioBubbles = new List<GameObject>();
            audioBubbleMovementVectors = new List<Vector3>();
            distractionMoveVectors = new List<Vector3>();
            audioBubbleTargetPositions = new List<Vector3>();
            distractionSpawnTimes = new List<float>();
            distractionDestroyTimes = new List<float>();
            holdingAudioClips = new List<GameObject>();
            distractionTargetPositions = new List<Vector3>();
            audioBubbleToAudioClipNumber = new Dictionary<GameObject, int>();
            audioNumbersLeft = new List<int>();

            for (int i = 0; i < audioVisualizations.Count; i++)
            {
                audioNumbersLeft.Add(i);
            }
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
            variables = GameObject.FindGameObjectWithTag("GameConfiguration").GetComponent<GameConfiguration>();
            distractionAudioPlayer = variables.audioSource;
            CreateDistraction();
            distractionController = GameObject.FindGameObjectWithTag("Clouds").GetComponent<DistractionController>();


            // todo: try to simulate two players in the game 

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

    private void debugDictionaryContent()
    {
        if (debugMode)
        {
            foreach (KeyValuePair<GameObject, int> val in audioBubbleToAudioClipNumber)
            {
                //Now you can access the key and value both separately from this attachStat as:
                Debug.LogFormat("{0} : {1}", val.Key, val.Value);
            }
        }
    }

    public void Update()
    {
        if (photonView.IsMine)
        {

            //debugDictionaryContent();
            players = GameObject.FindGameObjectsWithTag("Player");

            if (players.Length == 2)
            {
                players[0] = players[0].transform.GetChild(1).gameObject;
                players[1] = players[1].transform.GetChild(1).gameObject;
                if (lightening && !lighteningAssigned)
                {
                    lightening.SetActive(true);
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All,
                    lightening.transform.GetChild(0).GetChild(0).gameObject.name);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All,
                    lightening.transform.GetChild(0).GetChild(1).gameObject.name);
                    lighteningAssigned = true;
                    //lightening.transform.GetChild(0).GetChild(0).transform.position = players[0].transform.position;
                    //lightening.transform.GetChild(0).GetChild(1).transform.position = players[1].transform.position;
                }
            }
            else
            {
                if (lightening) lightening.SetActive(false);
                lighteningAssigned = false;
            }
            if (clipsCollected == totalClips)
            {
                if (!startedGameOver)
                {
                    startedGameOver = true;
                    StartCoroutine(endGameSequence());
                }
            }
            else
            {
                //UpdateDistractionsTimeout();
                //if (clipsCollected == (totalClips - audioNumbersLeft.Count))
                //{
                //Debug.LogError("Clips collected and to be played are not in sync.");
                //}
                UpdateSpawning();
                UpdateAudioClipMovement();
                UpdateAttachedAudioPosition();
                UpdateDistractionMovement();

                CheckAudioAttachPickup();
                CheckAudioPickups();

                DistractionPickedUp();
                MagneticAudioMovement();
                //UpdateInvicibility();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Reset();
            }
            if (Input.GetKeyDown(KeyCode.A) && audioBubbles.Count > 0)
            {
                AudioAttached(audioBubbles[0]);
            }
            if (Input.GetKeyDown(KeyCode.S) && holdingAudioClips.Count > 0)
            {
                AudioPickedUp(holdingAudioClips[0]);
            }
            if (Input.GetKeyDown(KeyCode.D) && holdingAudioClips.Count > 0)
            {
                audioNumbersLeft.Add(audioBubbleToAudioClipNumber[holdingAudioClips[0]]);
                audioBubbleToAudioClipNumber.Remove(holdingAudioClips[0]);
                StartCoroutine(NetworkedDestroyAudioBubbleAfter(holdingAudioClips[0], 0));
                holdingAudioClips.RemoveAt(0);
            }
        }
    }

    void UpdateSpawning()
    {
        if (
            audioBubbles.Count + holdingAudioClips.Count < numAudioBubblesAtOneTime
            && Time.time - lastSpawnTime > timeBetweenSpawns
            && audioNumbersLeft.Count > 0
            )
        {
            CreateAudioBubbleRandomClip();
            lastSpawnTime = Time.time;
        }
        if (distractions.Count < numDistractionsAtOneTime)
        {
            CreateDistraction();

        }
    }

    void UpdateAudioClipMovement()
    {

        for (int i = 0; i < audioBubbles.Count; i++)
        {
            audioBubbles[i].transform.position += audioBubbleMovementVectors[i];

            audioBubbleMovementVectors[i] = Vector3.Lerp(audioBubbleMovementVectors[i],
            (audioBubbleTargetPositions[i] - audioBubbles[i].transform.position).normalized * audioBubbleMoveSpeed,
                0.1f * Time.deltaTime);
            if (Vector3.Distance(audioBubbles[i].transform.position, audioBubbleTargetPositions[i]) < distToChangeTarget)
            {
                audioBubbleTargetPositions[i] = CreateNewRandomPosition();
            }
        }
    }

    void UpdateAttachedAudioPosition()
    {
        if (players.Length == 2)
        {
            for (int i = 0; i < holdingAudioClips.Count; i++)
            {
                Vector3 pos = Vector3.Lerp(players[0].transform.position, players[1].transform.position,
                (float)(i + 1) / (holdingAudioClips.Count + 1));
                holdingAudioClips[i].transform.position = pos;

            }
        }
    }


    void CheckAudioAttachPickup()
    {
        if (players.Length == 2)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            Ray ray = new Ray(players[0].transform.position, players[1].transform.position - players[0].transform.position);

            for (int i = 0; i < audioBubbles.Count; i++)
            {
                if (Vector3.Cross(ray.direction, audioBubbles[i].transform.position - ray.origin).magnitude < collectionRadius)
                {
                    AudioAttached(audioBubbles[i]);
                }
                /*
                if (Vector3.Distance(center, audioBubbles[i].transform.position) < collectionRadius)
                {
                    AudioPickedUp(audioBubbles[i]);
                }
                */
            }
        }
    }

    void CheckAudioPickups()
    {
        if (players.Length == 2)
        {
            for (int i = 0; i < holdingAudioClips.Count; i++)
            {
                //if (Vector3.Distance(players[0].transform.parent.position, players[1].transform.parent.position) < 0.25f && !VoicemailPlaying())
                //{
                    //Play the voice mail as soon as the bird is on teh branch
                    if (!VoicemailPlaying())
                    {
                    AudioPickedUp(holdingAudioClips[i]);
                    return;
                    
                }
            }
        }

    }

    //Brings bird to the center of the cylinder
    void MagneticAudioMovement()
    {
        if (players.Length == 2)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            Ray ray = new Ray(players[0].transform.position, players[1].transform.position - players[0].transform.position);

            for (int i = 0; i < audioBubbles.Count; i++)
            {
                if (Vector3.Cross(ray.direction, audioBubbles[i].transform.position - ray.origin).magnitude < collectionRadius)
                {
                    audioBubbles[i].transform.position +=
                        (center - audioBubbles[i].transform.position).normalized * magneticSpeed * Time.deltaTime;
                }
                /*
                if(Vector3.Distance(center, audioBubbles[i].transform.position) < magneticRadius)
                {
                    audioBubbles[i].transform.position +=
                        (center - audioBubbles[i].transform.position).normalized * magneticSpeed * Time.deltaTime;
                }
                */
            }
        }
    }

    bool VoicemailPlaying()
    {
        bool toReturn = false;
        for (int i = 0; i < audioVisualizations.Count; i++)
        {
            toReturn = toReturn || audioVisualizations[i].GetComponent<AudioSource>().isPlaying;
        }
        return toReturn;
    }

    void AudioPickedUp(GameObject audioBubble)
    {
        Debug.Log("Inside AudioPickedUp method");
        if (photonView.IsMine)
        {
            clipsCollected++;
            if (clipsCollected != totalClips)
            {
                if (audioBubbleToAudioClipNumber.ContainsKey(audioBubble))
                {
                    audioVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StartPlaying();
                    StartCoroutine(
                    NetworkedDestroyAudioBubbleAfter(audioBubble,
                        audioVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<AudioSource>().clip.length
                        )
                    );
                    audioBubbleToAudioClipNumber.Remove(audioBubble);
                    holdingAudioClips.Remove(audioBubble);
                    audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "singing");
                }

            }
            else
            {
                audioBubbleToAudioClipNumber.Remove(audioBubble);
                holdingAudioClips.Remove(audioBubble);
                PhotonNetwork.Destroy(audioBubble);

            }
        }
    }

    void AudioAttached(GameObject audioBubble)
    {
        if (photonView.IsMine)
        {
            holdingAudioClips.Add(audioBubble);
            int spot = audioBubbles.IndexOf(audioBubble);
            audioBubbles.Remove(audioBubble);
            audioBubbleMovementVectors.RemoveAt(spot);
            audioBubbleTargetPositions.RemoveAt(spot);
            audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "perched");
        }
    }

    void CreateAudioBubbleRandomClip()
    {
        if (photonView.IsMine)
        {
            int clipNumber = audioNumbersLeft[(int)Random.Range(0, audioNumbersLeft.Count)];
            GameObject bubble = PhotonNetwork.Instantiate(audioBubblePrefab, CreateNewRandomPosition(), Quaternion.identity);
            audioNumbersLeft.Remove(clipNumber);
            audioBubbles.Add(bubble);
            audioBubbleToAudioClipNumber.Add(bubble, clipNumber);
            audioBubbleMovementVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f)).normalized * audioBubbleMoveSpeed);
            audioBubbleTargetPositions.Add(CreateNewRandomPosition());
            bubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "flying");
        }
    }

    Vector3 CreateNewRandomPosition()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        float dist = Random.Range(0, gameAreaSize);
        float randomHeight = Random.Range(0.5f, 1.5f);
        return new Vector3(dist * Mathf.Cos(angle), randomHeight, dist * Mathf.Sin(angle));
    }

    // Distractions code below
    void DistractionPickedUp()
    {
        Debug.LogFormat("In Distraction {0}", DistractionController.inDistraction);
        //distractionController.GetComponent<PhotonView>().RPC("IsDistractionActive1", RpcTarget.All);
        if (debugMode)
        {
            Debug.LogFormat("Method: DistractionPickedUp with value: {0}", DistractionController.inDistraction);
            Debug.LogFormat("Variable: DistractionPickedUp with value: {0}", DistractionController.inDistraction);
        }
        if (photonView.IsMine && DistractionController.inDistraction && holdingAudioClips.Count > 0)
        {
            Debug.Log("Inside Distraction picked up");
            GameObject audioBubble = holdingAudioClips[0];
            if (audioBubbleToAudioClipNumber.ContainsKey(audioBubble))
            {
                audioVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StopPlaying();
                audioNumbersLeft.Add(audioBubbleToAudioClipNumber[audioBubble]);
                audioBubbleToAudioClipNumber.Remove(audioBubble);
                //StartCoroutine(NetworkedDestroyAudioBubbleAfter(audioBubble, 2));
                PhotonNetwork.Destroy(audioBubble);
                Debug.Log("Bird object destroyed!");
                //if (holdingAudioClips[0] == audioBubble)
                //{
                holdingAudioClips.RemoveAt(0);
                //}
            }

        }
    }


    void CreateDistraction()
    {
        GameObject d = PhotonNetwork.Instantiate(distractionPrefabs[Random.Range(0, distractionPrefabs.Length)]
        , CreateNewRandomPosition(), Quaternion.identity);
        distractions.Add(d);
        float angle = Random.Range(0, 2 * Mathf.PI);
        float dist = 2 * gameAreaSize;
        float randomHeight = Random.Range(0.5f, 1.5f);
        d.transform.position = new Vector3(dist * Mathf.Cos(angle), randomHeight, dist * Mathf.Sin(angle));
        d.transform.LookAt(Vector3.zero + new Vector3(0, randomHeight, 0));
        distractionTargetPositions.Add(new Vector3(-dist * Mathf.Cos(angle), randomHeight, -dist * Mathf.Sin(angle)));
        distractionMoveVectors.Add((distractionTargetPositions[distractionTargetPositions.Count - 1]
        - d.transform.position).normalized * distractionMoveSpeed * Time.deltaTime);
    }


    void UpdateDistractionsTimeout()
    {
        for (int i = 0; i < distractions.Count; i++)
        {
            if (Time.time > distractionDestroyTimes[i])
            {
                PhotonNetwork.Destroy(distractions[i]);
                distractionSpawnTimes.RemoveAt(i);
                distractionTargetPositions.RemoveAt(i);
                distractionMoveVectors.RemoveAt(i);
                distractions.RemoveAt(i);
                distractionDestroyTimes.RemoveAt(i);
                i--;
            }
        }
    }

    void UpdateDistractionMovement()
    {
        for (int i = 0; i < distractions.Count; i++)
        {
            if (invincible)
            {
                //if(Vector3.Distance(shield.transform.position, distractions[i].transform.position) < shield.transform.localScale.x / 2f)
                //{
                //    distractionMoveVectors[i] = (distractions[i].transform.position - shield.transform.position).normalized
                //    * moveSpeed * 1f;
                //}
            }
            distractions[i].transform.position += distractionMoveVectors[i];

            //distractionMoveVectors[i] = Vector3.Lerp(distractionMoveVectors[i],
            //(distractionTargetPositions[i] - distractions[i].transform.position).normalized * moveSpeed / 2f,
            //    0.5f * Time.deltaTime);
            //if (Vector3.Distance(distractions[i].transform.position, distractionTargetPositions[i]) < distToChangeTarget)
            //{
            //    distractionTargetPositions[i] = CreateNewRandomPosition();
            //}
            if (Vector3.Distance(distractions[i].transform.position, distractionTargetPositions[i]) < distToChangeTarget)
            {
                distractionMoveVectors.RemoveAt(i);
                distractionTargetPositions.RemoveAt(i);
                PhotonNetwork.Destroy(distractions[i]);
                distractions.RemoveAt(i);
            }

        }
    }


    // ignore below functions for now.
    void Reset()
    {
        distractionAudioPlayer.Stop();
        for (int j = 0; j < audioVisualizations.Count; j++)
        {
            audioVisualizations[j].GetComponent<AudioSource>().Stop();
        }
        while (distractions.Count > 0)
        {
            PhotonNetwork.Destroy(distractions[0]);
            distractions.RemoveAt(0);
        }
        while (audioBubbles.Count > 0)
        {
            PhotonNetwork.Destroy(audioBubbles[0]);
            audioBubbles.RemoveAt(0);
        }
        while (holdingAudioClips.Count > 0)
        {
            PhotonNetwork.Destroy(holdingAudioClips[0]);
            holdingAudioClips.RemoveAt(0);
        }
        for (int i = 0; i < audioVisualizations.Count; i++)
        {
            audioVisualizations[i].GetComponent<SoundClipGameObjects>().Reset();
        }
        distractions = new List<GameObject>();
        audioBubbles = new List<GameObject>();
        audioBubbleMovementVectors = new List<Vector3>();
        distractionMoveVectors = new List<Vector3>();
        audioBubbleTargetPositions = new List<Vector3>();
        distractionSpawnTimes = new List<float>();
        distractionDestroyTimes = new List<float>();
        holdingAudioClips = new List<GameObject>();
        distractionTargetPositions = new List<Vector3>();
        audioBubbleToAudioClipNumber = new Dictionary<GameObject, int>();
        audioNumbersLeft = new List<int>();
        for (int i = 0; i < audioVisualizations.Count; i++)
        {
            audioNumbersLeft.Add(i);
        }
        clipsCollected = 0;
        distractionsPickedUp = 0;
        startedGameOver = false;
        gameOver = false;
    }

    IEnumerator endGameSequence()
    {
        distractionAudioPlayer.Stop();
        for (int j = 0; j < audioVisualizations.Count; j++)
        {
            audioVisualizations[j].GetComponent<AudioSource>().Stop();
        }
        while (distractions.Count > 0)
        {
            PhotonNetwork.Destroy(distractions[0]);
            distractions.RemoveAt(0);
        }
        while (audioBubbles.Count > 0)
        {
            PhotonNetwork.Destroy(audioBubbles[0]);
            audioBubbles.RemoveAt(0);
        }
        int i = 0;
        bool playing = false;
        while (i < audioVisualizations.Count)
        {
            if (playing)
            {
                if (!audioVisualizations[i].GetComponent<AudioSource>().isPlaying)
                {
                    playing = false;
                    i++;
                }
            }
            else
            {
                audioVisualizations[i].GetComponent<SoundClipGameObjects>().StartPlaying();
                playing = true;
            }
            yield return null;
        }
        gameOver = true;
    }


    IEnumerator NetworkedDestroyAudioBubbleAfter(GameObject audioBubble, float t)
    {
        yield return new WaitForSeconds(t);
        //if (audioBubbleToAudioClipNumber.ContainsKey(audioBubble))
        //{
        //audioVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StopPlaying();
        //}
        if (audioBubble != null)
        {
            PhotonNetwork.Destroy(audioBubble);
        }
        
    }

    IEnumerator AudioPickedUpPlayAfter(GameObject audioBubble, float t)
    {
        yield return new WaitForSeconds(t);
        AudioPickedUp(audioBubble);
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
