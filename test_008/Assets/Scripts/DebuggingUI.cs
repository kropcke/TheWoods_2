using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggingUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private float sliderValue = 1.0f;
    private float maxSliderValue = 10.0f;

    public GUISkin skin;

    void OnGUI()
    {
        GUI.skin = skin;
        //// Make a group on the center of the screen
        //GUI.BeginGroup(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 300, 200));
        //// All rectangles are now adjusted to the group. (0,0) is the topleft corner of the group.

        //// We'll make a box so you can see where the group is on-screen.
        //GUI.Box(new Rect(0, 0, 300, 400), "Group is here");
        //if(GUI.Button(new Rect(10, 40, 280, 150), "Press to vibrate")){
        //    Handheld.Vibrate();
        //}

        //// End the group we started above. This is very important to remember!
        //GUI.EndGroup();

        GUI.Label(new Rect(10, 10, 100, 100), Mathf.Floor((1.0f / Time.smoothDeltaTime)).ToString());
    }
}
