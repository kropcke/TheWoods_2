using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotater : MonoBehaviour {
    public float speed = 2.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(Vector3.up * speed * Time.deltaTime);
	}
}
