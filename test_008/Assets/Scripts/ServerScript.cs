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
    public string audioBubblePrefab;
    public string[] distractionPrefabs;
    GameObject distractionSoundsGO;
    public GameObject ShieldPrefab;
    public List<AudioClip> distractionSounds;
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalServerInstance;

    GameObject lightening;

    int distractionsPickedUp = 0;
    int clipsCollected = 0;
    int totalClips = 8;
    GameObject shield;
    float lastSpawnTime = 0;

    List<GameObject> audioVisualizations;

    List<GameObject> distractions;
    List<GameObject> audioBubbles;
    List<Vector3> movementVectors;
    List<Vector3> distractionMoveVectors;
    List<Vector3> targetPositions;
    List<Vector3> distractionTargetPositions;
    List<float> distractionSpawnTimes;
    List<float> distractionDestroyTimes;

    List<GameObject> holdingAudioClips;

    float distToChangeTarget = 0.25f;
    float moveSpeed = 0.02f;
    float distractionSpeed = 0.3f;
    List<int> audioNumbersLeft;

    Dictionary<GameObject, int> audioBubbleToAudioClipNumber;
    GameObject[] players;
    bool startedGameOver = false;
    bool gameOver = false;

    bool lighteningAssigned = false;
    bool invincible = false;
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
            distractionSpawnTimes = new List<float>();
            distractionDestroyTimes = new List<float>();
            holdingAudioClips = new List<GameObject>();
            distractionTargetPositions = new List<Vector3>();
            audioBubbleToAudioClipNumber = new Dictionary<GameObject, int>();
            audioNumbersLeft = new List<int>();
            distractionSoundsGO = GameObject.FindWithTag("DistractionSoundsGO");
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
                if (lightening && !lighteningAssigned)
                {
                    lightening.SetActive(true);
                    players[0].GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All,
                    lightening.transform.GetChild(0).GetChild(0).gameObject.name);
                    players[1].GetComponent<PhotonView>().RPC("TakeControl", RpcTarget.All,
                    lightening.transform.GetChild(0).GetChild(1).gameObject.name);
                    lighteningAssigned = true;
                    //lightening.transform.GetChild(0).GetChild(0).transform.position = players[0].transform.position;
                    //lightening.transform.GetChild(0).GetChild(1).transform.position = players[1].transform.position;
                }
            }
            else
            {
                if(lightening) lightening.SetActive(false);
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
                UpdateSpawning();
                UpdateAudioClipMovement();
                UpdateAttachedAudioPosition();
                UpdateDistractionMovement();

                CheckAudioAttachPickup();
                CheckAudioPickups();
                CheckDistractionPickups();
                MagneticAudioMovement();
                UpdateInvicibility();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Reset();
            }
            if (Input.GetKeyDown(KeyCode.A) && audioBubbles.Count > 0)
            {
                AudioAttached(audioBubbles[0]);
            }
            if(Input.GetKeyDown(KeyCode.S) && holdingAudioClips.Count > 0)
            {
                AudioPickedUp(holdingAudioClips[0]);
            }
            if(Input.GetKeyDown(KeyCode.D) && holdingAudioClips.Count > 0)
            {
                audioNumbersLeft.Add(audioBubbleToAudioClipNumber[holdingAudioClips[0]]);
                audioBubbleToAudioClipNumber.Remove(holdingAudioClips[0]);
                PhotonNetwork.Destroy(holdingAudioClips[0]);
                holdingAudioClips.RemoveAt(0);
            }
        }
    }
    void Reset()
    {
        distractionSoundsGO.GetComponent<AudioSource>().Stop();
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
        while(holdingAudioClips.Count > 0)
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
        movementVectors = new List<Vector3>();
        distractionMoveVectors = new List<Vector3>();
        targetPositions = new List<Vector3>();
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
    IEnumerator endGameSequence() {
        distractionSoundsGO.GetComponent<AudioSource>().Stop();
        for(int j = 0; j < audioVisualizations.Count; j++)
        {
            audioVisualizations[j].GetComponent<AudioSource>().Stop();
        }
        while (distractions.Count > 0)
        {
            PhotonNetwork.Destroy(distractions[0]);
            distractions.RemoveAt(0);
        }
        while(audioBubbles.Count > 0)
        {
            PhotonNetwork.Destroy(audioBubbles[0]);
            audioBubbles.RemoveAt(0);
        }
        int i = 0;
        bool playing = false;
        while(i < audioVisualizations.Count)
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


    void UpdateInvicibility()
    {
        if(players.Length == 2)
        {
            if(Vector3.Distance(players[0].transform.position, players[1].transform.position) < 1f)
            {
                if (!shield)
                {
                    shield = PhotonNetwork.Instantiate(ShieldPrefab.name,
                    (players[0].transform.position + players[1].transform.position) / 2f,
                        Quaternion.identity);
                    shield.transform.localScale = Vector3.one * 2f;
                }
                else
                {
                    shield.transform.position = Vector3.Lerp(shield.transform.position,
                    (players[0].transform.position + players[1].transform.position) / 2f,
                        Time.deltaTime);
                }
                invincible = true;
                distractionSoundsGO.GetComponent<AudioSource>().volume = 0;
            }
            else
            {
                invincible = false;
                distractionSoundsGO.GetComponent<AudioSource>().volume = 1;
                if(shield) PhotonNetwork.Destroy(shield);
            }
        }
    }
    void UpdateDistractionsTimeout()
    {
        for(int i =0; i < distractions.Count; i++)
        {
            if(Time.time > distractionDestroyTimes[i])
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
    void UpdateSpawning()
    {
        if(audioBubbles.Count + holdingAudioClips.Count < numAudioBubblesAtOneTime && 
        Time.time - lastSpawnTime > timeBetweenSpawns && 
            audioNumbersLeft.Count > 0)
        {
            CreateAudioBubbleRandomClip();
            lastSpawnTime = Time.time;
        }
        if(distractions.Count < numDistractionsAtOneTime)
        {
            CreateDistraction();
        
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

    void MagneticAudioMovement()
    {
        if (players.Length == 2)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) *0.5f;
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
        if(players.Length == 2) {
           for (int i = 0; i < holdingAudioClips.Count; i++)
            {
                if(Vector3.Distance(players[0].transform.position, holdingAudioClips[i].transform.position) < 0.25f &&
                Vector3.Distance(players[1].transform.position, holdingAudioClips[i].transform.position) < 0.25f &&
                    !VoicemailPlaying())
                {
                    AudioPickedUp(holdingAudioClips[i]);
                    return;
                }
            }
        }

    }
    void CheckDistractionPickups()
    {
        if (players.Length == 2 && !invincible)
        {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            Ray ray = new Ray(players[0].transform.position, players[1].transform.position - players[0].transform.position);

            for (int i = 0; i < distractions.Count; i++)
            {
                if (Vector3.Cross(ray.direction, distractions[i].transform.position - ray.origin).magnitude < collectionRadius)
                {
                    DistractionPickedUp(distractions[i]);
                }
                /*
                if (Vector3.Distance(center, distractions[i].transform.position) < collectionRadius)
                {
                    DistractionPickedUp(distractions[i]);
                }
                */
            }
        }
    }

    void DistractionPickedUp(GameObject distraction)
    {
        if (photonView.IsMine && !invincible)
        {
            distractionSoundsGO.GetComponent<AudioSource>().loop = false;
            distractionSoundsGO.GetComponent<AudioSource>().Stop();
            distractionSoundsGO.GetComponent<AudioSource>().clip =
                distractionSounds[Random.Range(0, distractionSounds.Count)];
            distractionSoundsGO.GetComponent<AudioSource>().time = 0;
            distractionSoundsGO.GetComponent<AudioSource>().Play();
            distractionsPickedUp++;
            //Do some other stuff? Play scary noise?
            //int spot = distractions.IndexOf(distraction);
            //distractions.Remove(distraction);
            //distractionMoveVectors.RemoveAt(spot);
            //distractionTargetPositions.RemoveAt(spot);
            //PhotonNetwork.Destroy(distraction);
            if(holdingAudioClips.Count > 0)
            {
                audioNumbersLeft.Add(audioBubbleToAudioClipNumber[holdingAudioClips[0]]);
                audioBubbleToAudioClipNumber.Remove(holdingAudioClips[0]);
                PhotonNetwork.Destroy(holdingAudioClips[0]);
                holdingAudioClips.RemoveAt(0);
            }
        }
    }

    void AudioPickedUp(GameObject audioBubble)
    {
        if (photonView.IsMine)
        {
            clipsCollected++;
            if (clipsCollected != totalClips)
            {
                audioVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<SoundClipGameObjects>().StartPlaying();
                StartCoroutine(NetworkedDestroyAfter(audioBubble,
                    audioVisualizations[audioBubbleToAudioClipNumber[audioBubble]].GetComponent<AudioSource>().clip.length));

                audioBubbleToAudioClipNumber.Remove(audioBubble);
                holdingAudioClips.Remove(audioBubble);
                audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "singing");
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
            movementVectors.RemoveAt(spot);
            targetPositions.RemoveAt(spot);
            audioBubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "perched");
        }
    }
    void CreateAudioBubbleRandomClip()
    {

        int clipNumber = audioNumbersLeft[(int)Random.Range(0, audioNumbersLeft.Count)];
        GameObject bubble = PhotonNetwork.Instantiate(audioBubblePrefab, CreateNewRandomPosition(), Quaternion.identity);
        audioNumbersLeft.Remove(clipNumber);
        audioBubbles.Add(bubble);
        audioBubbleToAudioClipNumber.Add(bubble, clipNumber);
        movementVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f)).normalized * moveSpeed);
        targetPositions.Add(CreateNewRandomPosition());
        bubble.GetComponent<PhotonView>().RPC("SetState", RpcTarget.All, "flying");
    }
    void CreateDistraction()
    {
        GameObject d = PhotonNetwork.Instantiate(distractionPrefabs[Random.Range(0, distractionPrefabs.Length)]
        , CreateNewRandomPosition(), Quaternion.identity);
        distractions.Add(d);
        //distractionMoveVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
        //    Random.Range(-0.01f, 0.01f),
        //    Random.Range(-0.01f, 0.01f)).normalized * moveSpeed / 2f);
        float angle = Random.Range(0, 2 * Mathf.PI);
        float dist = 2 * gameAreaSize;
        float randomHeight = Random.Range(0.5f, 1.5f);
        d.transform.position = new Vector3(dist * Mathf.Cos(angle), randomHeight, dist * Mathf.Sin(angle));
        d.transform.LookAt(Vector3.zero + new Vector3(0, randomHeight, 0));
        distractionTargetPositions.Add(new Vector3(-dist * Mathf.Cos(angle), randomHeight, -dist * Mathf.Sin(angle)));
        distractionMoveVectors.Add((distractionTargetPositions[distractionTargetPositions.Count - 1]
        - d.transform.position).normalized * distractionSpeed * Time.deltaTime);
        //distractionSpawnTimes.Add(Time.time);
        //distractionDestroyTimes.Add(Time.time + Random.Range(5f, 10f));
    }
    Vector3 CreateNewRandomPosition()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        float dist = Random.Range(0, gameAreaSize);
        float randomHeight = Random.Range(0.5f, 1.5f);
        return new Vector3(dist * Mathf.Cos(angle), randomHeight, dist * Mathf.Sin(angle));
    }
    
    bool VoicemailPlaying() 
    {
        bool toReturn = false;
        for(int i = 0; i < audioVisualizations.Count; i++)
        {
            toReturn = toReturn || audioVisualizations[i].GetComponent<AudioSource>().isPlaying;
        }
        return toReturn;
    }
    IEnumerator NetworkedDestroyAfter(GameObject g, float t)
    {
        yield return new WaitForSeconds(t);
        PhotonNetwork.Destroy(g);
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
