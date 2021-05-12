using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class EulaLoader : MonoBehaviour
{

    public TextAsset eulaFile;
    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        text.text = eulaFile.text;

    }


}