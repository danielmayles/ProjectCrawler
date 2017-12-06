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


    public Rigidbody MiddleSpine;
    public float HeadUprightForce;
    public float SpineUprightForce;

    private int ConnectionID;

    protected void Start()
    {
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
        Rigidbody[] RagdollRigidBodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < RagdollRigidBodies.Length; i++)
        {
            RagdollRigidBodies[i].isKinematic = false;
        }

        Collider[] RagdollColliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < RagdollColliders.Length; i++)
        {
            RagdollColliders[i].enabled = true;
        }
    }

    public void StopRagdoll()
    {
        Rigidbody[] RagdollRigidBodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < RagdollRigidBodies.Length; i++)
        {
            RagdollRigidBodies[i].isKinematic = true;
        }

        Collider[] RagdollColliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < RagdollColliders.Length; i++)
        {
            RagdollColliders[i].enabled = false;
        }
    }

    public void Awake()
    {
        DisableFullRagdoll();
    }


    void DisableFullRagdoll()
    {
       
    }

    public IEnumerator UpdatePlayerPhysics()
    {
        while(isAlive)
        {
            HeadRigidBody.AddForce(Vector3.up * HeadUprightForce, ForceMode.Force);
            yield return new WaitForEndOfFrame();
        }
    }
}
