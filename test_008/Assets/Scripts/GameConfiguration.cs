using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfiguration : MonoBehaviour {

    public List<AudioClip> distractionAudios;    

    public List<AudioClip> messageAudios;
    public string connectorPrefab;
    public AudioSource audioSource;
    internal bool debugMode;



    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
