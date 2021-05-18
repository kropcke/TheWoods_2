using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class ServerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    // gameplay/state/debug variables
    GameConfiguration variables;
    public static float gameAreaSize = 2f;
    bool startedGameOver = false;
    public static bool gameOver = false;
    bool gameStarted = false;
    bool simulatePlayers = false;

    // Bird variables
    public float magneticSpeed = 0.5f;
    public float collectionRadius = 0.25f;
    public int maxNumOfBirdsFlyingAtOneTime = 1;
    public string birdPrefab;
    public List<GameObject> birdsFlying;
    public List<GameObject> birdsPlayedVoiceMail;
    HashSet<GameObject> birdsCurrentlyPlayingVoiceMail;
    List<Vector3> birdMovementVectors;
    List<Vector3> birdTargetPositions;
    public List<GameObject> birdsHolding;
    public float birdMoveSpeed = 0.01f;
    public List<int> birdsLeft; // what is this?
    Dictionary<GameObject, int> birdToVoiceMailNumber;
    public int totalBirdCount = 4;
    public int birdsCollected = 0;
    float distToChangeTarget = 0.25f; // for both cloud and bird
    public float timeBetweenBirdSpawns = 5f;
    float birdLastSpawnTime = 0;
    public float timeToWaitBeforeSpawningFirstBird = 5f;
    bool readyToSpawnFirstBird = false;

    public List<GameObject> audioBubbleVisualizations;
    private GameObject middleBranch;
    private GameObject feathersParticleSystem;
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;


    // player variables
    GameObject lightening;
    public GameObject[] players;
    bool lighteningAssigned = false;

    // distraction (clouds) variables
    public string[] distractionPrefabs;
    public AudioSource distractionAudioPlayer;
    public int numDistractionsAtOneTime = 1;
    int distractionsPickedUp = 0;
    List<GameObject> distractions;
    public float distractionMoveSpeed = 0.2f;
    public List<Vector3> distractionMoveVectors;
    public List<Vector3> distractionTargetPositions;
    float distractionDestroyTime = 0;
    public float timeBetweenDistractionSpawns = 3f;
    bool readyToSpawnFirstDistraction = false;
    public float timeToWaitBeforeSpawningFirstDistraction = 3f;

    private string voicemailPattern = "Voicemail-";
    public int numOfVoiceMails = 2;

    public OSC oscObj;
    public string patchName;
    bool enableOSC = true;


    public void Awake()
    {
        if (photonView.IsMine)
        {

            Destroy(GameObject.Find("ARCamera"));
            LocalServerInstance = gameObject;
            
            lightening = GameObject.Find("Lightening");
            oscObj = GameObject.Find("Osc").GetComponent<OSC>();
            chooseNewVoicemail(GameObject.FindGameObjectsWithTag("AudioVisualization"));

            birdsFlying = new List<GameObject>();
            birdsCurrentlyPlayingVoiceMail = new HashSet<GameObject>();
            birdsPlayedVoiceMail = new List<GameObject>();
            birdMovementVectors = new List<Vector3>();
            birdTargetPositions = new List<Vector3>();
            birdsHolding = new List<GameObject>();
            birdToVoiceMailNumber = new Dictionary<GameObject, int>();
            birdsLeft = new List<int>();
            for (int i = 0; i < audioBubbleVisualizations.Count; i++)
            {
                birdsLeft.Add(i);

            }

            distractions = new List<GameObject>();
            distractionMoveVectors = new List<Vector3>();
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
            feathersParticleSystem = GameObject.Find("Feathers");
            variables = GameObject.FindGameObjectWithTag("GameConfiguration").GetComponent<GameConfiguration>();
            distractionAudioPlayer = variables.audioSource;

            lightening.GetComponent<PhotonView>().RequestOwnership();
            lightening.transform.GetChild(0).GetChild(0).GetComponent<PhotonView>().RequestOwnership();
            lightening.transform.GetChild(0).GetChild(1).GetComponent<PhotonView>().RequestOwnership();

        }
    }
    public void Update()
    {
        if (photonView.IsMine)
        {
            if (simulatePlayers)
            {
                simulateTwoPlayers();

            }
            
            if (gameStarted && PhotonNetwork.PlayerList.Length == 1 && !simulatePlayers)
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
                    StartCoroutine(waitBeforeSpawningFirstBird(timeToWaitBeforeSpawningFirstBird));
                    StartCoroutine(waitBeforeSpawningFirstDistraction(timeToWaitBeforeSpawningFirstDistraction));

                    StartCoroutine(waitBeforeSpawningFirstBird(timeToWaitBeforeSpawningFirstBird));

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
                        StartCoroutine(waitBeforeSpawningFirstDistraction(timeToWaitBeforeSpawningFirstDistraction));

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
            if (birdsCollected == totalBirdCount)
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

                CheckAudioPickups();

                DistractionPickedUp();
                MagneticAudioMovement();

            }


            // Server Commands
            if (Input.GetKeyDown(KeyCode.Space) || GameConfiguration.restartGame)
            {
                Debug.LogFormat("Restarting game: Restart variable value is {0} ", GameConfiguration.restartGame);
                GameConfiguration.restartGame = false;
                Reset();
            }
            if (Input.GetKeyDown(KeyCode.A) && birdsFlying.Count > 0)
            {
                AudioAttached(birdsFlying[0]);
            }
            if (Input.GetKeyDown(KeyCode.S) && birdsHolding.Count > 0)
            {
                AudioPickedUp(birdsHolding[0]);
            }
            // Add a bird to the branch
            if (Input.GetKeyDown(KeyCode.D) && birdsHolding.Count > 0)
            {
                birdsLeft.Add(birdToVoiceMailNumber[birdsHolding[0]]);
                birdToVoiceMailNumber.Remove(birdsHolding[0]);
                StartCoroutine(NetworkedDestroyAudioBubbleAfter(birdsHolding[0], 0));
                birdsHolding.RemoveAt(0);
            }
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
                GameConfiguration.restartGame = true;
            }

        }
    }

    public void chooseNewVoicemail(GameObject[] audioVisualization)
    {
        int voicemailNum = (int)Random.Range(1, numOfVoiceMails + 1);
        Debug.LogFormat("voicemailNum:" + voicemailNum);
        audioBubbleVisualizations = new List<GameObject>(totalBirdCount);
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
    IEnumerator waitBeforeSpawningFirstBird(float t)
    {
        yield return new WaitForSeconds(t);
        readyToSpawnFirstBird = true;

    }
    IEnumerator waitBeforeSpawningFirstDistraction(float t)
    {
        yield return new WaitForSeconds(t);
        readyToSpawnFirstDistraction = true;

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
            readyToSpawnFirstBird && birdsFlying.Count + birdsHolding.Count < maxNumOfBirdsFlyingAtOneTime &&
            Time.time - birdLastSpawnTime > timeBetweenBirdSpawns &&
            birdsLeft.Count > 0
        )
        {
            CreateAudioBubbleRandomClip();
            birdLastSpawnTime = Time.time;
        }
        if (readyToSpawnFirstDistraction && distractions.Count < numDistractionsAtOneTime && Time.time - distractionDestroyTime > timeBetweenDistractionSpawns)
        {
            CreateDistraction();

        }
    }
    void UpdateAudioClipMovement()
    {

        for (int i = 0; i < birdsFlying.Count; i++)
        {
            /**
             * Modified bird motion.
             * Bird now points in the direction of its motion (usually).
             */

            Vector3 start = birdsFlying[i].transform.position;
            Vector3 end = birdTargetPositions[i];
            Debug.DrawLine(start, end, Color.white);

            // Record the last position.
            Vector3 last = birdsFlying[i].transform.position;

            // This line actually moves the bird.
            // birdsFlying[i].transform.position = Vector3.MoveTowards(start, end,  0.4f * Time.deltaTime);
            birdsFlying[i].transform.position += birdMovementVectors[i];

            // Record the current position.
            Vector3 current = birdsFlying[i].transform.position;

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
            float localDegrees = Vector3.Angle(birdsFlying[i].transform.forward, direction); // angle relative to last heading of GameObject
            // Debug.LogFormat("worldDegrees: {0}", worldDegrees);
            // Debug.LogFormat("localDegrees: {0}", localDegrees);

            birdsFlying[i].transform.rotation = Quaternion.Euler(
                birdsFlying[i].transform.rotation.eulerAngles.x,
                worldDegrees + 180,
                birdsFlying[i].transform.rotation.eulerAngles.z
            );

            // "Magnetic" effect.
            birdMovementVectors[i] = Vector3.Lerp(
                birdMovementVectors[i],
                (birdTargetPositions[i] - birdsFlying[i].transform.position).normalized * birdMoveSpeed,
                0.1f * Time.deltaTime
            );

            // When the bird gets close to the goal position, generate a new position to move towards.
            if (Vector3.Distance(birdsFlying[i].transform.position, birdTargetPositions[i]) < distToChangeTarget)
            {
                birdTargetPositions[i] = CreateNewRandomPosition();
            }
            if (enableOSC)
            {
                OscMessage message = new OscMessage();
                message.address = "/BirdPosXYZ";
                message.values.Add(birdsFlying[i].transform.position.x);
                message.values.Add(birdsFlying[i].transform.position.y);
                message.values.Add(birdsFlying[i].transform.position.z);
                oscObj.Send(message);
            }

        }
    }

    void UpdateAttachedAudioPosition()
    {
        if (players.Length == 2)
        {

            for (int i = 0; i < birdsHolding.Count; i++)
            {

                birdsHolding[i].SetActive(false);
                if (birdsPlayedVoiceMail.Count == 0)
                {
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, 0);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, 0);
                    middleBranch.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    middleBranch.transform.GetChild(birdsPlayedVoiceMail.Count).gameObject.SetActive(true);
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, birdsPlayedVoiceMail.Count);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                        RpcTarget.All, birdsPlayedVoiceMail.Count);
                }
            }
            for (int i = 0; i < birdsPlayedVoiceMail.Count; i++)
            {
                birdsPlayedVoiceMail[i].SetActive(false);
            }
        }
    }
    void CheckAudioPickups()
    {
        if (players.Length == 2)
        {
            for (int i = 0; i < birdsHolding.Count; i++)
            {
                //Play the voice mail as soon as the bird is on the branch
                if (!VoicemailPlaying() && !birdsCurrentlyPlayingVoiceMail.Contains(birdsHolding[i]))
                {
                    birdsCurrentlyPlayingVoiceMail.Add(birdsHolding[i]);
                    if (enableOSC)
                    {
                        OscMessage message1 = new OscMessage();

                        message1.address = "/BirdMessage";
                        message1.values.Add(birdsPlayedVoiceMail.Count); //0-3
                        message1.values.Add(1);
                        oscObj.Send(message1);
                    }

                    AudioPickedUp(birdsHolding[i]);

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

            for (int i = 0; i < birdsFlying.Count; i++)
            {
                if (Vector3.Cross(ray.direction, birdsFlying[i].transform.position - ray.origin).magnitude < collectionRadius)
                {
                    birdsFlying[i].transform.position +=
                        (center - birdsFlying[i].transform.position).normalized * magneticSpeed * Time.deltaTime;

                    AudioAttached(birdsFlying[i]);

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

            if (birdsCollected < (totalBirdCount - 1))
            {
                if (birdToVoiceMailNumber.ContainsKey(audioBubble))
                {
                    //audioBubbleVisualizations[birdToVoiceMailNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StartPlaying(false);
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                        RpcTarget.All, audioBubbleVisualizations[birdToVoiceMailNumber[audioBubble]].name);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                        RpcTarget.All, audioBubbleVisualizations[birdToVoiceMailNumber[audioBubble]].name);

                    StartCoroutine(
                        NetworkedAudioBubblePlayedAfter(audioBubble,
                            audioBubbleVisualizations[birdToVoiceMailNumber[audioBubble]].GetComponent<AudioSource>().clip.length
                        )
                    );
                    
                    audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "singing");
                }
            }
            if (birdsCollected == (totalBirdCount - 1))
            {
                while (distractions.Count > 0)
                {
                    PhotonNetwork.Destroy(distractions[0]);
                    distractions.RemoveAt(0);
                }
                players[0].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                    RpcTarget.All, audioBubbleVisualizations[totalBirdCount - 1].name);
                players[1].transform.parent.GetComponent<PhotonView>().RPC("PlayVoiceMail",
                    RpcTarget.All, audioBubbleVisualizations[totalBirdCount - 1].name);

                StartCoroutine(
                    NetworkedAudioBubblePlayedAfter(audioBubble,
                        audioBubbleVisualizations[totalBirdCount - 1].GetComponent<AudioSource>().clip.length
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
            birdsHolding.Add(audioBubble);
            int spot = birdsFlying.IndexOf(audioBubble);
            birdsFlying.Remove(audioBubble);
            birdMovementVectors.RemoveAt(spot);
            birdTargetPositions.RemoveAt(spot);
            audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "perched");

        }
    }

    void CreateAudioBubbleRandomClip()
    {
        if (photonView.IsMine)
        {
            int clipNumber = birdsLeft[(int)Random.Range(0, birdsLeft.Count - 1)];
            GameObject bubble = PhotonNetwork.Instantiate(birdPrefab, CreateNewRandomPosition(), Quaternion.identity);
            birdsLeft.Remove(clipNumber);
            birdsFlying.Add(bubble);
            birdToVoiceMailNumber.Add(bubble, clipNumber);
            birdMovementVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f)).normalized * birdMoveSpeed);
            birdTargetPositions.Add(CreateNewRandomPosition());
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
                
                if ((birdsHolding.Count > 0 || birdsCurrentlyPlayingVoiceMail.Count > 0 || birdsPlayedVoiceMail.Count > 0))
                {
                    
                    players[0].transform.parent.GetComponent<PhotonView>().RPC("EnableFeatherParticles",
                        RpcTarget.All, 6);
                    players[1].transform.parent.GetComponent<PhotonView>().RPC("EnableFeatherParticles",
                        RpcTarget.All, 6);
                    feathersParticleSystem.transform.GetComponent<ParticleFeathers>().enabled = true;
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

    IEnumerator DisableFeathersAfter(float t)
    {
        yield return new WaitForSeconds(t);
        middleBranch.transform.GetChild(6).gameObject.SetActive(false);
    }

    void CreateDistraction()
    {
        if (players.Length == 2)
        {
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
                distractionDestroyTime = Time.time;
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
        for (int i = 0; i < birdsPlayedVoiceMail.Count; i++)
        {
            birdsPlayedVoiceMail[i].GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "flying");
            Vector3 currentPos = birdsPlayedVoiceMail[i].transform.position;
            Vector3 newPos = new Vector3(currentPos.x + 1, currentPos.y + 2, currentPos.z + 2);
            birdsPlayedVoiceMail[i].transform.position = Vector3.Lerp(currentPos, newPos, Time.deltaTime * 1f);

        }

        for (int i = 0; i < birdsHolding.Count; i++)
        {
            birdsHolding[i].GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "flying");
            Vector3 currentPos = birdsHolding[i].transform.position;
            Vector3 newPos = new Vector3(currentPos.x + 1, currentPos.y + 2, currentPos.z + 2);
            birdsHolding[i].transform.position = Vector3.Lerp(currentPos, newPos, Time.deltaTime * 1f);
        }

    }

    void ResetAudioBubbleState()

    {
        for (int i = 0; i < totalBirdCount; i++)
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

        birdsLeft = new List<int>();

        while (birdsPlayedVoiceMail.Count > 0)
        {
            PhotonNetwork.Destroy(birdsPlayedVoiceMail[0]);
            birdsPlayedVoiceMail.RemoveAt(0);
        }
        while (birdsFlying.Count > 0)
        {
            PhotonNetwork.Destroy(birdsFlying[0]);
            birdsFlying.RemoveAt(0);
        }
        while (birdsHolding.Count > 0)
        {
            PhotonNetwork.Destroy(birdsHolding[0]);
            birdsHolding.RemoveAt(0);
        }
        //for (int i = 0; i < audioBubbleVisualizations.Count; i++)
        //{
        //    Debug.Log(audioBubbleVisualizations[i].name);
        //    audioBubbleVisualizations[i].GetComponent<SoundClipGameObjects>().Reset();
        //}
        birdsFlying = new List<GameObject>();
        birdsCurrentlyPlayingVoiceMail = new HashSet<GameObject>();
        birdMovementVectors = new List<Vector3>();
        birdTargetPositions = new List<Vector3>();
        birdsHolding = new List<GameObject>();
        birdToVoiceMailNumber = new Dictionary<GameObject, int>();
        birdsCollected = 0;

        for (int i = 0; i < audioBubbleVisualizations.Count; i++)
        {
            birdsLeft.Add(i);
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
            distractionDestroyTime = Time.time;
        }
        distractions = new List<GameObject>();
        distractionMoveVectors = new List<Vector3>();
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
        while (birdsFlying.Count > 0)
        {
            PhotonNetwork.Destroy(birdsFlying[0]);
            birdsFlying.RemoveAt(0);
        }

        yield return null;
        gameOver = true;
        if (enableOSC)
        {
            OscMessage message = new OscMessage();
            message.address = "/GameOnOff";
            message.values.Add(0);
            oscObj.Send(message);
        }
        ShowMenuOptions("gameOver");

    }

    IEnumerator NetworkedAudioBubblePlayedAfter(GameObject audioBubble, float t)
    {
        yield return new WaitForSeconds(t);
        if (audioBubble != null)
        {
            birdsPlayedVoiceMail.Add(audioBubble);
            if (enableOSC)
            {
                OscMessage message1 = new OscMessage();

                message1.address = "/BirdMessage";
                message1.values.Add(birdsPlayedVoiceMail.Count - 1); //0-3
                message1.values.Add(0);
                oscObj.Send(message1);
            }
            for (int i = 0; i < birdsPlayedVoiceMail.Count - 1; i++)
            {
                middleBranch.transform.GetChild(i).gameObject.SetActive(true);
                //middleBranch.transform.GetChild(i).transform.GetChild(3).gameObject.SetActive(false);
                players[0].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                    RpcTarget.All, i);
                players[1].transform.parent.GetComponent<PhotonView>().RPC("EnableBird",
                    RpcTarget.All, i);
            }
            birdToVoiceMailNumber.Remove(audioBubble);
            birdsHolding.Remove(audioBubble);
            birdsCollected++;
            birdLastSpawnTime = Time.time - 4f; // let the bird be created 5 - 4 = 1second after current audio clip
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