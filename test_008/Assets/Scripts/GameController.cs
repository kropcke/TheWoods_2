using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : MonoBehaviour {

    // The two player game objects created in the scene
    public GameObject player1, player2;
    // The server game object in the scene
    public GameObject server;

    // A prefab of the shadow guy to spawn
    public GameObject shadowPrefab;
    // A prefab of the objective to place around
    public GameObject objectivesPrefab;
    // A prefab of the orb to use
    public GameObject orbPrefab;


    // The number of shadows to create
    public int numShadows;
    // The number of objectives to create on the ground
    public int numObjectives;
    // The height (in meters) difference for a player to pass the orb
    public float orbPassHeightDifference;
    // The time it takes to successfully transfer the cube
    public float orbPassTimeRequirement;

    private bool running;
    GameObject currentOrbHolder;
    List<GameObject> objectives;
    List<GameObject> shadows;


	void Start () {
        running = true;
        /* Each update we need to:
         * 1. Check if the players are attempting to switch posession of the orb
         * 2. Update the position of the orb and the lighting based on the orb
         * 3. See if a player has "picked-up" an objective from the ground
         * 4. Update shadow positions
         * 5. Check if a shadow has hit a player
         * 6. Make a player's phone beep based on how far the shadows are
         */

        // Start a Coroutine to handle the position switching
        StartCoroutine(OrbSwitchingRoutine());
	}

    private IEnumerator OrbSwitchingRoutine(){
        // Bools to see if we're attempting to pass the orb
        bool passingToPlayer1 = false, passingToPlayer2 = false;
        float timeInState = 0, startTimeInState = 0;
        while(running){
            if(player1.transform.position.y - player2.transform.position.y > 
               orbPassHeightDifference){
                if(passingToPlayer2){
                    timeInState = Time.time - startTimeInState;
                }
                else{
                    startTimeInState = Time.time;
                    timeInState = 0;
                }
                passingToPlayer2 = true;
                passingToPlayer1 = false;
            }
            else if(player2.transform.position.y - player1.transform.position.y >
                    orbPassHeightDifference){
                if(passingToPlayer1){
                    timeInState = Time.time - startTimeInState;
                }
                else{
                    startTimeInState = Time.time;
                    timeInState = 0;
                }
                passingToPlayer1 = true;
                passingToPlayer2 = false;
            }
            else{
                startTimeInState = Time.time;
                timeInState = 0;
                passingToPlayer2 = passingToPlayer1 = false;
            }

            if(timeInState > orbPassTimeRequirement && passingToPlayer1 
               && currentOrbHolder != player1){
                currentOrbHolder = player1;
            }
            else if(timeInState > orbPassTimeRequirement && passingToPlayer2 
                    && currentOrbHolder != player2){
                currentOrbHolder = player2;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

}
