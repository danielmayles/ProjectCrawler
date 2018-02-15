using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputType
{
    Left,
    Right,
    Jump
}

public class ControllerablePlayer : Player
{
    List<InputType> InputsToSendToServer = new List<InputType>();
    private int CurrentInputID;

    void FixedUpdate()
    { 
        bool InputKeyPressed = false;
        if (Input.GetKey(KeyCode.A))
        {
            InputKeyPressed = true;
            InputsToSendToServer.Add(InputType.Left);
            CurrentVelocity -= Speed * Vector3.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            InputKeyPressed = true;
            InputsToSendToServer.Add(InputType.Right);
            CurrentVelocity += Speed * Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InputKeyPressed = true;
            InputsToSendToServer.Add(InputType.Jump);
            CurrentVelocity += JumpForce * Vector3.up;
        }

        if (InputKeyPressed)
        {
            CurrentInputID++;
            transform.position += CurrentVelocity * Time.deltaTime;
            CurrentVelocity = Vector3.zero;
            NetworkPacketSender.SendPlayerInput(GetPlayerConnectionID(), InputsToSendToServer, CurrentInputID, Time.deltaTime);         
        }
    }

    public override void Ragdoll()
    {
        base.Ragdoll();
        NetworkPacketSender.SendRagdollPlayer(GetPlayerConnectionID());
    }

    public override void SetPosition(Vector3 Position, int InputID)
    {
        if(CurrentInputID == InputID && transform.position != Position)
        {
            transform.position = Position;
        }
    }
}
