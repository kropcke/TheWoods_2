using System.Collections;
using UnityEngine;

public class SoundClipParticles : MonoBehaviour
{
    private bool debugMode = false; // todo: move to game configuration
    AudioClip c;
    ParticleSystem.Particle[] particles;
    public float distBetweenChannels;
    public float particleSize;
    public int spectrumSize;
    public float maxHeight;

    bool playing = false;

    // Use this for initialization
    void Start()
    {
        c = GetComponent<AudioSource>().clip;
        ParticleSystem.MainModule mainModule = GetComponent<ParticleSystem>().main;
        mainModule.maxParticles = c.channels;
        if (debugMode)
        {
            print("Channels: " + c.channels);
            print("Frequency: " + c.frequency);
            print("Length: " + c.length);
            print("Samples: " + c.samples);
        }
        InstantiateParticles();
        StartCoroutine(AnimateParticles());
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (debugMode)
            {
                print("Space pressed");
            }
            playing = !playing;
            if (GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Stop();
            }
            else
            {
                GetComponent<AudioSource>().Play();
            }
        }
    }
    private void InstantiateParticles()
    {
        particles = new ParticleSystem.Particle[spectrumSize];
        for (int i = 0; i < spectrumSize; i++)
        {
            particles[i].position = new Vector3(distBetweenChannels * i, 0, 0);
            particles[i].startSize = particleSize;
            particles[i].startColor = Color.red;
        }
        GetComponent<ParticleSystem>().SetParticles(particles, spectrumSize);

    }
    IEnumerator AnimateParticles()
    {
        while (true)
        {
            if (playing)
            {
                float[] samples = new float[spectrumSize];
                GetComponent<AudioSource>().GetSpectrumData(samples, 0, FFTWindow.Rectangular);
                for (int i = 0; i < spectrumSize; i++)
                {
                    particles[i].position = new Vector3(distBetweenChannels * i,
                    samples[i] * maxHeight, 0);
                    particles[i].startSize = particleSize;

                }
                GetComponent<ParticleSystem>().SetParticles(particles, spectrumSize);
                yield return null;
            }
            yield return null;
        }
    }
}
