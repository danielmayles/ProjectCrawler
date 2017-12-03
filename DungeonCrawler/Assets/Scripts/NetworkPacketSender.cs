using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPacketSender : MonoBehaviour
{
    public void SendPacketToAllPlayers(NetworkPacket Packet, QosType QualityOfServiceType)
    {
        Packet.SetPacketTarget(PacketTargets.RelayToAllClients);
        ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
    }

    public void RelayPacketToNearbyPlayers(NetworkPacket Packet, Room RoomSentFrom, QosType QualityOfServiceType)
    {
        Player[] Players = RoomSentFrom.GetPlayersAndNearbyPlayers();
        for (int i = 0; i < Players.Length; i++)
        {
            Packet.SetPacketTarget(Players[i].GetPlayerConnectionID());
            ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
        }
    }

    public void SpawnPlayer(int PlayerConnectionID)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.SpawnPlayer;
        networkPacket.SetPacketData(BitConverter.GetBytes(PlayerConnectionID), 0, sizeof(int));
        SendPacketToAllPlayers(networkPacket, QosType.Reliable);
    }
}
