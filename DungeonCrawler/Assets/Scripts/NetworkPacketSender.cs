﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPacketSender : MonoBehaviour
{
    public static void SendPacketToAllPlayers(NetworkPacket Packet, QosType QualityOfServiceType)
    {
#if SERVER
         NetworkServer.Instance.SendPacketToAllClients(Packet, NetworkServer.Instance.GetChannel(QualityOfServiceType));
#else
        Packet.SetPacketTarget(PacketTargets.RelayToAllClients);
        ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
#endif
    }

    public static void RelayPacketToNearbyPlayers(NetworkPacket Packet, Room RoomSentFrom, QosType QualityOfServiceType)
    {
        Player[] Players = RoomSentFrom.GetPlayersAndNearbyPlayers();
        for (int i = 0; i < Players.Length; i++)
        {
            Packet.SetPacketTarget(Players[i].GetPlayerConnectionID());
#if SERVER
            NetworkServer.Instance.SendPacketToClient(Packet, QualityOfServiceType);
#else
            ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
#endif
        }
    }

    public static void SendToServerOnly(NetworkPacket Packet, QosType QualityOfServiceType)
    {
        Packet.SetPacketTarget(PacketTargets.ServerOnly);
#if SERVER
        NetworkPacketReader.ReadPacket(Packet, NetworkDetails.GetLocalConnectionID(), true);
#else
        ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
#endif
    }

    public static void SendPlayerReady(int PlayerConnectionID)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.PlayerReady;
        networkPacket.SetPacketData(BitConverter.GetBytes(PlayerConnectionID), 0, sizeof(int));
        SendToServerOnly(networkPacket, QosType.Reliable);
    }

    public static void SendSpawnPlayer(int PlayerConnectionID)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.SpawnPlayer;
        networkPacket.SetPacketData(BitConverter.GetBytes(PlayerConnectionID), 0, sizeof(int));
        SendPacketToAllPlayers(networkPacket, QosType.Reliable);
    }

    public static void SendSpawnRoom(int RoomIndex, int RoomPrefabIndex, Vector3 Position)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.SpawnRoom;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(RoomIndex));
        data.AddRange(BitConverter.GetBytes(RoomPrefabIndex));
        data.AddRange(Serializer.GetBytes(Position));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        SendPacketToAllPlayers(networkPacket, QosType.Reliable);
    }
}
