using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnterRoomButton : Link {
    public GameObject errorPanel;

    public GameObject inputGameObject;
    TMP_InputField input;

    void Start() {
        input = inputGameObject.GetComponent<TMP_InputField>();
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);
    }

    public override void visitPage() {
        string roomName = input.text;
        LobbyManager.instance.EnterRoomClicked(roomName, errorResponse);
    }

    public void errorResponse(bool success, string message) {
        if (success) {
            nextPanel.SetActive(true);
        } else {
            errorPanel.SetActive(true);
        }
        thisPanel.SetActive(false);
    }

}