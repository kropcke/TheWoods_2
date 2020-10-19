using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerConnectorScript : MonoBehaviour
{
    GameObject[] players;
    GameObject middleBranch;
    public GameObject dot;
    List<GameObject> dots1 = new List<GameObject>();
    List<GameObject> dots2 = new List<GameObject>();

    [Range(0.0001f, 1f)]
    public float size;

    [Range(0.01f, 2f)]
    public float delta;

    bool createdots = true;
    public int count;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        middleBranch = GameObject.Find("NewMiddleBranch");
        if (players.Length == 2)
        {

            if (players[0].GetPhotonView().ViewID > players[1].GetPhotonView().ViewID)
            {
                connectPlayerToBranch(players[0].transform.GetChild(1).gameObject, players[1].transform.GetChild(1).gameObject);
            }
            else
            {
                connectPlayerToBranch(players[1].transform.GetChild(1).gameObject, players[0].transform.GetChild(1).gameObject);
            }

        }


    }
    void connectPlayerToBranch(GameObject gameObj1, GameObject gameObj2)
    {
        if (createdots)
        {
            dots1 = createDots(middleBranch.transform.GetChild(4).transform.position, gameObj1.transform.position);
            dots2 = createDots(middleBranch.transform.GetChild(5).transform.position, gameObj2.transform.position);
            createdots = false;
        }
        if (dots1 != null)
        {
            updateDots(middleBranch.transform.GetChild(4).transform.position, gameObj1.transform.position, dots1);
        }
        if (dots2 != null)
        {
            updateDots(middleBranch.transform.GetChild(5).transform.position, gameObj2.transform.position, dots2);
        }
    }

    List<GameObject> createDots(Vector3 start, Vector3 end)
    {

        Vector3 distance = end - start;
        Vector3 delta = distance / count;
        List<GameObject> dots = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var g = GetOneDot();
            g.transform.localScale = Vector3.one * size;
            g.transform.parent = transform;
            dots.Add(g);

        }

        return dots;
    }

    void updateDots(Vector3 start, Vector3 end, List<GameObject> dots)
    {
        Vector3 distance = end - start;
        Vector3 delta = distance / count;

        int i = 0;
        foreach (var d in dots)
        {
            d.transform.position = start + i * delta;
            i++;
        }
    }

    GameObject GetOneDot()
    {
        var gameObject1 = Instantiate(dot);
        gameObject1.transform.localScale = Vector3.one * size;
        gameObject1.transform.parent = transform;
        return gameObject1;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
