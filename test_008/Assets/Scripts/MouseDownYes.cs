using UnityEngine;
using UnityEngine.Networking;
using HutongGames.PlayMaker;

public class MouseDownYes : NetworkBehaviour
{

    //public Camera cam;
    //public GameObject cam_server;
    //public GameObject cam_client;


    void Start()
    {

    }

	[ClientCallback]

    //working, but only by clicking on full screen
	void Update()
    {

        //if (isServer)
        //{
        //    cam_server.SetActive = false;
        //    return;
        //}

        //if (isServer)
        //{
        //    cam_server.enabled = false;
        //    return;
        //}
            
        if (Input.GetMouseButtonDown(0))

        {
            MouseDownTrue();
            RpcMouseDownYes();
            CmdMouseDownYes();
        }

        if (Input.GetMouseButtonUp(0))
        {
            MouseDownFalse();
            RpcMouseDownNo();
            CmdMouseDownNo();
        }
    }

    void MouseDownTrue()
    {
        Debug.Log("Pressed primary button.");

        FsmInt globalVar = FsmVariables.GlobalVariables.FindFsmInt("var_tr_1a");
        globalVar.Value = 1;
    }

    void MouseDownFalse()
    {
        Debug.Log("Pressed primary button.");

        FsmInt globalVar = FsmVariables.GlobalVariables.FindFsmInt("var_tr_1a");
        globalVar.Value = 0;
    }

    [ClientRpc]

    void RpcMouseDownYes()
    {
        Debug.Log("Pressed primary button.");

        FsmInt globalVar = FsmVariables.GlobalVariables.FindFsmInt("var_tr_1a");
        globalVar.Value = 1;
    }

    [ClientRpc]

    void RpcMouseDownNo()
    {
        Debug.Log("Pressed primary button.");

        FsmInt globalVar = FsmVariables.GlobalVariables.FindFsmInt("var_tr_1a");
        globalVar.Value = 0;
    }

    [Command]

    void CmdMouseDownYes()
    {
        Debug.Log("Pressed primary button.");

        FsmInt globalVar = FsmVariables.GlobalVariables.FindFsmInt("var_tr_1a");
        globalVar.Value = 1;
    }

    [Command]

    void CmdMouseDownNo()
    {
        Debug.Log("Pressed primary button.");

        FsmInt globalVar = FsmVariables.GlobalVariables.FindFsmInt("var_tr_1a");
        globalVar.Value = 0;
    }

}


//working, but only on one of the clients, not both
/*void OnMouseDown()
{
    MouseDownTrue();
    RpcMouseDownYes();
    CmdMouseDownYes();
}

void OnMouseUp()
{
    MouseDownFalse();
    RpcMouseDownNo();
    CmdMouseDownNo();
}*/