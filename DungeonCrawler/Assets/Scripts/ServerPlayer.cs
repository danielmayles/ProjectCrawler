using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlayer : Player
{
    public override void ReceiveInputs(byte[] PlayerInputData, int AmountOfInputs, int InputID, float DeltaTime)
    {
        InputType[] PlayerInputs = new InputType[AmountOfInputs];

        int CurrentByteIndex = 16;
        for (int i = 0; i < AmountOfInputs; i++)
        {
            PlayerInputs[i] = (InputType)BitConverter.ToInt32(PlayerInputData, CurrentByteIndex);
            CurrentByteIndex += 4;

            switch (PlayerInputs[i])
            {
                case InputType.Left:
                    {
                        CurrentVelocity -= Speed * Vector3.right;
                    }
                    break;

                case InputType.Right:
                    {
                        CurrentVelocity += Speed * Vector3.right;
                    }
                    break;

                case InputType.Jump:
                    {
                        CurrentVelocity += JumpForce * Vector3.up;
                    }
                    break;

                case InputType.ArmDirection:
                    {
                        SetArmDirection(Serializer.DeserializeToVector3(PlayerInputData, CurrentByteIndex));
                        CurrentByteIndex += 12;
                    }
                    break;
            }
        }

        transform.position += CurrentVelocity * DeltaTime;
        NetworkPacketSender.SendUpdatePlayer(GetPlayerConnectionID(), InputID, transform.position, transform.forward, CurrentArmDirection, CurrentRoom);
        //NetworkPacketSender.SendPlayerArmDirection(GetPlayerConnectionID(), InputID, CurrentArmDirection, CurrentRoom);
        //NetworkPacketSender.SendPlayerPosition(GetPlayerConnectionID(), InputID, transform.position, CurrentRoom);
        CurrentVelocity = Vector3.zero;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Room")
        {
            Room newRoom = other.GetComponent<Room>();
            if (CurrentRoom != newRoom)
            {
                CurrentRoom.PlayerLeaveRoom(GetPlayerConnectionID());
                newRoom.PlayerJoinRoom(GetPlayerConnectionID(), transform.position);
            }
        }
    }
}
