using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenEulaButton : Link
{
    public string eulaLink = "http://wizaga.com/thewoods.html";
    private const string prefsKey = "eula_displayed";

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(visitPage);


        // If the EULA has already been read, skip this screen.
        if (PlayerPrefs.GetString(prefsKey, "false") == "true")
        {
            nextPanel.SetActive(true);
            thisPanel.SetActive(false);
        }
        
    }

    public override void visitPage()
    {
        Application.OpenURL(eulaLink);
        PlayerPrefs.SetString(prefsKey, "true");
        nextPanel.SetActive(true);
        thisPanel.SetActive(false);
    }

}