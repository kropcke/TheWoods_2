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
    public List<AudioClip> distractionSounds;
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
    List<float> distractionSpawnTimes;
    List<float> distractionDestroyTimes;

    float distToChangeTarget = 0.25f;
    float moveSpeed = 0.01f;
    List<int> audioNumbersLeft;

    Dictionary<GameObject, int> audioBubbleToAudioClipNumber;
    GameObject[] players;
    bool startedGameOver = false;
    bool gameOver = false;
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
                UpdateDistractionsTimeout();
                UpdateSpawning();
                UpdateAudioClipMovement();
                UpdateDistractionMovement();
                CheckAudioPickups();
                CheckDistractionPickups();
                MagneticAudioMovement();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                clipsCollected = totalClips;
            }
        }
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
        && audioNumbersLeft.Count > 0)
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
    void CheckAudioPickups()
    {
        if(players.Length == 2) {
            Vector3 center = (players[0].transform.position + players[1].transform.position) * 0.5f;
            Ray ray = new Ray(players[0].transform.position, players[1].transform.position - players[0].transform.position);

            for (int i = 0; i < audioBubbles.Count; i++)
            {
                if(Vector3.Cross(ray.direction, audioBubbles[i].transform.position - ray.origin).magnitude < collectionRadius)
                {
                    AudioPickedUp(audioBubbles[i]);
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
    void CheckDistractionPickups()
    {
        if (players.Length == 2)
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
        if (photonView.IsMine)
        {
            distractionSoundsGO.GetComponent<AudioSource>().loop = false;
            distractionSoundsGO.GetComponent<AudioSource>().Stop();
            distractionSoundsGO.GetComponent<AudioSource>().clip =
                distractionSounds[Random.Range(0, distractionSounds.Count)];
            distractionSoundsGO.GetComponent<AudioSource>().time = 0;
            distractionSoundsGO.GetComponent<AudioSource>().Play();
            //distractionSoundsGO.GetComponent<AudioSource>().SetScheduledEndTime(2f);
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

        int clipNumber = audioNumbersLeft[(int)Random.Range(0, audioNumbersLeft.Count)];
        GameObject bubble = PhotonNetwork.Instantiate(audioBubblePrefab, CreateNewRandomPosition(), Quaternion.identity);
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
        GameObject d = PhotonNetwork.Instantiate(distractionPrefabs[Random.Range(0, distractionPrefabs.Length)]
        , CreateNewRandomPosition(), Quaternion.identity);
        distractions.Add(d);
        distractionMoveVectors.Add(new Vector3(Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f)).normalized * moveSpeed / 2f);
        distractionTargetPositions.Add(CreateNewRandomPosition());
        distractionSpawnTimes.Add(Time.time);
        distractionDestroyTimes.Add(Time.time + Random.Range(5f, 10f));
    }
    Vector3 CreateNewRandomPosition()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        float dist = Random.Range(0, gameAreaSize);
        float randomHeight = Random.Range(0.5f, 1.5f);
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
