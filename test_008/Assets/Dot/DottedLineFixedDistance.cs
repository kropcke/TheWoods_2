using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class DottedLineFixedDistance : MonoBehaviour {

    public GameObject p0;
    public GameObject p1;
    public GameObject dot;

    [Range(0.01f, 1f)]
    public float size;

    [Range(0.1f, 2f)]
    public float delta;

    private LineRenderer line;

    List<GameObject> dots = new List<GameObject>();
    List<Vector2> positions = new List<Vector2>();

    // Start is called before the first frame update
    void Start() {
        line = GetComponent<LineRenderer>();
        if (line) {
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
        }

    }

    // Update is called once per frame
    void Update() {

        if (line) {
            line.SetPosition(0, p0.transform.position);
            line.SetPosition(1, p1.transform.position);
        }

        // updateDots(p0.transform.position, p1.transform.position);

        createDots(p0.transform.position, p1.transform.position);
    }

    void createDots(Vector3 start, Vector3 end) {
        foreach (var d in dots) {
            Destroy(d);
        }
        dots.Clear();

        Vector3 distance = end - start;
        Vector3 step = distance * delta;

        Vector3 direction = (end - start).normalized;
        Vector3 pointer = start;

        while ((end - start).magnitude > (pointer - start).magnitude) {
            var g = GetOneDot();
            g.transform.position = pointer;
            dots.Add(g);
            pointer += (direction * delta);
        }
    }
    GameObject GetOneDot() {
        var gameObject = Instantiate(dot);
        gameObject.transform.localScale = Vector3.one * size;
        gameObject.transform.parent = transform;
        return gameObject;
    }

}