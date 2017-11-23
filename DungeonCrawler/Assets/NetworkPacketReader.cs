using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public void ReadPacket(NetworkPacket Packet)
    {
        switch (Packet.PacketHeader)
        {
            case NetworkPacketHeader.InitPlayer:
                int PlayersConnectionID = BitConverter.ToInt32(Packet.Data, 0);
                break;
        }
    }
}
