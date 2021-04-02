using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomButton : Link {
    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);
    }

    public override void visitPage() {
        nextPanel.SetActive(true);
        thisPanel.SetActive(false);
    }

}