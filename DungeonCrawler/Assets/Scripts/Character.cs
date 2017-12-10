using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public GameObject CharacterModel;
    public float Speed;
    public float JumpForce;
    protected bool isAlive;
    protected bool isOnFloor = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Floor")
        {
            isOnFloor = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Floor")
        {
            isOnFloor = false;
        }
    }
}
