using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputType
{
    Left,
    Right,
    Jump,
    ArmDirection,
}

public class ControllerablePlayer : Player
{
    List<byte> InputsToSendToServer = new List<byte>();
    private int CurrentInputID;
    private Vector3 LastArmDirection;
    private int CurrentAmountOfInputs;

    void FixedUpdate()
    {
        CurrentAmountOfInputs = 0;
        if (Input.GetKey(KeyCode.A))
        {
            CurrentAmountOfInputs++;
            InputsToSendToServer.AddRange(BitConverter.GetBytes((int)InputType.Left));
            CurrentVelocity -= Speed * Vector3.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            CurrentAmountOfInputs++;
            InputsToSendToServer.AddRange(BitConverter.GetBytes((int)(InputType.Right)));
            CurrentVelocity += Speed * Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CurrentAmountOfInputs++;
            InputsToSendToServer.AddRange(BitConverter.GetBytes((int)InputType.Jump));
            CurrentVelocity += JumpForce * Vector3.up;
        }

        UpdateArmDirection();
        if(LastArmDirection != CurrentArmDirection)
        {
            CurrentAmountOfInputs++;
            InputsToSendToServer.AddRange(BitConverter.GetBytes((int)(InputType.ArmDirection)));
            InputsToSendToServer.AddRange(Serializer.GetBytes(CurrentArmDirection));
            LastArmDirection = CurrentArmDirection;
        }

        if (CurrentAmountOfInputs > 0)
        {
            CurrentInputID++;
            transform.position += CurrentVelocity * Time.deltaTime;
            CurrentVelocity = Vector3.zero;
            NetworkPacketSender.SendPlayerInput(GetPlayerConnectionID(), InputsToSendToServer, CurrentAmountOfInputs, CurrentInputID, Time.deltaTime);
            InputsToSendToServer.Clear();
        }
    }

    public void UpdateArmDirection()
    {
        Vector3 MousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        MousePosWorld.z = transform.position.z;
        Vector3 Dir = (MousePosWorld - transform.position).normalized;
        CurrentArmDirection = transform.position + (Dir * 10);
        IKController.SetHandTargetPositions(CurrentArmDirection, CurrentArmDirection);
    }

    public override void Ragdoll()
    {
        base.Ragdoll();
        NetworkPacketSender.SendRagdollPlayer(GetPlayerConnectionID());
    }

    public override void PlayerUpdate(int InputID, Vector3 Pos, Vector3 Dir, Vector3 ArmDir)
    {
        if (CurrentInputID == InputID)
        {   
            base.PlayerUpdate(InputID, Pos, Dir, ArmDir);
        }
    }

    public override void SetPosition(Vector3 Position)
    {
       transform.position = Position;
    }

    public override void SetArmDirection(Vector3 ArmDirection)
    {
        base.SetArmDirection(ArmDirection);
    }

    public override void OnPlayerChangeRooms(Room newRoom)
    {
        base.OnPlayerChangeRooms(newRoom);
        CameraManager.Instance.ChangeCameraPosition(newRoom.transform.position);
    }
}
