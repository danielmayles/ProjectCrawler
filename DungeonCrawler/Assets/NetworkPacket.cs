using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPacket:ScriptableObject
{
    public int IntendedRecipientConnectionID;
    public NetworkPacketHeader MessageType;
    public byte[] Data;
    public int DataSize;
}