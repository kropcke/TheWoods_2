using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.IO;


public class BlockRoomButton : Link
{
    private const string apiURL = "http://thewoods-blocklist-postgres-production.up.railway.app/block/";


    public GameObject errorPanel;

    public GameObject inputGameObject;
    TMP_InputField input;

    static readonly HttpClient httpClient = new HttpClient();

    void Start()
    {
        input = inputGameObject.GetComponent<TMP_InputField>();
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);
    }

    public override void visitPage()
    {
        string roomName = input.text;

        string result = AddToBlocklist(roomName);
        print(result);
        nextPanel.SetActive(true);
        thisPanel.SetActive(false);
    }



    string AddToBlocklist(string room)
    {
        string url = apiURL + room;
        print(url);

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());

        return reader.ReadToEnd();


    }

}