using UnityEngine;
using System.Collections;
using Photon.Pun;

public class CylinderConnectorScript : MonoBehaviour
{
    public GameObject StartObject;
    public GameObject EndObject;
    GameConfiguration variables;

    private bool enableCylinder = true;

    GameObject LinkObject;
    GameObject[] players;

    private Vector3 defaultLinkSize = new Vector3(0, 0, 0);
    private Quaternion defaultLinkRotation = Quaternion.Euler(0, 0, 0);

    private void CreatePrimitiveCylinderLink()
    {
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

    void Start()
    {     
        if (enableCylinder)
        {
            CreatePrimitiveCylinderLink();
        }
        else
        {
            variables = GameObject.FindGameObjectWithTag("GameConfiguration").GetComponent<GameConfiguration>();
            LinkObject = GameObject.Find("MiddleBranch");
            //LinkObject.gameObject.SetActive(true);
            //LinkObject.GetComponent<this>.enabled = true;
            //LinkObject.SetActive(true);
            
            LinkObject.transform.position = new Vector3(0, 1, 0);
            
        }
        LinkObject.transform.rotation = defaultLinkRotation;
        LinkObject.transform.localScale = defaultLinkSize;
        //Debug.Log("LinkObject is {0}", LinkObject);
    }

    void Update()
    {

        players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 2)
        {

            Vector3 MidPosition = StartObject.transform.position + ((EndObject.transform.position - StartObject.transform.position) * 0.5F);
            Vector3 Direction = (EndObject.transform.position - StartObject.transform.position).normalized;
            LinkObject.transform.position = MidPosition;

            LinkObject.transform.rotation = Quaternion.LookRotation(Direction, Vector3.right) * Quaternion.Euler(90, 0, 0);
            Vector3 unused = EndObject.transform.position - StartObject.transform.position;
            if (enableCylinder)
            {
                
                LinkObject.transform.localScale = new Vector3(0.02F, Mathf.Min(unused.magnitude * 0.5f, 1f), 0.02F);
            }
            else
            {
                LinkObject.transform.localScale = new Vector3(unused.magnitude * 4f, 4F, 4F);

            }
            //  }
            //change these measurements after the game object
            //LinkObject.transform.localScale = new Vector3(1F, 1F, 2F);
            //Debug.LogFormat("StartPosition: {0} , EndPosition: {1} , MidPosition: {2}, Direction: {3} , Scale: {4}"
            //          , StartObject.transform.position
            //          , EndObject.transform.positionÏ
            //          , MidPosition
            //          , Quaternion.LookRotation(Direction, Vector3.right) * Quaternion.Euler(90, 0, 0)
            //	, new Vector3(0.02F, ((EndObject.transform.position - StartObject.transform.position) * 0.5F).magnitude, 0.02F));
        } else
        {
            LinkObject.transform.localScale = defaultLinkSize;
        }

    }
}
