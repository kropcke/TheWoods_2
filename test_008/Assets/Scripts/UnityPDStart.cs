using UnityEngine;
using System.Collections;

public class UnityPDStart : MonoBehaviour
{
    public string patchName;
    private int _pdPatch = -1;
    // Use this for initialization
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("Initializing UnityPD");

        UnityPD.Init();

        yield return new WaitForSeconds(2f);

        _pdPatch = UnityPD.OpenPatch(patchName);
    }

    void OnApplicationQuit()
    {
        UnityPD.Deinit();
    }
}
