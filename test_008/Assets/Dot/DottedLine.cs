using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class DottedLine : MonoBehaviour {

    public GameObject p0;
    public GameObject p1;
    public GameObject dot;

    [Range(0.01f, .5f)]
    public float size;

    public int count = 10;

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

        createDots(p0.transform.position, p1.transform.position);
    }

    // Update is called once per frame
    void Update() {

        if (line) {
            line.SetPosition(0, p0.transform.position);
            line.SetPosition(1, p1.transform.position);
        }

        updateDots(p0.transform.position, p1.transform.position);

    }

    void createDots(Vector3 start, Vector3 end) {
        Vector3 distance = end - start;
        Vector3 delta = distance / count;

        for (int i = 0; i < count; i++) {
            var g = GetOneDot();
            dots.Add(g);
            g.transform.position = start + i * delta;
        }
    }

    void updateDots(Vector3 start, Vector3 end) {
        Vector3 distance = end - start;
        Vector3 delta = distance / count;

        int i = 0;
        foreach (var d in dots) {
            d.transform.position = start + i * delta;
            i++;
        }
    }

    GameObject GetOneDot() {
        var gameObject = Instantiate(dot);
        gameObject.transform.localScale = Vector3.one * size;
        gameObject.transform.parent = transform;
        return gameObject;
    }

}