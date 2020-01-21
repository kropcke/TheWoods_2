using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using UnityEngine.Animations;

public class ServerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    GameConfiguration variables;
    public static float gameAreaSize = 2f;

    // Bird variables
    public float magneticSpeed = 0.5f;
    public float collectionRadius = 0.25f;
    public int audioBubblesFlyingAtOneTime = 1;
    public string audioBubblePrefab;

    public List<GameObject> audioBubblesFlying;
    public List<GameObject> audioBubblesPlayed;
    public HashSet<GameObject> audioBubblesNowPlaying;

    List<Vector3> audioBubbleMovementVectors;
    List<Vector3> audioBubbleTargetPositions;
    public List<GameObject> audioBubblesHolding;
    public float audioBubbleMoveSpeed = 0.02f;
    public float smoothSpeed = 0.02f;
    public List<int> audioBubblesLeft; // what is this?

    public Dictionary<GameObject, int> audioBubbleToAudioClipNumber;
    public int audioBubblestotalCount = 4;
    public int audioBubblesCollected = 0;
    float distToChangeTarget = 0.55f; // for both cloud and bird


    public List<GameObject> audioBubbleVisualizations; // indirectly belong to bird


    // player variables
    GameObject lightening;
    public GameObject[] players;
    bool lighteningAssigned = false;

    // gameplay/state/debug variables
    public float timeBetweenSpawns = 5f;
    float lastSpawnTime = 0;

    bool startedGameOver = false;
    bool gameOver = false;
    bool debugMode = false; // // todo: Move to gamecontroller.
    bool simulatePlayers = false;

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
    private GameObject middleBranch;


    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            Destroy(GameObject.Find("ARCamera"));
            LocalServerInstance = gameObject;
            lightening = GameObject.Find("Lightening");
            audioBubbleVisualizations = new List<GameObject>();
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("AudioVisualization").Length; i++)
            {
                audioBubbleVisualizations.Add(GameObject.FindGameObjectsWithTag("AudioVisualization")[i]);
            }

            audioBubblesFlying = new List<GameObject>();
            audioBubblesNowPlaying = new HashSet<GameObject>();
            audioBubblesPlayed = new List<GameObject>();
            audioBubbleMovementVectors = new List<Vector3>();
            audioBubbleTargetPositions = new List<Vector3>();
            audioBubblesHolding = new List<GameObject>();
            audioBubbleToAudioClipNumber = new Dictionary<GameObject, int>();
            audioBubblesLeft = new List<int>();
            for (int i = 0; i < audioBubbleVisualizations.Count; i++)
            {
                audioBubblesLeft.Add(i);
            }

            distractions = new List<GameObject>();
            distractionMoveVectors = new List<Vector3>();
            distractionSpawnTimes = new List<float>();
            distractionDestroyTimes = new List<float>();
            distractionTargetPositions = new List<Vector3>();


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
            middleBranch = GameObject.Find("NewMiddleBranch");
            variables = GameObject.FindGameObjectWithTag("GameConfiguration").GetComponent<GameConfiguration>();
            distractionAudioPlayer = variables.audioSource;

            lightening.GetComponent<PhotonView>().RequestOwnership();
            lightening.transform.GetChild(0).GetChild(0).GetComponent<PhotonView>().RequestOwnership();
            lightening.transform.GetChild(0).GetChild(1).GetComponent<PhotonView>().RequestOwnership();

            for (int i = 0; i < audioBubbleVisualizations.Count; i++)
            {
                audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().Init();
                audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().PositionInCircle(4 * i * audioBubblestotalCount, 64, gameAreaSize);
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


    void simulateTwoPlayers()
    {
        GameObject[] playersInScene = GameObject.FindGameObjectsWithTag("Player");
        if (playersInScene.Length == 0)
        {
            PhotonNetwork.Instantiate("Player", new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity);
            PhotonNetwork.Instantiate("Player", new Vector3(-0.5f, 0.5f, 0.5f), Quaternion.identity);
        }
    }

    public void Update()
    {
        if (photonView.IsMine)
        {
            debugDictionaryContent();

            if (simulatePlayers)
            {
                simulateTwoPlayers();

            }
            players = GameObject.FindGameObjectsWithTag("Player");

            if (players.Length == 2)
            {
                players[0] = players[0].transform.GetChild(1).gameObject;
                players[1] = players[1].transform.GetChild(1).gameObject;
                if (simulatePlayers)
                {
                    GameObject.Find("LightningStart").transform.position = players[0].transform.position;
                    GameObject.Find("LightningEnd").transform.position = players[1].transform.position;
                }
                else
                {
                    if (lightening && !lighteningAssigned)
                    {
                        lightening.SetActive(true);
                        players[0].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All,
                        lightening.transform.GetChild(0).GetChild(0).gameObject.name);
                        players[1].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All,
                        lightening.transform.GetChild(0).GetChild(1).gameObject.name);
                        lighteningAssigned = true;


                    }

                }

            }
            else
            {
                if (lightening)
                {
                    lightening.SetActive(false);

                }
                lighteningAssigned = false;

            }

            if (audioBubblesCollected == audioBubblestotalCount)
            {
                if (!startedGameOver)
                {
                    startedGameOver = true;
                    StartCoroutine(endGameSequence());
                }
            }
            else
            {
                UpdateSpawning();
                UpdateAudioClipMovement();
                UpdateAttachedAudioPosition();
                UpdateDistractionMovement();

                //CheckAudioAttachPickup();
                CheckAudioPickups();

                DistractionPickedUp();
                MagneticAudioMovement();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Reset();
            }
            if (Input.GetKeyDown(KeyCode.A) && audioBubblesFlying.Count > 0)
            {
                AudioAttached(audioBubblesFlying[0]);
            }
            if (Input.GetKeyDown(KeyCode.S) && audioBubblesHolding.Count > 0)
            {
                AudioPickedUp(audioBubblesHolding[0]);
            }
            if (Input.GetKeyDown(KeyCode.D) && audioBubblesHolding.Count > 0)
            {
                audioBubblesLeft.Add(audioBubbleToAudioClipNumber[audioBubblesHolding[0]]);
                audioBubbleToAudioClipNumber.Remove(audioBubblesHolding[0]);
                StartCoroutine(NetworkedDestroyAudioBubbleAfter(audioBubblesHolding[0], 0));
                audioBubblesHolding.RemoveAt(0);
            }
        }
    }

    void UpdateSpawning()
    {
        if (
            audioBubblesFlying.Count + audioBubblesHolding.Count < audioBubblesFlyingAtOneTime
            && Time.time - lastSpawnTime > timeBetweenSpawns
            && audioBubblesLeft.Count > 0
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

        for (int i = 0; i < audioBubblesFlying.Count; i++)
        {
            /**
             * Modified bird motion.
             * Bird now points in the direction of its motion (usually).
            */

            Vector3 start = audioBubblesFlying[i].transform.position;
            Vector3 end = audioBubbleTargetPositions[i];
            Debug.DrawLine(start, end, Color.white);


            // Record the last position.
            Vector3 last = audioBubblesFlying[i].transform.position;

            // This line actually moves the bird.
            // audioBubblesFlying[i].transform.position = Vector3.MoveTowards(start, end,  0.4f * Time.deltaTime);
            audioBubblesFlying[i].transform.position += audioBubbleMovementVectors[i];

            // Record the current position.
            Vector3 current = audioBubblesFlying[i].transform.position;

            // Point the bird in the direction it is going.
            float angle = Vector3.Angle(last, current);
            // Debug.LogFormat("last: ({0},{1},{2}), current: ({3},{4},{5})", last.x, last.y, last.z, current.x, current.y, current.z);


            // The velocity is the difference between the new position and the last position.
            Vector3 velocity = last - current;
            // Debug.LogFormat("velocity: ({0},{1},{2})", velocity.x, velocity.y, velocity.z);
            velocity.y = 0;

            // The vector direction is the normalized velocity.
            Vector3 direction = velocity.normalized;

            // Offset the forward direction by a normalized unit vector.
            float worldDegrees = Vector3.Angle(new Vector3(0, 0, 1), direction);
            float localDegrees = Vector3.Angle(audioBubblesFlying[i].transform.forward, direction); // angle relative to last heading of GameObject
            // Debug.LogFormat("worldDegrees: {0}", worldDegrees);
            // Debug.LogFormat("localDegrees: {0}", localDegrees);

            audioBubblesFlying[i].transform.rotation = Quaternion.Euler(
                audioBubblesFlying[i].transform.rotation.eulerAngles.x,
                worldDegrees + 180,
                audioBubblesFlying[i].transform.rotation.eulerAngles.z
            );


            // "Magnetic" effect.
            audioBubbleMovementVectors[i] = Vector3.Lerp(
                audioBubbleMovementVectors[i],
                (audioBubbleTargetPositions[i] - audioBubblesFlying[i].transform.position).normalized * audioBubbleMoveSpeed,
                0.1f * Time.deltaTime
            );


            // When the bird gets close to the goal position, generate a new position to move towards.
            if (Vector3.Distance(audioBubblesFlying[i].transform.position, audioBubbleTargetPositions[i]) < distToChangeTarget)
            {
                audioBubbleTargetPositions[i] = CreateNewRandomPosition();
                // print("new position: " + audioBubbleTargetPositions[i].ToString());
            }
        }
    }


    void UpdateAttachedAudioPosition()
    {
        if (players.Length == 2)
        {

            for (int i = 0; i < audioBubblesHolding.Count; i++)
            {
                //audioBubblesHolding[i].SetActive(false);

                Vector3 pos = Vector3.Lerp(
                        players[0].transform.position,
                        players[1].transform.position,
                        (float)(i + 1) / (audioBubblesHolding.Count + 1)
                        );
                audioBubblesHolding[i].transform.position = pos;
            }
            for (int i = 0; i < audioBubblesPlayed.Count; i++)
            {
                audioBubblesPlayed[i].SetActive(false);
            }
        }
    }

    void CheckAudioAttachPickup()
    {
        if (players.Length == 2)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            Ray ray = new Ray(players[0].transform.position, players[1].transform.position - players[0].transform.position);

            for (int i = 0; i < audioBubblesFlying.Count; i++)
            {
                if (Vector3.Cross(ray.direction, audioBubblesFlying[i].transform.position - ray.origin).magnitude < collectionRadius)
                {
                    AudioAttached(audioBubblesFlying[i]);
                }
            }
        }
    }

    void CheckAudioPickups()
    {
        if (players.Length == 2)
        {
            for (int i = 0; i < audioBubblesHolding.Count; i++)
            {
                //Play the voice mail as soon as the bird is on the branch
                if (!VoicemailPlaying() && !audioBubblesNowPlaying.Contains(audioBubblesHolding[i]))
                {
                    audioBubblesNowPlaying.Add(audioBubblesHolding[i]);

                    // Vector3 pos = Vector3.Lerp(
                    //players[0].transform.position,
                    //players[1].transform.position,
                    //(float)(i + 1) / (audioBubblesHolding.Count + 1)
                    //);
                    // audioBubblesHolding[i].transform.position = pos;
                    AudioPickedUp(audioBubblesHolding[i]);

                    return;

                }
            }
            //foreach(GameObject playingbird in audioBubblesNowPlaying)
            //{
            //    playingbird.SetActive(false);

            //}
        }

    }

    //Brings bird to the center of the cylinder
    void MagneticAudioMovement()
    {
        if (players.Length == 2)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            Ray ray = new Ray(players[0].transform.position, players[1].transform.position - players[0].transform.position);

            for (int i = 0; i < audioBubblesFlying.Count; i++)
            {
                if (Vector3.Cross(ray.direction, audioBubblesFlying[i].transform.position - ray.origin).magnitude < collectionRadius)
                {
                    audioBubblesFlying[i].transform.position +=
                        (center - audioBubblesFlying[i].transform.position).normalized * magneticSpeed * Time.deltaTime;
                    AudioAttached(audioBubblesFlying[i]);

                }
            }
        }
    }

    bool VoicemailPlaying()
    {
        bool toReturn = false;
        for (int i = 0; i < audioBubbleVisualizations.Count; i++)
        {
            toReturn = toReturn || audioBubbleVisualizations[i].GetComponent<AudioSource>().isPlaying;
        }
        return toReturn;
    }

    void AudioPickedUp(GameObject audioBubble)
    {
        if (photonView.IsMine)
        {
            if (audioBubblesCollected <= audioBubblestotalCount)
            {
                if (audioBubbleToAudioClipNumber.ContainsKey(audioBubble))
                {
                    audioBubbleVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StartPlaying(false);
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                       RpcTarget.All, audioBubbleVisualizations[audioBubbleToAudioClipNumber[audioBubble]].name);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                       RpcTarget.All, audioBubbleVisualizations[audioBubbleToAudioClipNumber[audioBubble]].name);



                    StartCoroutine(
                    NetworkedAudioBubblePlayedAfter(audioBubble,
                        audioBubbleVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<AudioSource>().clip.length
                        )
                    );

                    audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "singing");
                }
            }
        }
    }

    void AudioAttached(GameObject audioBubble)
    {
        if (photonView.IsMine)
        {
            audioBubblesHolding.Add(audioBubble);
            int spot = audioBubblesFlying.IndexOf(audioBubble);
            audioBubblesFlying.Remove(audioBubble);
            audioBubbleMovementVectors.RemoveAt(spot);
            audioBubbleTargetPositions.RemoveAt(spot);
            audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "perched");


        }
    }

    void CreateAudioBubbleRandomClip()
    {
        if (photonView.IsMine)
        {
            int clipNumber = audioBubblesLeft[(int)Random.Range(0, audioBubblesLeft.Count)];
            GameObject bubble = PhotonNetwork.Instantiate(audioBubblePrefab, CreateNewRandomPosition(), Quaternion.identity);
            audioBubblesLeft.Remove(clipNumber);
            audioBubblesFlying.Add(bubble);
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
        if (debugMode)
        {
            Debug.LogFormat("In Distraction {0}", DistractionController.inDistraction);
            Debug.LogFormat("Method: DistractionPickedUp with value: {0}", DistractionController.inDistraction);
            Debug.LogFormat("Variable: DistractionPickedUp with value: {0}", DistractionController.inDistraction);
        }
        if (
            photonView.IsMine
            && DistractionController.inDistraction
            && (audioBubblesHolding.Count > 0 || audioBubblesNowPlaying.Count > 0 || audioBubblesPlayed.Count > 0)
            )
        {

            //players[0].transform.parent.GetComponent<PhotonView>().RPC("DisableBirds",
            //              RpcTarget.All, null);
            //players[1].transform.parent.GetComponent<PhotonView>().RPC("DisableBirds",
            //          RpcTarget.All, null);
            //for (int i = 0; i < audioBubbleVisualizations.Count; i++)
            //{
            //            players[0].transform.parent.GetComponent<PhotonView>().RPC("StopVoiceMail",
            //                   RpcTarget.All, audioBubbleVisualizations[i].name);
            //            players[1].transform.parent.GetComponent<PhotonView>().RPC("StopVoiceMail",
            //                   RpcTarget.All, audioBubbleVisualizations[i].name);
            //}
            FlyawayAndDestroyBird();
            ResetAudioBubbleState();
        }
    }


    void CreateDistraction()
    {
        if (players.Length == 2)
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

            d.GetComponent<AudioSource>().enabled = true;

        }
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
            distractions[i].transform.position += distractionMoveVectors[i];

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
        ResetAudioBubbleState();
        ResetDistractionState();
    }


    void FlyawayAndDestroyBird()
    {
        if (debugMode)
        {
            Debug.Log("Flyaway audio bubbles.");
        }

        for (int i = 0; i < audioBubblesPlayed.Count; i++)
        {

            //audioBubblesPlayed[i].SetActive(true);
            audioBubblesPlayed[i].GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "flying");
            Vector3 currentPos = audioBubblesPlayed[i].transform.position;
            Vector3 newPos = new Vector3(currentPos.x + 1, currentPos.y + 2, currentPos.z + 2);
            audioBubblesPlayed[i].transform.position = Vector3.Lerp(currentPos, newPos, Time.deltaTime * 1f);

        }

        for (int i = 0; i < audioBubblesHolding.Count; i++)
        {
            audioBubblesHolding[i].GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "flying");
            Vector3 currentPos = audioBubblesHolding[i].transform.position;
            Vector3 newPos = new Vector3(currentPos.x + 1, currentPos.y + 2, currentPos.z + 2);
            audioBubblesHolding[i].transform.position = Vector3.Lerp(currentPos, newPos, Time.deltaTime * 1f);
        }

        //ResetAudioBubbleState();
    }

    void ResetAudioBubbleState()

    {
        for (int i = 0; i < audioBubblesPlayed.Count; i++)
        {
            middleBranch.transform.GetChild(i).gameObject.SetActive(false);
        }
        if (players.Length == 2)
        {
            players[0].transform.parent.GetComponent<PhotonView>().RPC("DisableBirds",
                                  RpcTarget.All, "DisableBirds");
            players[1].transform.parent.GetComponent<PhotonView>().RPC("DisableBirds",
                      RpcTarget.All, "DisableBirds");
            for (int i = 0; i < audioBubbleVisualizations.Count; i++)
            {
                players[0].transform.parent.GetComponent<PhotonView>().RPC("StopVoiceMail",
                       RpcTarget.All, audioBubbleVisualizations[i].name);
                players[1].transform.parent.GetComponent<PhotonView>().RPC("StopVoiceMail",
                       RpcTarget.All, audioBubbleVisualizations[i].name);
            }
        }
        if (debugMode)
        {
            Debug.Log("Reset audio bubble state.");
        }

        audioBubblesLeft = new List<int>();

        while (audioBubblesPlayed.Count > 0)
        {
            PhotonNetwork.Destroy(audioBubblesPlayed[0]);
            audioBubblesPlayed.RemoveAt(0);
        }
        while (audioBubblesFlying.Count > 0)
        {
            PhotonNetwork.Destroy(audioBubblesFlying[0]);
            audioBubblesFlying.RemoveAt(0);
        }
        while (audioBubblesHolding.Count > 0)
        {
            PhotonNetwork.Destroy(audioBubblesHolding[0]);
            audioBubblesHolding.RemoveAt(0);
        }
        for (int i = 0; i < audioBubbleVisualizations.Count; i++)
        {
            audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().Reset();
        }
        audioBubblesFlying = new List<GameObject>();
        audioBubblesNowPlaying = new HashSet<GameObject>();
        audioBubbleMovementVectors = new List<Vector3>();
        audioBubbleTargetPositions = new List<Vector3>();
        audioBubblesHolding = new List<GameObject>();
        audioBubbleToAudioClipNumber = new Dictionary<GameObject, int>();
        audioBubblesCollected = 0;
        for (int i = 0; i < audioBubbleVisualizations.Count; i++)
        {
            audioBubblesLeft.Add(i);
        }
        startedGameOver = false;
        gameOver = false;



    }

    void ResetDistractionState()
    {
        distractionAudioPlayer.Stop();

        while (distractions.Count > 0)
        {
            PhotonNetwork.Destroy(distractions[0]);
            distractions.RemoveAt(0);
        }
        distractions = new List<GameObject>();
        distractionMoveVectors = new List<Vector3>();
        distractionSpawnTimes = new List<float>();
        distractionDestroyTimes = new List<float>();
        distractionTargetPositions = new List<Vector3>();
        distractionsPickedUp = 0;
    }

    IEnumerator endGameSequence()
    {
        distractionAudioPlayer.Stop();
        for (int j = 0; j < audioBubbleVisualizations.Count; j++)
        {
            audioBubbleVisualizations[j].GetComponent<AudioSource>().Stop();
        }
        while (distractions.Count > 0)
        {
            PhotonNetwork.Destroy(distractions[0]);
            distractions.RemoveAt(0);
        }
        while (audioBubblesFlying.Count > 0)
        {
            PhotonNetwork.Destroy(audioBubblesFlying[0]);
            audioBubblesFlying.RemoveAt(0);
        }
        int i = 0;
        bool playing = false;
        while (i < audioBubbleVisualizations.Count)
        {
            if (playing)
            {
                if (!audioBubbleVisualizations[i].GetComponent<AudioSource>().isPlaying)
                {
                    playing = false;
                    i++;
                }
            }
            else
            {
                audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().StartPlaying(true);
                playing = true;
            }
            yield return null;
        }
        gameOver = true;
    }

    IEnumerator NetworkedAudioBubblePlayedAfter(GameObject audioBubble, float t)
    {
        yield return new WaitForSeconds(t);
        if (audioBubble != null)
        {
            audioBubblesPlayed.Add(audioBubble);
            //for(int i = 0; i < audioBubblesHolding.Count; i++)
            //{
            //    audioBubblesHolding[i].SetActive(false);
            //}
            for (int i = 0; i < audioBubblesPlayed.Count; i++)
            {
                //audioBubblesPlayed[i].SetActive(false);

                middleBranch.transform.GetChild(i).gameObject.SetActive(true);
                players[0].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                              RpcTarget.All, i);
                players[1].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                          RpcTarget.All, i);
            }
            audioBubbleToAudioClipNumber.Remove(audioBubble);
            audioBubblesHolding.Remove(audioBubble);
            audioBubblesCollected++;
            lastSpawnTime = Time.time - 4f; // let the bird be created 5 - 4 = 1second after current audio clip
        }
    }


    IEnumerator NetworkedDestroyAudioBubbleAfter(GameObject audioBubble, float t)
    {
        yield return new WaitForSeconds(t);

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
