using System.Collections;
using UnityEngine;

public class SplineDecorator : MonoBehaviour {

    public BezierSplineCalculator spline;

    public int frequency;

    public bool lookForward;

    public Transform[] items;

    public Transform[] instances;

    bool running = false;

    private float stepSize;

    void Start() { }

    public void Run() {
        instances = new Transform[frequency * items.Length];

        if (frequency <= 0 || items == null || items.Length == 0) {
            print("No items found @ SplineDecorator.Run()");

            return;
        }

        stepSize = frequency * items.Length;
        if (spline.Loop || stepSize == 1) {
            stepSize = 1f / stepSize;
        } else {
            stepSize = 1f / (stepSize - 1);
        }

        int inst = 0;
        for (int p = 0, f = 0; f < frequency; f++) {
            for (int i = 0; i < items.Length; i++, p++) {
                Transform item = Instantiate(items[i])as Transform;
                Vector3 position = spline.GetPoint(p * stepSize);
                item.transform.position = position;
                if (lookForward) {
                    item.transform.LookAt(position + spline.GetDirection(p * stepSize));
                }

                float r = 0.8f;
                r = Random.Range(.5f, 1.2f);
                r *= (float)p / (float)instances.Length;
                item.transform.localScale += new Vector3(r, r, r);
                item.transform.Rotate(0f, Random.Range(-10f, 45f), 0f);

                // item.transform.parent = transform;
                instances[inst] = item;
                inst++;
            }
        }

        running = true;
    }

    void Update() {
        if (running) {
            int p = 0;
            foreach (var item in instances) {
                Vector3 position = spline.GetPoint(p * stepSize);
                item.transform.position = position;
                p++;

                var delta = spline.GetDirection(p * stepSize);
                var angle = Quaternion.LookRotation(delta, Vector3.zero);
                item.transform.rotation = angle;
                item.transform.Rotate(90f, 0f, 0f);
            }
        }
    }
}