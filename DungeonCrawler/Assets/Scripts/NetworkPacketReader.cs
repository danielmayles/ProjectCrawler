using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class NetworkPacketReader : MonoBehaviour
{
    public static void ReadPacket(NetworkPacket Packet, int SenderConnectionID, bool isServerReading)
    {
        switch (Packet.PacketHeader)
        {
            case NetworkPacketHeader.ConnectionID:
                {
                    int LocalConnectionID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    NetworkDetails.SetLocalConnectionID(LocalConnectionID);
                    NetworkPacketSender.SendPlayerReady(LocalConnectionID);
                }
                break;

            case NetworkPacketHeader.RequestConnectionID:
                {
                    NetworkPacket ConnectionIDPacket = ScriptableObject.CreateInstance<NetworkPacket>();
                    ConnectionIDPacket.SetPacketTarget(SenderConnectionID);
                    ConnectionIDPacket.PacketHeader = NetworkPacketHeader.ConnectionID;
                    ConnectionIDPacket.SetPacketData(BitConverter.GetBytes(SenderConnectionID), 0, sizeof(int));
                    NetworkManager.Instance.SendPacketToClient(ConnectionIDPacket, QosType.Reliable);
                }
                break;

            case NetworkPacketHeader.PlayerReady:
                {
                    int ReadyPlayerConnectionID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    BattleRoyale_GameManager.Instance.PlayerReady(ReadyPlayerConnectionID);
                }
                break;

            case NetworkPacketHeader.StartGame:


                break;

            case NetworkPacketHeader.SpawnPlayer:
                {
                    int PlayerConnectionID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int SpawnRoomIndex = BitConverter.ToInt32(Packet.GetPacketData(), 4);

                    if (PlayerConnectionID == NetworkDetails.GetLocalConnectionID())
                    {
                        PlayerManager.Instance.SpawnControllerablePlayer(PlayerConnectionID, SpawnRoomIndex);
                    }
                    else
                    {
                        PlayerManager.Instance.SpawnPlayer(PlayerConnectionID, SpawnRoomIndex);
                    }
                }
                break;

            case NetworkPacketHeader.PlayerPosition:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int InputID = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    Vector3 position = Serializer.DeserializeToVector3(Packet.GetPacketData(), 8);
                    PlayerManager.Instance.GetPlayer(PlayerID).SetPosition(position, InputID);
                }
                break;

            case NetworkPacketHeader.PlayerTransform:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    Vector3 position = Serializer.DeserializeToVector3(Packet.GetPacketData(), 4);
                    Vector3 rotation = Serializer.DeserializeToVector3(Packet.GetPacketData(), 16);
                    PlayerManager.Instance.GetPlayer(PlayerID).SetTransform(position, rotation);
                }
                break;

            case NetworkPacketHeader.RagdollPlayer:
                {
                    int PlayerToRagdollID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    PlayerManager.Instance.GetPlayer(PlayerToRagdollID).Ragdoll();
                }
                break;

            case NetworkPacketHeader.PlayerInputUpdate:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    float DeltaTime = BitConverter.ToSingle(Packet.GetPacketData(), 4);
                    int InputID = BitConverter.ToInt32(Packet.GetPacketData(), 8);
                    int AmountOfInputs = BitConverter.ToInt32(Packet.GetPacketData(), 12);
                    InputType[] PlayerInputs = new InputType[AmountOfInputs];
                    for(int i = 0; i < AmountOfInputs; i++)
                    {
                        PlayerInputs[i] = (InputType)BitConverter.ToInt32(Packet.GetPacketData(), 16 + (4 * i));
                    }

                    PlayerManager.Instance.GetPlayer(PlayerID).UpdatePlayer(PlayerInputs, InputID, DeltaTime);
                }
                break;

            case NetworkPacketHeader.PlayerChangeRoom:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int RoomIndex = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    PlayerManager.Instance.GetPlayer(PlayerID).CurrentRoom.PlayerLeaveRoom(PlayerID);
                    LevelManager.Instance.GetRoom(RoomIndex).PlayerJoinRoom(PlayerID);
                }
                break;

            case NetworkPacketHeader.RequestLevel:
                {
                    NetworkPacketSender.SendLevelData(SenderConnectionID);
                }
                break;

            case NetworkPacketHeader.LevelData:
                {
                    LevelManager.Instance.ReadInLevelBytes(Packet.GetPacketData());
                }
                break;

            case NetworkPacketHeader.RequestRoomData:
                {
                    int RoomIndex = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    NetworkPacketSender.SendRoomData(SenderConnectionID, RoomIndex);
                }
                break;

            case NetworkPacketHeader.RoomData:
                {
                    LevelManager.Instance.ReadInRoomAsBytes(Packet.GetPacketData());
                }
                break;
        }
    }
}
