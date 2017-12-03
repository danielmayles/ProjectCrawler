using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class NetworkPacketReader : MonoBehaviour
{
    public static NetworkPacketReader Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void ReadPacket(NetworkPacket Packet, int SenderConnectionID, bool isServerReading)
    {
        switch (Packet.PacketHeader)
        {
            case NetworkPacketHeader.ConnectionID:
                int LocalConnectionID = BitConverter.ToInt32(Packet.GetPacketData(), 0);
                NetworkDetails.SetLocalConnectionID(LocalConnectionID);
                break;

            case NetworkPacketHeader.RequestConnectionID:
                NetworkPacket ConnectionIDPacket = ScriptableObject.CreateInstance<NetworkPacket>();
                ConnectionIDPacket.SetPacketTarget(SenderConnectionID);
                ConnectionIDPacket.PacketHeader = NetworkPacketHeader.ConnectionID;
                ConnectionIDPacket.SetPacketData(BitConverter.GetBytes(SenderConnectionID), 0, sizeof(int));
                NetworkServer.Instance.SendPacketToClient(ConnectionIDPacket, QosType.Reliable);   
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
