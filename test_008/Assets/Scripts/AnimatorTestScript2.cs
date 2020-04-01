using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Realtime;
using Photon.Pun;

public class AnimatorTestScript2 : MonoBehaviour
{
    // Start is called before the first frame update
    Animator anim;
    public string paramter;
    int current;
    void Start()
    {
        anim = GetComponent<Animator>();
        current = 0;
    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log(current);
            anim.SetInteger(paramter, current);
            current++;
        }


        // Loop the current state
        if (current > 2)
        {
            current = 0;
        }

    }

    public void SetState2(string s)
    {
        if (s == "flying")
        {
            anim.SetInteger(paramter, 0);

        }
        else if (s == "perched")
        {
            anim.SetInteger(paramter, 1);
        }
        else if (s == "singing")
        {
            anim.SetInteger(paramter, 1);
        }
    }

    [PunRPC]
    public void SetState(string s)
    {
        if (s == "flying")
        {
            anim.SetInteger(paramter, 2);

        }
        else if (s == "perched")
        {
            anim.SetInteger(paramter, 1);
        }
        else if (s == "singing")
        {
            anim.SetInteger(paramter, 1);
        }
    }
}
