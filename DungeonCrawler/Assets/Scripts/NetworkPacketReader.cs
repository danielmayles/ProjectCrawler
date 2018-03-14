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
                    PlayerManager.Instance.SpawnPlayer(PlayerConnectionID);         
                }
                break;

            case NetworkPacketHeader.PlayerPosition:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int InputID = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    Vector3 position = Serializer.DeserializeToVector3(Packet.GetPacketData(), 8);
                    PlayerManager.Instance.GetPlayer(PlayerID).SetPosition(position);
                }
                break;

            case NetworkPacketHeader.ArmDirection:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int InputID = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    Vector3 ArmDirection = Serializer.DeserializeToVector3(Packet.GetPacketData(), 8);
                    PlayerManager.Instance.GetPlayer(PlayerID).SetArmDirection(ArmDirection);
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

            case NetworkPacketHeader.PlayerUpdate:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int InputID = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    Vector3 position = Serializer.DeserializeToVector3(Packet.GetPacketData(), 8);
                    Vector3 ForwardVector = Serializer.DeserializeToVector3(Packet.GetPacketData(), 20);
                    Vector3 CurrentArmDir = Serializer.DeserializeToVector3(Packet.GetPacketData(), 32);
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
            
                    PlayerManager.Instance.GetPlayer(PlayerID).ReceiveInputs(Packet.GetPacketData(), AmountOfInputs, InputID, DeltaTime);
                }
                break;

            case NetworkPacketHeader.AddPlayerToRoom:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int RoomIndex = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    Vector3 PlayerPosition = Serializer.DeserializeToVector3(Packet.GetPacketData(), 8);
                    LevelManager.Instance.GetRoom(RoomIndex).PlayerJoinRoom(PlayerID, PlayerPosition);
                }
                break;

            case NetworkPacketHeader.RemovePlayerFromRoom:
                {
                    int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    int RoomIndex = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    LevelManager.Instance.GetRoom(RoomIndex).PlayerLeaveRoom(PlayerID);
                }
                break;

            case NetworkPacketHeader.SpawnPlayerInRoom:
                {
                    //int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    //int RoomIndex = BitConverter.ToInt32(Packet.GetPacketData(), 4);
                    //LevelManager.Instance.GetRoom(RoomIndex).PlayerJoinRoom(PlayerID);
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
                    int RoomIndex = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                    LevelManager.Instance.GetRoom(RoomIndex).ReadInRoomAsBytes(Packet.GetPacketData());
                }
                break;

            case NetworkPacketHeader.RequestCurrentPlayers:
                {
                    NetworkPacketSender.SendPlayerData(SenderConnectionID);
                }
                break;

            case NetworkPacketHeader.PlayerData:
                {
                    PlayerManager.Instance.ReadInPlayersAsBytes(Packet.GetPacketData());
                }
                break;
        }
    }
}
