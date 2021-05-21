using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.IO;


public class EnterRoomButton : Link
{
    public GameObject errorPanel;
    private const string apiURL = "http://thewoods-blocklist-postgres-production.up.railway.app/blocklist/";

    public GameObject inputGameObject;
    TMP_InputField input;

    void Start()
    {
        input = inputGameObject.GetComponent<TMP_InputField>();
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);
    }

    public override void visitPage()
    {
        string roomName = input.text;
        var blocklist = GetBlocklist(apiURL);

        foreach (string i in blocklist)
        {
            print(i);
        }
        if (blocklist.Contains(roomName))
        {
            print("Tried to join blocked room: " + roomName);
            errorPanel.SetActive(true);
            thisPanel.SetActive(false);
        }


        LobbyManager.instance.EnterRoomClicked(roomName, errorResponse);
    }

    public void errorResponse(bool success, string message)
    {
        print(success);
        print(message);
        if (success)
        {
            nextPanel.SetActive(true);
        }
        else
        {
            errorPanel.SetActive(true);
        }
        thisPanel.SetActive(false);
    }

    List<string> GetBlocklist(string url)
    {

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());

        string arrayJSON = reader.ReadToEnd();

        var minusBrackets = arrayJSON.Replace("[", "").Replace("]", "");

        List<string> result = new List<string>(minusBrackets.Split(','));


        return result;


    }

}