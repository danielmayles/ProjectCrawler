using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public Room CurrentRoom;
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

    public float MaxHeadUprightForce;
    public float CurrentHeadUprightForce;
    private int ConnectionID;
    protected bool isRagdolling;
   
    protected void Start()
    {
        CurrentHeadUprightForce = MaxHeadUprightForce;
        StoreInitalRotations();
        StoreInitalPositions();
        StartCoroutine(UpdatePlayerPhysics());
    }

    public void InitPlayer(int ConnectionID)
    {
        this.ConnectionID = ConnectionID;
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
        //Resets root to make Sure Root Doesn't drift from the ragdoll
        Vector3 currentRagdollPos = HipRigidBody.transform.position;
        transform.position = currentRagdollPos;        
        HipRigidBody.transform.position -= HipRigidBody.transform.position - currentRagdollPos;
        
        StartCoroutine(GetPlayerUp());
    }

    public IEnumerator GetPlayerUp()
    {
        Quaternion CurrentHipBoneRotation = HipRigidBody.transform.localRotation;
        Quaternion CurrentLeftLegBoneRotation = LeftLegRigidBody.transform.localRotation;
        Quaternion CurrentLeftUpLegBoneRotation = LeftUpLegRigidBody.transform.localRotation;
        Quaternion CurrentRightLegBoneRotation = RightLegRigidBody.transform.localRotation;
        Quaternion CurrentRightUpLegBoneRotation = RightUpLegRigidBody.transform.localRotation;

        Vector3 CurrentHipBonePosition = HipRigidBody.transform.localPosition;
        Vector3 CurrentLeftLegBonePosition = LeftLegRigidBody.transform.localPosition;
        Vector3 CurrentLeftUpLegBonePosition = LeftUpLegRigidBody.transform.localPosition;
        Vector3 CurrentRightLegBonePosition = RightLegRigidBody.transform.localPosition;
        Vector3 CurrentRightUpLegBonePosition = RightUpLegRigidBody.transform.localPosition;

        float Alpha = 0;
        float TimeToGetUp = 1.0f;
        float TimeElapsed = 0.0f;
        while(Alpha < 1)
        {
            TimeElapsed += Time.deltaTime;
            Alpha = TimeElapsed / TimeToGetUp;

            HipRigidBody.transform.localPosition = Vector3.Lerp(CurrentHipBonePosition, InitalHipBonePosition, Alpha);
            LeftLegRigidBody.transform.localPosition = Vector3.Lerp(CurrentLeftLegBonePosition, InitalLeftLegBonePosition, Alpha);
            LeftUpLegRigidBody.transform.localPosition = Vector3.Lerp(CurrentLeftUpLegBonePosition, InitalLeftUpLegBonePosition, Alpha);
            RightLegRigidBody.transform.localPosition = Vector3.Lerp(CurrentRightLegBonePosition, InitalRightLegBonePosition, Alpha);
            RightUpLegRigidBody.transform.localPosition = Vector3.Lerp(CurrentRightUpLegBonePosition, InitalRightUpLegBonePosition, Alpha);

            HipRigidBody.transform.localRotation = Quaternion.Lerp(CurrentHipBoneRotation, InitalHipBoneRotation, Alpha);
            LeftLegRigidBody.transform.localRotation = Quaternion.Lerp(CurrentLeftLegBoneRotation, InitalLeftLegBoneRotation, Alpha);
            LeftUpLegRigidBody.transform.localRotation = Quaternion.Lerp(CurrentLeftUpLegBoneRotation, InitalLeftUpLegBoneRotation, Alpha);
            RightLegRigidBody.transform.localRotation = Quaternion.Lerp(CurrentRightLegBoneRotation, InitalRightLegBoneRotation, Alpha);
            RightUpLegRigidBody.transform.localRotation = Quaternion.Lerp(CurrentRightUpLegBoneRotation, InitalRightUpLegBoneRotation, Alpha);
            yield return new WaitForEndOfFrame();
        }

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
