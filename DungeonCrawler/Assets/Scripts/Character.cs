using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public float Speed;
    public float JumpForce;
    protected bool isAlive;
    protected bool isOnFloor = false;
    protected Rigidbody2D rigidBody;
    private int ConnectionID;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void SpawnPlayer(int PlayerConnectionID)
    {
        ConnectionID = PlayerConnectionID;
        isAlive = true;
    }

    public int GetPlayerConnectionID()
    {
        return ConnectionID;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Floor")
        {
            isOnFloor = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Floor")
        {
            isOnFloor = false;
        }
    }
}
