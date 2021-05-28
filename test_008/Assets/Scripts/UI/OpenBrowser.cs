using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenBrowser : Link
{
    public string eulaLink = "http://wizaga.com/thewoods.html";

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);


        
    }

    public override void visitPage()
    {
        Application.OpenURL(eulaLink);
        // nextPanel.SetActive(true);
        // thisPanel.SetActive(false);
    }
}