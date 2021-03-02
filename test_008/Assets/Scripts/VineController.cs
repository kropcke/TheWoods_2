using Photon.Pun;
using UnityEngine;

// TODO: PlayerConnector script should create two instances of VineController
public class VineController : MonoBehaviour {
    GameObject[] players;
    private GameObject phoneGameObject;
    public GameObject branchTipGameObject;
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

    public float SecondaryOffset = .6f;

    private SplineKitSpline spline;
    private SplineKitDecorator decorator;
    public bool flagReverse = false;

    // TODO: Random distribution, rotation

    private string playerName;

    public VineController(string player, Material lineMat) {
        // print("created VineController for " + player);
        playerName = player;
        this.lineMat = lineMat;
    }

    void Start() {
        // This will never be called as VineController isn't attached to any entities
    }
    bool isSetup = false;

    public void Setup(SplineKitSpline spline, SplineKitDecorator decorator, GameObject phone, GameObject branchTip, bool reverse) {
        this.spline = spline;
        this.decorator = decorator;

        phoneGameObject = phone;
        branchTipGameObject = branchTip;

        // Set up LineRenderer
        line = phone.AddComponent<LineRenderer>();
        line.material = lineMat;
        line.positionCount = 10;
        line.widthMultiplier = 0.004f;

        // Set up Catmul-Rom curve
        waypoints = new Transform[4];
        waypoints[0] = phoneGameObject.transform;

        // if (flagReverse) {
        //     SecondaryOffset = -SecondaryOffset;
        // }
        this.flagReverse = reverse;

        // Add additional waypoints to create a curve
        // One outside of A
        A1 = new GameObject("A1");
        // A1.transform.parent = A.transform;
        A1.transform.position = phoneGameObject.transform.position;
        // A1.transform.position += new Vector3(SecondaryOffset, 0, 0);
        waypoints[1] = A1.transform;

        // And one outside of B
        B1 = new GameObject("B1");
        // B1.transform.parent = B.transform;
        B1.transform.position = branchTipGameObject.transform.position;
        // B1.transform.position += new Vector3(-SecondaryOffset, 0, 0);
        waypoints[2] = B1.transform;

        waypoints[3] = branchTipGameObject.transform;
        int n = waypoints.Length;

        spline.Reset();
        decorator.lookForward = true;

        decorator.Run();

        isSetup = true;
        // print("Done " + playerName);
    }

    public void Update() {
        if (isSetup) {
            float distance = Vector3.Distance(phoneGameObject.transform.position, branchTipGameObject.transform.position);

            var delta = (phoneGameObject.transform.position - branchTipGameObject.transform.position);
            delta.y = 0;
            var normalized = delta.normalized;

            float offset = distance / 2;

            if (flagReverse) {
                offset *= -1;
            }


            delta *= -1;
            Vector3 offsetA1 = delta / 2;
            Vector3 offsetB1 = delta / 2;


            A1.transform.position = phoneGameObject.transform.position + offsetA1;
            B1.transform.position = branchTipGameObject.transform.position - offsetB1;

            // After the vines have been setup, run the real update loop
            spline.SetControlPoint(0, phoneGameObject.transform.position);
            spline.SetControlPoint(1, A1.transform.position);
            spline.SetControlPoint(2, B1.transform.position);
            spline.SetControlPoint(3, branchTipGameObject.transform.position);



            float resolution = 1f / (float)(line.positionCount - 1);
            for (int i = 0; i < line.positionCount - 1; i++) {
                line.SetPosition(i, spline.GetPoint((float)i * resolution));
            }
            line.SetPosition(line.positionCount - 1, spline.GetPoint(1f));

            // line.SetPosition(0, phoneGameObject.transform.position);
            // line.SetPosition(1, A1.transform.position);
            // line.SetPosition(2, B1.transform.position);
            // line.SetPosition(3, branchTipGameObject.transform.position);

            DrawSpline(Color.white);

        }

    }

    //Draws a line between every point and the next.
    public void DrawSpline(Color color) {
        Vector3 last = phoneGameObject.transform.position;
        for (float t = 0; t < 1f; t += .01f) {
            Vector3 next = spline.GetPoint(t);
            Debug.DrawLine(last, next, color);
            last = next;
        }
        Debug.DrawLine(last, branchTipGameObject.transform.position, color);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
}