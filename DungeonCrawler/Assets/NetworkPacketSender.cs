using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPacketSender : MonoBehaviour
{
    public void SendPacketToAllPlayers(NetworkPacket Packet, QosType QualityOfServiceType)
    {
        int AmountOfConnections = PlayerManager.Instance.GetAmountOfPlayers();   
        for (int i = 0; i < AmountOfConnections; i++)
        {
            Packet.IntendedRecipientConnectionID = PlayerManager.Instance.GetPlayer(i).GetPlayerConnectionID();
            ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
        }
    }

    public void RelayPacketToNearbyPlayers(NetworkPacket Packet, Room RoomSentFrom, QosType QualityOfServiceType)
    {
        Character[] Players = RoomSentFrom.GetPlayersAndNearbyPlayers();
        for (int i = 0; i < Players.Length; i++)
        {
            Packet.IntendedRecipientConnectionID = Players[i].GetPlayerConnectionID();
            ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
        }
    }
}
