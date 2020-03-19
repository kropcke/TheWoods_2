using UnityEngine;
using System.Collections;

public class Screenshot : MonoBehaviour {
	void Update() {
		if(Input.GetKeyDown(KeyCode.S))
		   {
			ScreenCapture.CaptureScreenshot(Time.realtimeSinceStartup + "_Screenshot.png");
		}
	}
}