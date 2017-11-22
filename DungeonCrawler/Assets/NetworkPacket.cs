﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPacket:ScriptableObject
{
    public int IntendedRecipientConnectionID;
    public NetworkPacketHeader PacketHeader;
    public byte[] Data = new byte[ClientNetworkManager.Instance.ReceivePacketSize];
    public int DataSize;
}