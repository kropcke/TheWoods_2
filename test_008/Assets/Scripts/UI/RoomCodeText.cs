using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCodeText : MonoBehaviour {
    TextMeshProUGUI text;
    bool set = false;

    void Start() {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        if (!set) {
            if (LobbyManager.instance != null) {
                if (LobbyManager.instance.roomName != null) {
                    text.text = LobbyManager.instance.roomName;
                    set = true;
                }
            }
        }
    }

}