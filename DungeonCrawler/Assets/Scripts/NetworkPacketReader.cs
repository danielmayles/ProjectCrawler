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
                int LocalConnectionID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                NetworkDetails.SetLocalConnectionID(LocalConnectionID);
                NetworkPacketSender.SendPlayerReady(LocalConnectionID);
                break;

            case NetworkPacketHeader.RequestConnectionID:
                NetworkPacket ConnectionIDPacket = ScriptableObject.CreateInstance<NetworkPacket>();
                ConnectionIDPacket.SetPacketTarget(SenderConnectionID);
                ConnectionIDPacket.PacketHeader = NetworkPacketHeader.ConnectionID;
                ConnectionIDPacket.SetPacketData(BitConverter.GetBytes(SenderConnectionID), 0, sizeof(int));
                NetworkServer.Instance.SendPacketToClient(ConnectionIDPacket, QosType.Reliable);   
                break;

            case NetworkPacketHeader.PlayerReady:
                int ReadyPlayerConnectionID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                BattleRoyale_GameManager.Instance.PlayerReady(ReadyPlayerConnectionID);
                break;

            case NetworkPacketHeader.StartGame:


                break;

            case NetworkPacketHeader.SpawnRoom:
                byte[] data = Packet.GetPacketData();
                int RoomIndex = BitConverter.ToInt32(data, 0);
                int RoomPrefabIndex = BitConverter.ToInt32(data, 4);
                Vector3 RoomPosition = Serializer.DeserializeToVector3(data, 8);
                RoomManager.Instance.SetNewRoom(RoomIndex, RoomPrefabIndex, RoomPosition);
                break;

            case NetworkPacketHeader.SpawnPlayer:
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
                break;

            case NetworkPacketHeader.PlayerPosition:
                int PlayerID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                Vector3 position = Serializer.DeserializeToVector3(Packet.GetPacketData(), 4);
                PlayerManager.Instance.GetPlayer(PlayerID).SetPosition(position);
                break;
        }
    }
}
