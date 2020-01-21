using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class demo : MonoBehaviour {
    public Animator states;

    public Slider slider;
    public float sliderSpeed = 0.0f;

	// Use this for initialization
	void Start () {
        states = this.GetComponent<Animator>();
        slider = GameObject.Find("Slider").GetComponent<Slider>();
        slider.value = 1.0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown("1"))
        {
            Debug.Log("1");
            states.SetInteger("state", 1);

        }
        if (Input.GetKeyDown("2"))
        {
            Debug.Log("2");
            states.SetInteger("state", 2);

        }

        int layer = states.GetLayerIndex("main");

        // Debug.Log(states.GetCurrentAnimatorStateInfo(layer).shortNameHash);
        sliderSpeed = slider.value;


        // UnityEditor.Animations.AnimatorControllerLayer[] layers = states.layers;

        states.speed = sliderSpeed;



    }
}
