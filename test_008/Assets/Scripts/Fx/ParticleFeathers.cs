using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFeathers : MonoBehaviour {
    // Particle System GameObjects
    public List<GameObject> emitters = new List<GameObject>();
    // Flag for enabling the system
    public bool enabled = false;
    public float runtime = 3f;
    private float time = 0f;

    // Start is called before the first frame update
    void Start() {
        StopAll();
    }

    // Update is called once per frame
    void Update() {
        if (enabled) {
            if (time == 0f) {
                StartAll();
            }

            if (time < runtime) {
                time += Time.deltaTime;
            }

            if (time > runtime) {
                enabled = false;
                time = 0f;
                StopAll();
            }
        }

    }

    void StartAll() {
        foreach (var e in emitters) {
            var particles = e.GetComponent<ParticleSystem>();
            particles.Play();
        }
    }

    void StopAll() {
        foreach (var e in emitters) {
            var particles = e.GetComponent<ParticleSystem>();
            particles.Stop();
        }
    }
}