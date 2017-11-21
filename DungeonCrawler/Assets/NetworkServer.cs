using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkServer : MonoBehaviour
{
    public int socketPort = 8080;
    public int MaxConnections = 100;

    private List<Connection> Connections = new List<Connection>();
    private int AmountOfActiveConnections;
    private  int ReceivePacketSize = 2048;
    private int ReliableSequencedChannelId;
    private int ReliableChannelId;
    private int UnreliableChannelId;
    private int socketId;

    void InitNetwork()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        ReliableSequencedChannelId = config.AddChannel(QosType.ReliableSequenced);
        ReliableChannelId = config.AddChannel(QosType.Reliable);
        UnreliableChannelId = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, MaxConnections);
        socketId = NetworkTransport.AddHost(topology, socketPort);
    }

    int AddConnection(int ConnectionID)
    {
        AmountOfActiveConnections++;
        Connection NewConnection = new Connection(ConnectionID);
        for (int i = 0; i < Connections.Count; i++)
        {
            if (Connections[i].isActive == false)
            {
                Connections[i] = NewConnection;
                return i;
            }
        }

        int ConnectionIndex = Connections.Count;
        Connections.Add(NewConnection);
        return ConnectionIndex;
    }

    void RemoveConnection(int ConnectionID)
    {
        AmountOfActiveConnections--;
        for (int i = 0; i < Connections.Count; i++)
        {
            if (Connections[i].ConnectionID == ConnectionID)
            {
                Connections[i].isActive = false;
            }
        }
    }

    public int GetAmountOfConnections()
    {
        return Connections.Count;
    }

    public void SendPacketToClientUnreliable(int ConnectionIndex, NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packetHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packetData.Length));
        NewPacket.AddRange(packetData);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, UnreliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToClientUnreliable(int ConnectionIndex, NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, UnreliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToClientReliable(int ConnectionIndex, NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packetHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packetData.Length));
        NewPacket.AddRange(packetData);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToClientReliable(int ConnectionIndex, NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToClientReliableSequenced(int ConnectionIndex, NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packetHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packetData.Length));
        NewPacket.AddRange(packetData);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableSequencedChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToClientReliableSequenced(int ConnectionIndex, NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableSequencedChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    IEnumerator NetworkUpdate()
    {
        bool ServerActive = true;
        while (ServerActive)
        {
            int recHostId;
            int recConnectionId;
            int recChannelId;
            byte[] recBuffer = new byte[ReceivePacketSize];
            int dataSize;
            byte error;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, ReceivePacketSize, out dataSize, out error);
            switch (recNetworkEvent)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    AddConnection(recConnectionId);
                    Debug.Log("Connect Event Received");
                    break;
                case NetworkEventType.DataEvent:
                    


                    NetworkPacketHeader Header = (NetworkPacketHeader)BitConverter.ToInt32(recBuffer, 0);
                    int DataSize = BitConverter.ToInt32(recBuffer, 4);
                    byte[] data = new byte[DataSize];
                    Array.Copy(recBuffer, 8, data, 0, DataSize);
                    NetworkPacketReader.Instance.ReadPacket(Header, data);
                    break;
                case NetworkEventType.DisconnectEvent:
                    RemoveConnection(recHostId);
                    Debug.Log("remote client event disconnected");
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
