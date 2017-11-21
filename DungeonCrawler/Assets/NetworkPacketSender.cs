using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPacketSender : MonoBehaviour
{
    public void RelayPacketToAllPlayers(NetworkPacket Packet, QosType QualityOfServiceType)
    {
        int AmountOfConnections = PlayerManager.Instance.GetAmountOfPlayers();
        switch (QualityOfServiceType)
        {
            case QosType.Unreliable:
                for(int i = 0; i < AmountOfConnections; i++)
                {
                    Packet.IntendedRecipientConnectionID = PlayerManager.Instance.GetPlayer(i).GetPlayerConnectionID();
                    ClientNetworkManager.Instance.SendPacketToServerUnreliable(Packet);
                }
                break;

            case QosType.Reliable:
                for (int i = 0; i < AmountOfConnections; i++)
                {
                    Packet.IntendedRecipientConnectionID = PlayerManager.Instance.GetPlayer(i).GetPlayerConnectionID();
                    ClientNetworkManager.Instance.SendPacketToServerReliable(Packet);
                }
                break;

            case QosType.ReliableSequenced:
                for (int i = 0; i < AmountOfConnections; i++)
                {
                    Packet.IntendedRecipientConnectionID = PlayerManager.Instance.GetPlayer(i).GetPlayerConnectionID();
                    ClientNetworkManager.Instance.SendPacketToServerReliableSequenced(Packet);
                }
                break;
        }
    }

    public void RelayPacketToNearbyPlayers(NetworkPacket Packet, Room RoomSentFrom, QosType QualityOfServiceType)
    {
        Character[] Players = RoomSentFrom.GetPlayersAndNearbyPlayers();

        switch (QualityOfServiceType)
        {
            case QosType.Unreliable:
                for (int i = 0; i < Players.Length; i++)
                {
                    Packet.IntendedRecipientConnectionID = Players[i].GetPlayerConnectionID();
                    ClientNetworkManager.Instance.SendPacketToServerUnreliable(Packet);
                }
                break;

            case QosType.Reliable:
                for (int i = 0; i < Players.Length; i++)
                {
                    Packet.IntendedRecipientConnectionID = Players[i].GetPlayerConnectionID();
                    ClientNetworkManager.Instance.SendPacketToServerReliable(Packet);
                }
                break;

            case QosType.ReliableSequenced:
                for (int i = 0; i < Players.Length; i++)
                {
                    Packet.IntendedRecipientConnectionID = Players[i].GetPlayerConnectionID();
                    ClientNetworkManager.Instance.SendPacketToServerReliableSequenced(Packet);
                }
                break;
        }
    }
}
