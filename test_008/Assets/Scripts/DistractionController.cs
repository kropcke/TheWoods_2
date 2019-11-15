using UnityEngine;
using System.Collections;
using Photon.Pun;
public class DistractionController : MonoBehaviourPunCallbacks
{
	GameConfiguration variables;
	public static bool inDistraction;
    public bool debugMode = true; // todo: Move to gamecontroller.

    public GameObject origin;
    public float dx;
	// Use this for initialization
	void Start()
	{
		variables = GameObject.FindGameObjectWithTag("GameConfiguration").GetComponent<GameConfiguration>();
		variables.audioSource.loop = true;
        inDistraction = false;
    }

	// Update is called once per frame
	void Update()
	{
        dx = (origin.transform.position - gameObject.transform.position).sqrMagnitude;
        //TODO :Implement in a Better way - volume fadein and fadeout
        //GetComponent<AudioSource>().volume =  ( 1/dx);

    }


	private void OnTriggerEnter(Collider other)
	{

        if (photonView.IsMine && other.gameObject.tag == "ConnectorLink")
		{
            //Debug.LogFormat("Tag: {0}", other.gameObject.tag);
			inDistraction = true;
            if (debugMode)
            {
                print(" ========== Cloud trigger enter.");
            }
			PlayRandomDistractionAudio();
			//Invoke("PlayRandomDistractionAudio", 0.1f);
		}
	}

	private void PlayRandomDistractionAudio()
	{
		int audioCtr = Random.Range(0, variables.distractionAudios.Count);
		AudioClip current = variables.distractionAudios[audioCtr];
		StopDistractionAudioClipWithoutFade();
		variables.audioSource.clip = current;
		StartCoroutine(FadeIn(variables.audioSource, 1f));
	}

	private void OnTriggerExit(Collider other)
	{
		if (photonView.IsMine && other.gameObject.tag == "ConnectorLink")
		{
			inDistraction = false;
            if (debugMode)
            {
                print(" ========== Cloud trigger exit.");
            }
			StopDistractionAudioClip();
		}

	}

	private void StopDistractionAudioClipWithoutFade()
	{
		variables.audioSource.loop = false;
		variables.audioSource.volume = 0;
		variables.audioSource.Stop();
	}

	private void StopDistractionAudioClip()
	{
		Invoke("StopDistractionAudioClipWithoutFade", 3f);
		StartCoroutine(FadeOut(variables.audioSource, 1f));
	}

	private void OnTriggerStay(Collider other)
	{

        Handheld.Vibrate();
		if (photonView.IsMine && other.gameObject.tag == "ConnectorLink")
		{
            if (debugMode)
            {
                Debug.Log(" ========== Cloud trigger stay.");
            }
			inDistraction = true;
		}
	}

	//TODO: Move these to a different class
	public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
	{
		float startVolume = audioSource.volume;
		audioSource.loop = false;
		//Debug.LogFormat("startVolume:{0}", startVolume);
		while (audioSource.volume > 0)
		{
			audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
			//Debug.LogFormat("audioSource.volume:{0}", audioSource.volume);
			yield return null;
		}
		audioSource.Stop();
	}

	public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
	{
		audioSource.time = 0;
		audioSource.loop = true;
		audioSource.Play();
		audioSource.volume = 0f;
		while (audioSource.volume < 1)
		{
			audioSource.volume += Time.deltaTime / FadeTime;
			yield return null;
		}
	}
}
