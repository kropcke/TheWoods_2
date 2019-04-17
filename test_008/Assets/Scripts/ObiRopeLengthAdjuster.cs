using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObiRopeLengthAdjuster : MonoBehaviour {

    public GameObject obiRope;
    public GameObject Endpoint1, Endpoint2;



	// Update is called once per frame
	void Update () {
        obiRope.GetComponent<Obi.ObiRopeCursor>().ChangeLength(
        Vector3.Distance(Endpoint1.transform.position, Endpoint2.transform.position));
    }
}
