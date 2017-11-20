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

    public void ReadPacket(NetworkPacketHeader Header, byte[] packetData)
    {
        NetworkPacketHeader packetHeader = (NetworkPacketHeader)BitConverter.ToInt32(packetData, 0);
        switch (packetHeader)
        {
            
        }
    }
}
