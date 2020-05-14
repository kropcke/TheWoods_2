using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class linedraw : MonoBehaviour
{
    LineRenderer renderer;

   public Transform source;
   public Transform destination;

    private float dist;
    float counter;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<LineRenderer>();
       // renderer.SetPosition(0, source.position);
       // renderer.SetWidth(0.45f, 0.45f);

        //dist = Vector3.Distance(source.position, destination.position);

        
    }

    // Update is called once per frame
    void Update()
    {
        //if (counter < dist)
        //{
        //    counter = counter + (0.1f / 6f);
        //    float x = Mathf.Lerp(0, dist, counter);
        //    Vector3 point1 = source.position;
        //    Vector3 point2 = destination.position;

        //    Vector3 pointAlongLine = x * Vector3.Normalize(point2 - point1) + point1;

        //    renderer.SetPosition(1, pointAlongLine);
        //}

        renderer.SetPosition(0, source.position);
        renderer.SetPosition(1, destination.position);

    }
}
