using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class RestartScript : MonoBehaviour {

    [SerializeField] MeshRenderer target;
    SpriteRenderer srend;

    void Awake()
    {
        srend = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {

        Debug.Log("OnMouseDown");
    }

}
