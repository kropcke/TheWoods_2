using UnityEngine;
using System.Collections;
using Obi;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerConnectorScript2 : MonoBehaviour
{

    GameObject[] players;
    LineRenderer playerRenderer2;
    GameObject middleBranch;
    public GameObject dot;


    [Range(0.001f, 1f)]
    public float size;

    [Range(0.01f, 2f)]
    public float delta;
    List<GameObject> dots = new List<GameObject>();
    bool createdots = true;
    public int count = 10;
    // Use this for initialization
    void Start()
    {

            playerRenderer2 = GameObject.Find("PlayerRenderer2").GetComponent<LineRenderer>();

    }

    // Update is called once per frame
    void Update()
    {

            players = GameObject.FindGameObjectsWithTag("Player");
            middleBranch = GameObject.Find("NewMiddleBranch");


            if (players.Length == 2)
            {
            


                players[1] = players[1].transform.GetChild(1).gameObject;
                //playerRenderer1.enabled = true;
                playerRenderer2.enabled = false;


                playerRenderer2.SetPosition(0, middleBranch.transform.GetChild(5).transform.position);
                playerRenderer2.SetPosition(1, players[1].transform.position);

            if (createdots)
            {
                createDots(middleBranch.transform.GetChild(5).transform.position, players[1].transform.position);
                createdots = false;
            }
            updateDots(middleBranch.transform.GetChild(5).transform.position, players[1].transform.position);

            }
            else
            {
                playerRenderer2.enabled = false;
            }


    }



    //void createDots(Vector3 start, Vector3 end)
    //{
    //    foreach (var d in dots)
    //    {
    //        Destroy(d);
    //    }
    //    dots.Clear();

    //    Vector3 distance = end - start;
    //    Vector3 step = distance * delta;

    //    Vector3 direction = (end - start).normalized;
    //    Vector3 pointer = start;

    //    while ((end - start).magnitude > (pointer - start).magnitude)
    //    {
    //        var g = GetOneDot();
    //        g.transform.position = pointer;
    //        dots.Add(g);
    //        pointer += (direction * delta);
    //    }
    //}
    //GameObject GetOneDot()
    //{
    //    var temp = PhotonNetwork.Instantiate("Sphere", new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity);
    //    //var temp = Instantiate(dot);
    //    temp.transform.localScale = Vector3.one * size;
    //    temp.transform.parent = transform;
    //    return gameObject;
    //}
    void createDots(Vector3 start, Vector3 end)
    {
        Vector3 distance = end - start;
        Vector3 delta = distance / count;

        for (int i = 0; i < count; i++)
        {
            var g = GetOneDot();
            dots.Add(g);
            g.transform.position = start + i * delta;
        }
    }

    void updateDots(Vector3 start, Vector3 end)
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
        //throw new System.NotImplementedException();
    }

    
}
