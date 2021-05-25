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
    public GameObject blockedPanel;
    private const string apiURL = "http://thewoods-blocklist-postgres-production.up.railway.app/blocklist/";

    public GameObject inputGameObject;
    TMP_InputField input;

    bool blockedRoom = false; // set to `true` if user tried to join a room in the blocklist

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

        if (blocklist.Contains(roomName))
        {
            print("Tried to join blocked room: " + roomName);
            blockedRoom = true;
        } else {
            blockedRoom = false;
        }


        LobbyManager.instance.EnterRoomClicked(roomName, errorResponse);
    }

    public void errorResponse(bool success, string message)
    {
        if (success)
        {
            nextPanel.SetActive(true);
        }
        else
        {
            if (blockedRoom == true)
            {
                print("ree");
                blockedPanel.SetActive(true);
            }
            else
            {
                print("peen");
                errorPanel.SetActive(true);

            }
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