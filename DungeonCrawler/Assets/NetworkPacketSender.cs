using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPacketSender : MonoBehaviour
{
    public void RelayPacketToAllPlayers(NetworkPacket Packet, QosType QualityOfServiceType)
    {
        int AmountOfConnections = NetworkManager.Instance.GetAmountOfConnections();
        switch (QualityOfServiceType)
        {
            case QosType.Unreliable:
                for(int i = 0; i < AmountOfConnections; i++)
                {
                    NetworkManager.Instance.SendPacketUnreliable(i, Packet);
                }
                break;

            case QosType.Reliable:
                for (int i = 0; i < AmountOfConnections; i++)
                {
                    NetworkManager.Instance.SendPacketReliable(i, Packet);
                }
                break;

            case QosType.ReliableSequenced:
                for (int i = 0; i < AmountOfConnections; i++)
                {
                    NetworkManager.Instance.SendPacketReliableSequenced(i, Packet);
                }
                break;
        }
    }

    public void RelayPacketToNearByPlayers(NetworkPacket Packet, Room RoomSentFrom, QosType QualityOfServiceType)
    {
        Character[] Players = RoomSentFrom.GetPlayersAndNearbyPlayers();

        switch (QualityOfServiceType)
        {
            case QosType.Unreliable:
                for (int i = 0; i < Players.Length; i++)
                {
                    NetworkManager.Instance.SendPacketUnreliable(Players[i].GetPlayerConnectionID(), Packet);
                }
                break;

            case QosType.Reliable:
                for (int i = 0; i < Players.Length; i++)
                {
                    NetworkManager.Instance.SendPacketReliable(Players[i].GetPlayerConnectionID(), Packet);
                }
                break;

            case QosType.ReliableSequenced:
                for (int i = 0; i < Players.Length; i++)
                {
                    NetworkManager.Instance.SendPacketReliableSequenced(Players[i].GetPlayerConnectionID(), Packet);
                }
                break;
        }
    }
}
