﻿using System.Collections;
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
                BattleRoyale_GameManager.Instance.PlayerReady();
                break;

            case NetworkPacketHeader.StartGame:


                break;

            case NetworkPacketHeader.SpawnRoom:
                byte[] data = Packet.GetPacketData();
                int RoomIndex = BitConverter.ToInt32(data, 0);
                int RoomPrefabIndex = BitConverter.ToInt32(data, 4);
                Vector3 RoomPosition = Serializer.DeserializeToVector3(data, 8);
                RoomManager.Instance.SpawnRoom(RoomIndex, RoomPrefabIndex, RoomPosition);
                break;

            case NetworkPacketHeader.SpawnPlayer:
                int PlayerConnectionID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                if (PlayerConnectionID == NetworkDetails.GetLocalConnectionID())
                {
                    PlayerManager.Instance.SpawnControllerablePlayer(PlayerConnectionID);
                }
                else
                {
                    PlayerManager.Instance.SpawnPlayer(PlayerConnectionID);
                }
                break;
        }
    }
}