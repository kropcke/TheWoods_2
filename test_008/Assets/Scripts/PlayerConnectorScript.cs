using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerConnectorScript : MonoBehaviour {
    GameObject[] players;
    GameObject middleBranch;
    public GameObject dot;
    List<GameObject> dots1 = new List<GameObject>();
    List<GameObject> dots2 = new List<GameObject>();

    [Range(0.0001f, 1f)]
    public float size;

    [Range(0.01f, 2f)]
    public float delta;

    bool createdots = true;
    public int count;

    public SplineKitSpline player1Spline;
    public SplineKitSpline player2Spline;
    public SplineKitDecorator player1Decorator;
    public SplineKitDecorator player2Decorator;

    private VineController player1Vine;
    private VineController player2Vine;

    public GameObject branch;
    private GameObject branchTip1;
    private GameObject branchTip2;

    bool ready = false;

    void Start() {
        Material lineMat = Resources.Load<Material>("LineVine");
        player1Vine = new VineController("player1", lineMat);
        player2Vine = new VineController("player2", lineMat);

        player1Spline = GameObject.Find("Spline1").GetComponent<SplineKitSpline>();
        player2Spline = GameObject.Find("Spline2").GetComponent<SplineKitSpline>();

        player1Decorator = GameObject.Find("Decorator1").GetComponent<SplineKitDecorator>();
        player2Decorator = GameObject.Find("Decorator2").GetComponent<SplineKitDecorator>();

        branchTip1 = GameObject.Find("BranchTip2");
        branchTip2 = GameObject.Find("BranchTip1");

    }

    // Update is called once per frame
    void Update() {
        // make sure VineController.Setup() is called only once

        if (!ready) {
            print("something awesome");

            players = GameObject.FindGameObjectsWithTag("Player");
            print(players.Length);
            middleBranch = GameObject.Find("NewMiddleBranch");
            if (players.Length == 2) {
                print("players something");
                if (players[0].GetPhotonView().ViewID > players[1].GetPhotonView().ViewID) {
                    print("setup vine 1");
                    GameObject phone1 = players[0].transform.GetChild(1).gameObject;
                    GameObject phone2 = players[1].transform.GetChild(1).gameObject;
                    player1Vine.Setup(
                        player1Spline,
                        player1Decorator,
                        players[0].transform.GetChild(1).gameObject, branchTip1,
                        false
                    );
                    player2Vine.Setup(
                        player2Spline,
                        player2Decorator,
                        players[1].transform.GetChild(1).gameObject, branchTip2,
                        false
                    );

                } else {
                    print("setup vine 2");
                    GameObject phone1 = players[1].transform.GetChild(1).gameObject;
                    GameObject phone2 = players[0].transform.GetChild(1).gameObject;
                    player1Vine.Setup(
                        player1Spline,
                        player1Decorator,
                        players[1].transform.GetChild(1).gameObject, branchTip1,
                        false
                    );
                    player2Vine.Setup(
                        player2Spline,
                        player2Decorator,
                        players[0].transform.GetChild(1).gameObject, branchTip2,
                        true
                    );
                }
                ready = true;
            }

        } else {
            // player1Vine.Update();
            // player2Vine.Update();
        }
    }

    void LateUpdate() {
        if (ready) {
            player1Vine.Update();
            player2Vine.Update();
        }
    }

    public void OnDrawGizmos() {
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(branchTip1.transform.position, .05f);
        // Gizmos.DrawSphere(branchTip2.transform.position, .05f);
    }

    // void connectPlayerToBranch(GameObject gameObj1, GameObject gameObj2) {
    //     if (createdots) {
    //         dots1 = createDots(middleBranch.transform.GetChild(4).transform.position, gameObj1.transform.position);
    //         dots2 = createDots(middleBranch.transform.GetChild(5).transform.position, gameObj2.transform.position);
    //         createdots = false;
    //     }
    //     if (dots1 != null) {
    //         updateDots(middleBranch.transform.GetChild(4).transform.position, gameObj1.transform.position, dots1);
    //     }
    //     if (dots2 != null) {
    //         updateDots(middleBranch.transform.GetChild(5).transform.position, gameObj2.transform.position, dots2);
    //     }
    // }

    // List<GameObject> createDots(Vector3 start, Vector3 end) {

    //     Vector3 distance = end - start;
    //     Vector3 delta = distance / count;
    //     List<GameObject> dots = new List<GameObject>();

    //     for (int i = 0; i < count; i++) {
    //         var g = GetOneDot();
    //         g.transform.localScale = Vector3.one * size;
    //         g.transform.parent = transform;
    //         dots.Add(g);

    //     }

    //     return dots;
    // }

    // void updateDots(Vector3 start, Vector3 end, List<GameObject> dots) {
    //     Vector3 distance = end - start;
    //     Vector3 delta = distance / count;

    //     int i = 0;
    //     foreach (var d in dots) {
    //         d.transform.position = start + i * delta;
    //         i++;
    //     }
    // }

    // GameObject GetOneDot() {
    //     var gameObject1 = Instantiate(dot);
    //     gameObject1.transform.localScale = Vector3.one * size;
    //     gameObject1.transform.parent = transform;
    //     return gameObject1;
    // }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
}