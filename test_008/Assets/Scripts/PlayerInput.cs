using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    //Player starting position
    public Vector3 playerOnePosition = new Vector3(0, 0, 0);
    public Vector3 playerTwoPosition = new Vector3(0, 0, 0);

    //The CharacterControllers
    CharacterController playerOne;
    CharacterController playerTwo;

    public float speed = 10.0f;

    void Start()
    {
        playerOne = GameObject.FindGameObjectWithTag("Player 1").GetComponent<CharacterController>();
        playerTwo = GameObject.FindGameObjectWithTag("Player 2").GetComponent<CharacterController>();
    }

    void Update()
    {
        if (this.gameObject.tag == "Player 1")
        {
            if (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d"))
            {
                playerOnePosition = new Vector3(Input.GetAxis("Horizontal_P1"), 0, Input.GetAxis("Vertical_P1"));
                playerOnePosition = transform.TransformDirection(playerOnePosition);
                playerOnePosition *= speed;
            }

            if (Input.GetKeyUp("w") || Input.GetKeyUp("a") || Input.GetKeyUp("s") || Input.GetKeyUp("d"))
            {
                playerOnePosition = new Vector3(Input.GetAxis("Horizontal_P1"), 0, Input.GetAxis("Vertical_P1"));
                playerOnePosition = transform.TransformDirection(playerOnePosition);
                playerOnePosition *= 0;
            }
        }

        if (this.gameObject.tag == "Player 2")
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                playerTwoPosition = new Vector3(Input.GetAxis("Horizontal_P2"), 0, Input.GetAxis("Vertical_P2"));
                playerTwoPosition = transform.TransformDirection(playerTwoPosition);
                playerTwoPosition *= speed;
            }

            if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                playerTwoPosition = new Vector3(Input.GetAxis("Horizontal_P2"), 0, Input.GetAxis("Vertical_P2"));
                playerTwoPosition = transform.TransformDirection(playerTwoPosition);
                playerTwoPosition *= 0;
            }
        }

        playerOne.Move(playerOnePosition * Time.deltaTime);
        playerTwo.Move(playerTwoPosition * Time.deltaTime);
    }
}