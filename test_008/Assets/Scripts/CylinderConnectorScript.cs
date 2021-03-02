using System.Collections;
using Photon.Pun;
using UnityEngine;

public class CylinderConnectorScript : MonoBehaviour {
    public GameObject StartObject;
    public GameObject EndObject;
    GameConfiguration variables;

    private bool enableCylinder = false;
    GameObject LinkObject;
    GameObject[] players;

    private Vector3 defaultLinkSize = new Vector3(0, 0, 0);
    private Quaternion defaultLinkRotation = Quaternion.Euler(0, 0, 0);

    private void CreatePrimitiveCylinderLink() {
        LinkObject = GameObject.Find("CylinderLink");
        Destroy(LinkObject.GetComponent<CapsuleCollider>());
        LinkObject.AddComponent<MeshCollider>();
        MeshCollider c = LinkObject.GetComponent<MeshCollider>();
        c.convex = true;
        c.isTrigger = true;
        LinkObject.AddComponent<Rigidbody>();
        Rigidbody rb = LinkObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start() {
        if (enableCylinder) {
            CreatePrimitiveCylinderLink();
        } else {
            variables = GameObject.FindGameObjectWithTag("GameConfiguration").GetComponent<GameConfiguration>();
            LinkObject = GameObject.Find("NewMiddleBranch");
            LinkObject.GetComponent<MeshRenderer>().enabled = true;
            LinkObject.transform.position = new Vector3(0, 0, 0);

        }
        LinkObject.transform.rotation = defaultLinkRotation;
        LinkObject.transform.localScale = defaultLinkSize;
    }

    void Update() {

        players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 2) {
            Vector3 MidPosition = StartObject.transform.position + ((EndObject.transform.position - StartObject.transform.position) * 0.5F);
            Vector3 Direction = (EndObject.transform.position - StartObject.transform.position).normalized;
            Direction.y = 0;
            LinkObject.transform.position = MidPosition;

            // LinkObject.transform.rotation = Quaternion.LookRotation(
            //     Direction, Vector3.right) * Quaternion.Euler(0, 0, 0);
            LinkObject.transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);
            Vector3 unused = EndObject.transform.position - StartObject.transform.position;
            
            // TODO: this is bad- don't do this
            if (enableCylinder) {

                LinkObject.transform.localScale = new Vector3(0.02F, Mathf.Min(unused.magnitude * 0.5f, 1f), 0.02F);
            } else {
                LinkObject.transform.localScale = new Vector3(3f, 2F, 3F);

            }
        } else {
            LinkObject.transform.localScale = defaultLinkSize;
        }
    }
}