using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindMidPoint : MonoBehaviour {

    public Transform target;
    public Transform targetPartner;
    //private Vector3 direction = Vector3.zero;
    private Vector3 center = Vector3.zero;

    public void Update()
    {
        //direction = targetPartner.position - target.position;
        //float dirMag = direction.magnitude;
        //direction.Normalize();
        //center = direction * dirMag * 0.5F;

        center = (targetPartner.position + target.position) / 2;
        transform.position = center;
    }


	//public void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(center, 0.2F);
    //}
}
