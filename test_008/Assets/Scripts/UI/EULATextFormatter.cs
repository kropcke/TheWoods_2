using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class EULATextFormatter : MonoBehaviour
{
    private string eulaLink = "http://wizaga.com/TheWoods_PrivacyPolicy.html";
    private string eulaText = "";
    
    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        //WWW www = new WWW(eulaLink);
        //StartCoroutine(WaitForRequest(www));
        string eulaText = File.ReadAllText("Assets/Resources/EULA/eula.txt");
        gameObject.GetComponent<Text>().text = eulaText;

    }

    // Update is called once per frame
    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
    
        // check for errors
        if (www.error == null)
        {
           
            eulaText = www.text;
            Debug.Log("WWW Ok!: " + eulaText);
            gameObject.GetComponent<Text>().text = eulaText;
        } else {
            Debug.Log("WWW Error: "+ www.error);
        }    
    }
}
