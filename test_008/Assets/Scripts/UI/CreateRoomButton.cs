using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomButton : Link {
    string roomName = "eheeee"; // todo randomly generate

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);
    }

    public override void visitPage() {
        roomName = psuedoUniqueID(6);
        print(roomName);
        LobbyManager.instance.CreateRoomClicked(roomName);

        nextPanel.SetActive(true);
        thisPanel.SetActive(false);
    }

    string psuedoUniqueID(int length) {
        char[] characters = "0123456789".ToCharArray();
        System.Random rnd = new System.Random();

        string id = "";
        for (int i = 0; i < length; i++) {
            int j = rnd.Next(0, characters.Length);
            id += characters[j];
        }

        return id;
    }

}