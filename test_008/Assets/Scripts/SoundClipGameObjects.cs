using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(AudioSource))]
public class SoundClipGameObjects : MonoBehaviour
{
    private bool debugMode = false; // todo: move to GameConfiguration
    AudioSource s;
    AudioClip c;
    public float visObjectSize;
    public int spectrumSize;
    public float maxHeight;

    bool activated = false;
    bool playing = false;
    //float[] freqBands = { 20, 60, 250, 500, 2000, 4000, 6000, 20000 };
    float[] freqBands = { 20, 30, 60, 120, 240, 480, 960, 1920, 3840, 4800, 6000, 8000, 16000, 17000, 18500, 20000 };
    List<Vector3> savedScales;
    List<Vector3> savedPositions;
    List<Vector3> savedRotations;
    List<GameObject> bars;

    // Use this for initialization
    public void Init()
    {
        s = GetComponent<AudioSource>();
        c = s.clip;
        savedScales = new List<Vector3>();
        savedPositions = new List<Vector3>();
        savedRotations = new List<Vector3>();
        bars = new List<GameObject>();

        for (int i = 0; i < freqBands.Length; i++)
        {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.localScale = new Vector3(visObjectSize, visObjectSize, visObjectSize);
            c.GetComponent<Renderer>().material.color = new Color(0.5f, 0.2f, 0f);
            bars.Add(c);

        }
    }

    // Update is called once per frame
    void Update()
    {

        if (playing)
        {
            UpdateVisualization();
            if (!s.isPlaying) StopPlaying();
        }
    }

    void UpdateVisualization()
    {
        float[] samples = new float[spectrumSize];
        s.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        float[] freqBandsAmps = SamplesToFreqBands(samples);
        for (int i = 0; i < freqBands.Length; i++)
        {
            bars[i].transform.localScale = new Vector3(visObjectSize, maxHeight * freqBandsAmps[i], visObjectSize);
            bars[i].transform.position = savedPositions[i] +
            bars[i].transform.up * bars[i].transform.localScale.y / 2f;
        }
        if (savedScales.Count == 0 && s.time >= c.length / 2f)
        {
            for (int i = 0; i < freqBands.Length; i++)
            {
                savedScales.Add(bars[i].transform.localScale);
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < freqBands.Length; i++)
        {
            bars[i].transform.localScale = new Vector3(visObjectSize, visObjectSize / 2f, visObjectSize);
            bars[i].transform.position = savedPositions[i] +
                bars[i].transform.up * bars[i].transform.localScale.y / 2f;
        }
    }

    void SetBarsToSavedScales()
    {
        for (int i = 0; i < bars.Count; i++)
        {
            bars[i].transform.localScale = savedScales[i];
            bars[i].transform.position = savedPositions[i] +
            bars[i].transform.up * bars[i].transform.localScale.y / 2f;
        }
    }

    public void Activate()
    {
        for (int i = 0; i < bars.Count; i++)
        {
            bars[i].GetComponent<Renderer>().material.color = new Color(0.5f, 0.2f, 0f);
        }
    }

    public void StartPlaying()
    {
        if (debugMode)
        {
            print("Starting playing if not playing " + gameObject.name);
        }
        if (!playing)
        {
            
            playing = true;
            s.time = 0;
            s.Play();
        }
    }

    public void StopPlaying()
    {
        if (debugMode)
        {
            print("Stopping playing if playing " + gameObject.name);
        }
        if (playing)
        {
            playing = false;
            s.time = 0;
            s.Stop();
            SetBarsToSavedScales();
        }
    }

    public void PositionInCircle(int startIndex, int numPartitions, float radius)
    {
        for (int i = startIndex; i < startIndex + bars.Count; i++)
        {
            Vector3 pos = new Vector3();
            float xPos = radius * Mathf.Sin((2 * i * Mathf.PI) / (float)numPartitions);
            float zPos = radius * Mathf.Cos((2 * i * Mathf.PI) / (float)numPartitions);
            float yPos = 0;
            pos = new Vector3(xPos, yPos, zPos);
            savedPositions.Add(pos);
            bars[i - startIndex].transform.position = pos;

            bars[i - startIndex].transform.Rotate(new Vector3(90, 0, 0));
            bars[i - startIndex].transform.RotateAround(bars[i - startIndex].transform.position,
            bars[i - startIndex].transform.forward, (-360 * i) / numPartitions);

            bars[i - startIndex].transform.position +=
            bars[i - startIndex].transform.up *
                bars[i - startIndex].transform.localScale.y / 2f;
            //bars[i - startIndex].GetComponent<Renderer>().material.color =
            //   Color.HSVToRGB(startIndex / (float)(numPartitions), 1, 1);
        }
    }

    float[] SamplesToFreqBands(float[] samples)
    {
        float[] freqBandAmps = new float[freqBands.Length];
        float HzPerSample = 22050f / spectrumSize;
        int currentSpot = 0;
        float total = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            freqBandAmps[currentSpot] += samples[i];
            total += samples[i];
            float currentFreq = i * HzPerSample;
            if (currentSpot + 1 < freqBands.Length)
            {
                if (currentFreq > freqBands[currentSpot + 1])
                {
                    currentSpot += 1;
                }
            }

        }
        for (int i = 0; i < freqBandAmps.Length; i++)
        {
            freqBandAmps[i] = freqBandAmps[i] / (total + 0.1f);
        }
        return freqBandAmps;
    }
}
