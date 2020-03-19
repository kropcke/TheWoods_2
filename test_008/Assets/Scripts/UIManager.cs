
using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
public class UIManager : MonoBehaviour, IPunObservable
{

	//GameObject[] pauseObjects;
	GameObject[] finishObjects;
    GameObject server;
    GameObject[] players;
	//PlayerController playerController;
	//// Use this for initialization
	void Start () {
	//	Time.timeScale = 1;

	//	pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");			//gets all objects with tag ShowOnPause
	finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");          //gets all objects with tag ShowOnFinish

        server = GameObject.FindGameObjectWithTag("Server");
	//	hidePaused();
		hideFinished();

	//	//Checks to make sure MainLevel is the loaded level
	//	if(Application.loadedLevelName == "MainLevel")
	//		playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}
	
	//// Update is called once per frame
	void Update () {
       
        showFinished();
        if (ServerScript.gameOver == true)
        {
            
            //if (gameObject.tag == "Player")
            //{
            //    transform.GetChild(2).gameObject.SetActive(true);
            //}
            showFinished();
        }
        else
        {
            hideFinished();
        }
    }

	//Reloads the Level
	public void Reload(){
        Debug.Log("Gameover");
        server.GetComponent<ServerScript>().Reset();
    }

    //	//controls the pausing of the scene
    //	public void pauseControl(){
    //			if(Time.timeScale == 1)
    //			{
    //				Time.timeScale = 0;
    //				showPaused();
    //			} else if (Time.timeScale == 0){
    //				Time.timeScale = 1;
    //				hidePaused();
    //			}
    //	}

    //	//shows objects with ShowOnPause tag
    //	public void showPaused(){
    //		foreach(GameObject g in pauseObjects){
    //			g.SetActive(true);
    //		}
    //	}

    //	//hides objects with ShowOnPause tag
    //	public void hidePaused(){
    //		foreach(GameObject g in pauseObjects){
    //			g.SetActive(false);
    //		}
    //	}

    //shows objects with ShowOnFinish tag
    public void showFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(true);
        }
    }

    //hides objects with ShowOnFinish tag
    public void hideFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(false);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       // throw new System.NotImplementedException();
    }

    //	//loads inputted level
    //	public void LoadLevel(string level){
    //		Application.LoadLevel(level);
    //	}

    //}

}
