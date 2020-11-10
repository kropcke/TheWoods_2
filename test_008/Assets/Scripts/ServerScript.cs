using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
    public float audioBubbleMoveSpeed = 0.01f;

    public float smoothSpeed = 0.02f;
    public List<int> audioBubblesLeft; // what is this?

    public Dictionary<GameObject, int> audioBubbleToAudioClipNumber;
    public int audioBubblestotalCount = 4;
    public int audioBubblesCollected = 0;
    float distToChangeTarget = 0.25f; // for both cloud and bird

    public List<GameObject> audioBubbleVisualizations;

    // player variables
    GameObject lightening;
    public GameObject[] players;
    bool lighteningAssigned = false;

    // gameplay/state/debug variables
    public float timeBetweenSpawns = 5f;
    float lastSpawnTime = 0;

    bool startedGameOver = false;
    public static bool gameOver = false;
    bool debugMode = true; // // todo: Move to gamecontroller.
    bool simulatePlayers = false;

    // distraction (clouds) variables
    public string[] distractionPrefabs;

    public AudioSource distractionAudioPlayer;
    public int numDistractionsAtOneTime = 1;
    int distractionsPickedUp = 0;
    List<GameObject> distractions;
    public float distractionMoveSpeed = 0.2f;
    public List<Vector3> distractionMoveVectors;
    public List<Vector3> distractionTargetPositions;
    public Vector3 distractionIntialPosition;
    List<float> distractionSpawnTimes;
    List<float> distractionDestroyTimes;
    private GameObject middleBranch;

    private string voicemailPattern = "Voicemail-";
    public int numOfVoiceMails = 2;

    public OSC oscObj;
    public string patchName;
    bool enableOSC = true;

    bool gameStarted = false;

    public float timeToWaitBeforeSpawningFirstBird = 5f;
    bool readyToSpawnFirstBird = false;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;


    public void Awake()
    {
        if (photonView.IsMine)
        {

            Destroy(GameObject.Find("ARCamera"));

            LocalServerInstance = gameObject;
            lightening = GameObject.Find("Lightening");

            oscObj = GameObject.Find("Osc").GetComponent<OSC>();
            // obiSolver1 = GameObject.Find("ObiSolver1");
            chooseNewVoicemail(GameObject.FindGameObjectsWithTag("AudioVisualization"));

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

            //Start audio coming from Pd
            if (enableOSC)
            {
                OscMessage message = new OscMessage();
                message.address = "/GameOnOff";
                message.values.Add(1);
                oscObj.Send(message);
            }

        }
        //DontDestroyOnLoad(gameObject);
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

        }
    }

    [PunRPC]
    //TODO: REfactor this to accomodate multiple transfers from client to server
    public void UpdateVariableInServer(string option)
    {
        if (photonView.IsMine)
        {

            if (option == "NewGame")
            {
                //Stops audio coming from Pd
                if (enableOSC)
                {
                    OscMessage message = new OscMessage();
                    message.address = "/GameOnOff";
                    message.values.Add(0);
                    oscObj.Send(message);
                }
            }
            else if (option == "RestartGame")
            {
                Debug.LogFormat("Inside RestartGame");
                GameConfiguration.restartGame = true;
            }

        }
    }

    public void chooseNewVoicemail(GameObject[] audioVisualization)
    {
        int voicemailNum = (int)Random.Range(1, numOfVoiceMails + 1);
        Debug.LogFormat("voicemailNum:" + voicemailNum);
        audioBubbleVisualizations = new List<GameObject>(audioBubblestotalCount);
        //Voice mail 0 will be for cut scene-not adding for now
        GameObject all = null;
        for (int i = 0; i < audioVisualization.Length; i++)
        {
            if (audioVisualization[i].name.StartsWith(voicemailPattern + voicemailNum))
            {

                string[] tmp = audioVisualization[i].name.Split('-');
                //Debug.Log(tmp[2]);
                if (tmp[2] == "all")
                {
                    all = audioVisualization[i];
                }
                else
                {
                    audioBubbleVisualizations.Add(audioVisualization[i]);
                }
            }

        }
        audioBubbleVisualizations.Add(all);
        //for (int i = 0; i < audioBubbleVisualizations.Count; i++)
        //{
        //    audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().Init();
        //    audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().PositionInCircle(4 * i * audioBubblestotalCount, 64, gameAreaSize);
        //}
    }

    public override void OnDisable()
    {
        base.OnDisable();
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
    IEnumerator startNewGame()
    {
        yield return null;
        gameStarted = false;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
    public void Update()
    {
        if (photonView.IsMine)
        {

            if (simulatePlayers)
            {
                simulateTwoPlayers();

            }
            if (gameStarted && PhotonNetwork.PlayerList.Length == 1)
            {

                StartCoroutine(startNewGame());
            }
            players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length == 2)
            {
                if (!gameStarted)
                {
                    gameStarted = true;
                }
                int photonViewId1 = players[0].GetPhotonView().ViewID;
                int photonViewId2 = players[1].GetPhotonView().ViewID;

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
                        if (photonViewId1 > photonViewId2)
                        {
                            players[0].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All, lightening.transform.GetChild(0).GetChild(0).gameObject.name);
                            players[1].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All, lightening.transform.GetChild(0).GetChild(1).gameObject.name);

                        }
                        else
                        {
                            players[1].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All, lightening.transform.GetChild(0).GetChild(0).gameObject.name);
                            players[0].transform.parent.GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All, lightening.transform.GetChild(0).GetChild(1).gameObject.name);

                        }
                        lighteningAssigned = true;

                        StartCoroutine(waitBeforeSpawningFirstBird(timeToWaitBeforeSpawningFirstBird));

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
            if (Input.GetKeyDown(KeyCode.Space) || GameConfiguration.restartGame)
            {
                Debug.LogFormat("Restarting game: Restart variable value is {0} ", GameConfiguration.restartGame);
                GameConfiguration.restartGame = false;
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

    IEnumerator waitBeforeSpawningFirstBird(float t)
    {
        yield return new WaitForSeconds(t);
        readyToSpawnFirstBird = true;

    }
    void ShowMenuOptions(string option)
    {
        //TODO:Change this
        players[0].transform.parent.GetComponent<PhotonView>().RPC("ShowMenu",
            RpcTarget.All, option);
        players[1].transform.parent.GetComponent<PhotonView>().RPC("ShowMenu",
            RpcTarget.All, option);
    }
    void UpdateSpawning()
    {
        if (
            readyToSpawnFirstBird && audioBubblesFlying.Count + audioBubblesHolding.Count < audioBubblesFlyingAtOneTime &&
            Time.time - lastSpawnTime > timeBetweenSpawns &&
            audioBubblesLeft.Count > 0
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
            if (enableOSC)
            {
                OscMessage message = new OscMessage();
                message.address = "/BirdPosXYZ";
                message.values.Add(audioBubblesFlying[i].transform.position.x);
                message.values.Add(audioBubblesFlying[i].transform.position.y);
                message.values.Add(audioBubblesFlying[i].transform.position.z);
                oscObj.Send(message);
            }

        }
    }

    void UpdateAttachedAudioPosition()
    {
        if (players.Length == 2)
        {

            for (int i = 0; i < audioBubblesHolding.Count; i++)
            {

                audioBubblesHolding[i].SetActive(false);
                if (audioBubblesPlayed.Count == 0)
                {
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, 0);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, 0);
                    middleBranch.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    middleBranch.transform.GetChild(audioBubblesPlayed.Count).gameObject.SetActive(true);
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, audioBubblesPlayed.Count);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, audioBubblesPlayed.Count);
                }
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
                    if (enableOSC)
                    {
                        OscMessage message1 = new OscMessage();

                        message1.address = "/BirdMessage";
                        message1.values.Add(audioBubblesPlayed.Count); //0-3
                        message1.values.Add(1);
                        oscObj.Send(message1);
                    }

                    AudioPickedUp(audioBubblesHolding[i]);

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

            if (audioBubblesCollected < (audioBubblestotalCount - 1))
            {
                if (audioBubbleToAudioClipNumber.ContainsKey(audioBubble))
                {
                    //audioBubbleVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StartPlaying(false);
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
            if (audioBubblesCollected == (audioBubblestotalCount - 1))
            {
                while (distractions.Count > 0)
                {
                    PhotonNetwork.Destroy(distractions[0]);
                    distractions.RemoveAt(0);
                }
                players[0].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                    RpcTarget.All, audioBubbleVisualizations[audioBubblestotalCount - 1].name);
                players[1].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                    RpcTarget.All, audioBubbleVisualizations[audioBubblestotalCount - 1].name);

                StartCoroutine(
                    NetworkedAudioBubblePlayedAfter(audioBubble,
                        audioBubbleVisualizations[audioBubblestotalCount - 1].GetComponent<AudioSource>().clip.length
                    )
                );

                audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "singing");

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
            int clipNumber = audioBubblesLeft[(int)Random.Range(0, audioBubblesLeft.Count - 1)];
            Debug.Log("clipNumber" + clipNumber);
            GameObject bubble = PhotonNetwork.Instantiate(audioBubblePrefab, CreateNewRandomPosition(), Quaternion.identity);
            audioBubblesLeft.Remove(clipNumber);
            audioBubblesFlying.Add(bubble);
            audioBubbleToAudioClipNumber.Add(bubble, clipNumber);
            audioBubbleMovementVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f)).normalized * audioBubbleMoveSpeed);
            audioBubbleTargetPositions.Add(CreateNewRandomPosition());
            //bubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "flying");
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
        if (photonView.IsMine)
        {
            //TODO: Send the value 0 or 1 only once
            if (DistractionController.inDistraction)
            {
                if (enableOSC)
                {

                    OscMessage message1 = new OscMessage();

                    message1.address = "/TriggerDistraction";
                    message1.values.Add(1);
                    oscObj.Send(message1);
                }

                if ((audioBubblesHolding.Count > 0 || audioBubblesNowPlaying.Count > 0 || audioBubblesPlayed.Count > 0))
                {
                    FlyawayAndDestroyBird();
                    ResetAudioBubbleState();
                }

            }
            else
            {
                if (enableOSC)
                {
                    OscMessage message = new OscMessage();

                    message.address = "/TriggerDistraction";
                    message.values.Add(0);
                    oscObj.Send(message);
                }
            }
        }
    }

    void CreateDistraction()
    {
        if (players.Length == 2)
        {
            distractionIntialPosition = CreateNewRandomPosition();
            GameObject d = PhotonNetwork.Instantiate(distractionPrefabs[Random.Range(0, distractionPrefabs.Length)], CreateNewRandomPosition(), Quaternion.identity);
            distractions.Add(d);
            float angle = Random.Range(0, 2 * Mathf.PI);
            float dist = 2 * gameAreaSize;
            float randomHeight = Random.Range(0.5f, 1.5f);
            d.transform.position = new Vector3(dist * Mathf.Cos(angle), randomHeight, dist * Mathf.Sin(angle));
            d.transform.LookAt(Vector3.zero + new Vector3(0, randomHeight, 0));
            distractionTargetPositions.Add(new Vector3(-dist * Mathf.Cos(angle), randomHeight, -dist * Mathf.Sin(angle)));
            distractionMoveVectors.Add((distractionTargetPositions[distractionTargetPositions.Count - 1] -
                d.transform.position).normalized * distractionMoveSpeed * Time.deltaTime);

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
            if (enableOSC)
            {

                OscMessage message = new OscMessage();

                message.address = "/CloudPosXYZ";
                message.values.Add(distractions[i].transform.position.x);
                message.values.Add(distractions[i].transform.position.y);
                message.values.Add(distractions[i].transform.position.z);
                oscObj.Send(message);
            }

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
    public void Reset()
    {
        chooseNewVoicemail(GameObject.FindGameObjectsWithTag("AudioVisualization"));
        ResetAudioBubbleState();
        ResetDistractionState();
    }

    void FlyawayAndDestroyBird()
    {
        for (int i = 0; i < audioBubblesPlayed.Count; i++)
        {
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

        // play particle feather effect
        var particleFeatherGameObject = GameObject.Find("FeatherParticleManager");
        particleFeatherGameObject.GetComponent<ParticleFeathers>().enabled = true;

    }

    void ResetAudioBubbleState()

    {
        for (int i = 0; i < audioBubblestotalCount; i++)
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
        //for (int i = 0; i < audioBubbleVisualizations.Count; i++)
        //{
        //    Debug.Log(audioBubbleVisualizations[i].name);
        //    audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().Reset();
        //}
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

        yield return null;
        gameOver = true;
        ShowMenuOptions("gameOver");

    }

    IEnumerator NetworkedAudioBubblePlayedAfter(GameObject audioBubble, float t)
    {
        yield return new WaitForSeconds(t);
        if (audioBubble != null)
        {
            audioBubblesPlayed.Add(audioBubble);
            if (enableOSC)
            {
                OscMessage message1 = new OscMessage();

                message1.address = "/BirdMessage";
                message1.values.Add(audioBubblesPlayed.Count - 1); //0-3
                message1.values.Add(0);
                oscObj.Send(message1);
            }
            for (int i = 0; i < audioBubblesPlayed.Count - 1; i++)
            {
                middleBranch.transform.GetChild(i).gameObject.SetActive(true);
                //middleBranch.transform.GetChild(i).transform.GetChild(3).gameObject.SetActive(false);
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

    public void OnApplicationQuit()
    {

    }
}