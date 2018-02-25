using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public Room CurrentRoom;
    public Rigidbody PlayerRigidBody;

    public Rigidbody HeadRigidBody;
    protected Quaternion InitalHeadBoneRotation;
    protected Vector3 InitalHeadBonePosition;

    public Rigidbody HipRigidBody;
    protected Quaternion InitalHipBoneRotation;
    protected Vector3 InitalHipBonePosition;

    public Rigidbody SpineRigidBody;
    protected Quaternion InitalSpineBoneRotation;
    protected Vector3 InitalSpineBonePosition;

    public Rigidbody LeftArmRigidBody;
    protected Quaternion InitalLeftArmBoneRotation;
    protected Vector3 InitalLeftArmBonePosition;

    public Rigidbody LeftForearmRigidBody;
    protected Quaternion InitalLeftForearmBoneRotation;
    protected Vector3 InitalLeftForearmBonePosition;

    public Rigidbody LeftLegRigidBody;
    protected Quaternion InitalLeftLegBoneRotation;
    protected Vector3 InitalLeftLegBonePosition;

    public Rigidbody LeftUpLegRigidBody;
    protected Quaternion InitalLeftUpLegBoneRotation;
    protected Vector3 InitalLeftUpLegBonePosition;

    public Rigidbody RightArmRigidBody;
    protected Quaternion InitalRightArmBoneRotation;
    protected Vector3 InitalRightArmBonePosition;

    public Rigidbody RightForearmRigidBody;
    protected Quaternion InitalRightForearmBoneRotation;
    protected Vector3 InitalRightForearmBonePosition;

    public Rigidbody RightLegRigidBody;
    protected Quaternion InitalRightLegBoneRotation;
    protected Vector3 InitalRightLegBonePosition;

    public Rigidbody RightUpLegRigidBody;
    protected Quaternion InitalRightUpLegBoneRotation;
    protected Vector3 InitalRightUpLegBonePosition;

    protected Vector3 CurrentVelocity;

    public float MaxHeadUprightForce;
    public float CurrentHeadUprightForce;
    private int ConnectionID;
    protected bool isRagdolling;

    protected Vector3 OldPos;
    protected Vector3 OldRot;
    
    protected void Start()
    {
        CurrentHeadUprightForce = MaxHeadUprightForce;
        StoreInitalRotations();
        StoreInitalPositions();
    }

    public void InitPlayer(int ConnectionID)
    {
        this.ConnectionID = ConnectionID;
        gameObject.SetActive(false);
    }

    private void StoreInitalRotations()
    {
        InitalHeadBoneRotation = HeadRigidBody.transform.localRotation;
        InitalHipBoneRotation = HipRigidBody.transform.localRotation;
        InitalSpineBoneRotation = SpineRigidBody.transform.localRotation;
        InitalLeftArmBoneRotation = LeftArmRigidBody.transform.localRotation;
        InitalLeftForearmBoneRotation = LeftForearmRigidBody.transform.localRotation;
        InitalLeftLegBoneRotation = LeftLegRigidBody.transform.localRotation;
        InitalLeftUpLegBoneRotation = LeftUpLegRigidBody.transform.localRotation;
        InitalRightArmBoneRotation = RightArmRigidBody.transform.localRotation;
        InitalRightForearmBoneRotation = RightForearmRigidBody.transform.localRotation;
        InitalRightLegBoneRotation = RightLegRigidBody.transform.localRotation;
        InitalRightUpLegBoneRotation = RightUpLegRigidBody.transform.localRotation;
    }

    private void StoreInitalPositions()
    {
        InitalHeadBonePosition = HeadRigidBody.transform.localPosition;
        InitalHipBonePosition = HipRigidBody.transform.localPosition;
        InitalSpineBonePosition = SpineRigidBody.transform.localPosition;
        InitalLeftArmBonePosition = LeftArmRigidBody.transform.localPosition;
        InitalLeftForearmBonePosition = LeftForearmRigidBody.transform.localPosition;
        InitalLeftLegBonePosition = LeftLegRigidBody.transform.localPosition;
        InitalLeftUpLegBonePosition = LeftUpLegRigidBody.transform.localPosition;
        InitalRightArmBonePosition = RightArmRigidBody.transform.localPosition;
        InitalRightForearmBonePosition = RightForearmRigidBody.transform.localPosition;
        InitalRightLegBonePosition = RightLegRigidBody.transform.localPosition;
        InitalRightUpLegBonePosition = RightUpLegRigidBody.transform.localPosition;
    }

    public void SetIsVisible(bool IsVisible)
    {
        gameObject.SetActive(IsVisible);
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

    public virtual void SetPosition(Vector3 Position, int InputID)
    {
        transform.position = Position;
    }

    public void SetTransform(Vector3 Position, Vector3 Rotation)
    {
        transform.position = Position;
        Debug.Log("SERVER EULER Angles" + Rotation);
        transform.eulerAngles = Rotation;
    }

    public virtual void Ragdoll()
    {
        isRagdolling = true;
        HipRigidBody.isKinematic = false;
        LeftLegRigidBody.isKinematic = false;
        LeftUpLegRigidBody.isKinematic = false;
        RightLegRigidBody.isKinematic = false;
        RightUpLegRigidBody.isKinematic = false;
        CurrentHeadUprightForce = 0;
    }

    public virtual void OnPlayerChangeRooms(Room newRoom)
    {
        CurrentRoom = newRoom;
    }

    public virtual void UpdatePlayer(InputType[] PlayerInputs, int InputID, float DeltaTime)
    {

    }
}
