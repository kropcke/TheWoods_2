using Photon.Pun;
using UnityEngine;

// TODO: PlayerConnector script should create two instances of VineController
public class VineController : MonoBehaviour {
    GameObject[] players;
    private GameObject A;
    public GameObject B;
    private GameObject A1;
    private GameObject B1;

    private LineRenderer line;
    public Material lineMat;

    public Transform[] waypoints;

    public float drawResolution = 0.1f;
    public float superSampleResolution = 0.25f;
    public float tension = 0.5f;
    public bool displayTangents = false;
    public bool displayPath = true;

    public float SecondaryOffset = 3f;

    public BezierSplineCalculator spline;
    public SplineDecorator decorator;
    public bool flagReverse = false;

    // TODO: Random distribution, rotation

    void Start() {
        A = gameObject;

        // Set up LineRenderer
        line = gameObject.AddComponent<LineRenderer>();
        line.material = lineMat;
        line.positionCount = 10;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;

        // Set up Catmul-Rom curve
        waypoints = new Transform[4];
        waypoints[0] = A.transform;

        // if (flagReverse) {
        players = GameObject.FindGameObjectsWithTag("Player");
        if (players[0].GetPhotonView().ViewID > players[1].GetPhotonView().ViewID) {
            SecondaryOffset = -SecondaryOffset;
        } else {

        }

        // Add additional waypoints to create a curve
        // One outside of A
        A1 = new GameObject("A1");
        A1.transform.parent = A.transform;
        A1.transform.position = A.transform.position;
        A1.transform.position += new Vector3(SecondaryOffset, 0, 0);
        waypoints[1] = A1.transform;

        // And one outside of B
        B1 = new GameObject("B1");
        B1.transform.parent = B.transform;
        B1.transform.position = B.transform.position;
        B1.transform.position += new Vector3(-SecondaryOffset, 0, 0);
        waypoints[2] = B1.transform;

        waypoints[3] = B.transform;
        int n = waypoints.Length;
        // Debug.Log("There are " + n + " waypoints");

        spline.Reset();
        decorator.lookForward = true;

        decorator.Run();
    }

    void Setup() {
        if (players[0].GetPhotonView().ViewID > players[1].GetPhotonView().ViewID) {
            // connectPlayerToBranch(players[0].transform.GetChild(1).gameObject, players[1].transform.GetChild(1).gameObject);
        } else {
            // connectPlayerToBranch(players[1].transform.GetChild(1).gameObject, players[0].transform.GetChild(1).gameObject);
        }
    }

    bool isSetup = false;

    void Update() {
        if (!isSetup) {
            // When 2 players join, setup the vines
            if (players.Length == 2) {
                Setup();
                isSetup = true;
            }
        } else {
            // After the vines have been setup, run the real update loop
            spline.SetControlPoint(0, waypoints[0].position);
            spline.SetControlPoint(1, waypoints[1].position);
            spline.SetControlPoint(2, waypoints[2].position);
            spline.SetControlPoint(3, waypoints[3].position);

            float resolution = 1f / (float)(line.positionCount - 1);
            for (int i = 0; i < line.positionCount; i++) {
                line.SetPosition(i, spline.GetPoint((float)i * resolution));
            }

            DrawSpline(Color.white);
        }

    }

    //Draws a line between every point and the next.
    public void DrawSpline(Color color) {
        Vector3 last = A.transform.position;
        for (float t = 0; t < 1f; t += .01f) {
            Vector3 next = spline.GetPoint(t);
            Debug.DrawLine(last, next, color);
            last = next;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
}