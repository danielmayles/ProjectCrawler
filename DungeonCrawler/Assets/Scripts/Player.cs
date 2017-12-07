using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public Room CurrentRoom;
    public Rigidbody HeadRigidBody;
    public Rigidbody HipRigidBody;
    public Rigidbody SpineRigidBody;

    public Rigidbody LeftArmRigidBody;
    public Rigidbody LeftForearmRigidBody;
    public Rigidbody LeftLegRigidBody;
    public Rigidbody LeftUpLegRigidBody;

    public Rigidbody RightArmRigidBody;
    public Rigidbody RightForearmRigidBody;
    public Rigidbody RightLegRigidBody;
    public Rigidbody RightUpLegRigidBody;

    public float MaxHeadUprightForce;
    public float CurrentHeadUprightForce;
    private int ConnectionID;
    protected bool isRagdolling;

    protected void Start()
    {
        CurrentHeadUprightForce = MaxHeadUprightForce;
        StartCoroutine(UpdatePlayerPhysics());
    }

    public void InitPlayer(int ConnectionID)
    {
        this.ConnectionID = ConnectionID;
    }

    public void SetAlive()
    {
        isAlive = true;
    }

    public void Kill()
    {
        isAlive = false;
    }
     
    public int GetPlayerConnectionID()
    {
        return ConnectionID;
    }

    public void SetPosition(Vector3 Position)
    {
        transform.position = Position;
    }

    public void Ragdoll()
    {
        isRagdolling = true;
        HipRigidBody.isKinematic = false;
        LeftLegRigidBody.isKinematic = false;
        LeftUpLegRigidBody.isKinematic = false;
        RightLegRigidBody.isKinematic = false;
        RightUpLegRigidBody.isKinematic = false;
        CurrentHeadUprightForce = 0;
    }

    public void StopRagdoll()
    {
        CurrentHeadUprightForce = MaxHeadUprightForce;
        isRagdolling = false;
        HipRigidBody.isKinematic = true;
        LeftLegRigidBody.isKinematic = true;
        LeftUpLegRigidBody.isKinematic = true;
        RightLegRigidBody.isKinematic = true;
        RightUpLegRigidBody.isKinematic = true;       
    }

    public IEnumerator UpdatePlayerPhysics()
    {
        isAlive = true;
        while(isAlive)
        {
            HeadRigidBody.AddForce(Vector3.up * CurrentHeadUprightForce, ForceMode.Force);
            yield return new WaitForEndOfFrame();
        }
    }
}
