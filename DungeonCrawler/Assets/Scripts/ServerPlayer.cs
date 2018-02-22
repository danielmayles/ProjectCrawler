using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlayer : Player
{
    public override void UpdatePlayer(InputType[] PlayerInputs, int InputID, float DeltaTime)
    {
        for (int i = 0; i < PlayerInputs.Length; i++)
        {
            switch (PlayerInputs[i])
            {
                case InputType.Left:
                    CurrentVelocity -=  Speed * Vector3.right; 
                    break;

                case InputType.Right:
                    CurrentVelocity += Speed * Vector3.right;
                    break;

                case InputType.Jump:
                    CurrentVelocity += JumpForce * Vector3.up;
                    break;
            }
        }
        transform.position += CurrentVelocity * DeltaTime;
        NetworkPacketSender.SendPlayerPosition(GetPlayerConnectionID(), InputID, transform.position, CurrentRoom);
        CurrentVelocity = Vector3.zero;
    }

    public void OnTriggerEnter(Collider other)
    {
        /*
        if(other.tag == "Room")
        {
            Room newRoom = other.GetComponent<Room>();
            if (CurrentRoom != newRoom)
            {
                newRoom.PlayerJoinRoom(GetPlayerConnectionID());
                NetworkPacketSender.AddPlayerToRoom(GetPlayerConnectionID(), newRoom);
            }
        }
        */
    }
}
