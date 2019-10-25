using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfiguration : MonoBehaviour {

    public List<AudioClip> distractionAudios;    

    public List<AudioClip> messageAudios;
    public string connectorPrefab;
    public AudioSource audioSource;
    internal bool debugMode;

    public string patchName;
    private int _pdPatch = -1;


    // Use this for initialization
    void Start () {
        UnityPD.Init();
        _pdPatch = UnityPD.OpenPatch ( patchName );
        audioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnApplicationQuit() {
        if (_pdPatch >= 0) 
            UnityPD.ClosePatch( _pdPatch );
        UnityPD.Deinit();
    }
}
