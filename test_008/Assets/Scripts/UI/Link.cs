using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Link : MonoBehaviour {
    [HideInInspector]
    public Button button;
    public GameObject thisPanel;
    public GameObject nextPanel;

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);
    }

    public abstract void visitPage();

}