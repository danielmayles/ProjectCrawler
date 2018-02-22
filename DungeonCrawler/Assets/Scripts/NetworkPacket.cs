using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPacket : ScriptableObject
{
    private int PacketTargetID;
    private bool IsTargetRoom = false;
    public NetworkPacketHeader PacketHeader;
    private byte[] Data;

    public int GetDataSize()
    {
        return Data.Length;
    }

    public void SetPacketData(byte[] SourceData, int startIndex, int length)
    {
        Data = new byte[length];
        Array.Copy(SourceData, startIndex, Data, 0, length);
    }

    public byte[] GetPacketData()
    {
        return Data;
    }

    public byte[] GetBytes()
    {
        List<byte> PacketBytes = new List<byte>();
        PacketBytes.AddRange(BitConverter.GetBytes(PacketTargetID));
        PacketBytes.AddRange(BitConverter.GetBytes((int)PacketHeader));
        PacketBytes.AddRange(BitConverter.GetBytes(IsTargetRoom));
        if (Data != null && Data.Length != 0)
        {
            PacketBytes.AddRange(BitConverter.GetBytes(Data.Length));
            PacketBytes.AddRange(Data);
        }
        else
        {
            PacketBytes.AddRange(BitConverter.GetBytes(0));
        }
        return PacketBytes.ToArray();
    }

    public int GetTotalPacketSize()
    {
        int sizeOfPacket = sizeof(int);
        sizeOfPacket += sizeof(NetworkPacketHeader);
        sizeOfPacket += sizeof(int);
        if (Data != null && Data.Length != 0)
        {
            sizeOfPacket += Data.Length;
        }
        return sizeOfPacket;
    }

    public void SetPacketTarget(PacketTargets Target)
    {
        PacketTargetID = (int)Target;
    }

    public void SetPacketTarget(int TargetID)
    {
        PacketTargetID = TargetID;
    }

    public void SetIsTargetRoom(bool isTargetRoom)
    {
        IsTargetRoom = isTargetRoom;
    }

    public bool GetIsTargetRoom()
    {
        return IsTargetRoom;
    }

    public int GetPacketTarget()
    {
        return PacketTargetID;
    }
}