using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgreeButton : Link
{

    public GameObject newBackground;
    void Start() {
        newBackground.gameObject.SetActive(true);
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);
    }

    public override void visitPage() {
        newBackground.gameObject.SetActive(false);
        nextPanel.SetActive(true);
        thisPanel.SetActive(false);
    }

}