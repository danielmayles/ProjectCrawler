using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkServer : MonoBehaviour
{
    public static NetworkServer Instance;
    public int socketPort = 8080;
    public int MaxConnections = 100;

    private List<Connection> Connections = new List<Connection>();
    private int AmountOfActiveConnections;
    private  int ReceivePacketSize = 2048;
    private int ReliableSequencedChannelId;
    private int ReliableChannelId;
    private int UnreliableChannelId;
    private int socketId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

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

    public void SendPacketToClientUnreliable(NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, packet.IntendedRecipientConnectionID, UnreliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }


    public void SendPacketToClientReliable(NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, packet.IntendedRecipientConnectionID, ReliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToClientReliableSequenced(NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, packet.IntendedRecipientConnectionID, ReliableSequencedChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void RelayPacketToClient(NetworkPacket Packet, int ChannelID)
    {
        if(ChannelID == ReliableChannelId)
        {
            SendPacketToClientReliable(Packet);
        }
        else if (ChannelID == ReliableSequencedChannelId)
        {
            SendPacketToClientReliableSequenced(Packet);
        }
        else
        {
            SendPacketToClientUnreliable(Packet);
        }
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
                    NetworkPacket RecPacket = new NetworkPacket();
                    RecPacket.IntendedRecipientConnectionID = BitConverter.ToInt32(recBuffer, 0);                    
                    RecPacket.MessageType = (NetworkPacketHeader)BitConverter.ToInt32(recBuffer, 4);
                    RecPacket.DataSize = BitConverter.ToInt32(recBuffer, 8);
                    Array.Copy(recBuffer, 12, RecPacket.Data, 0, RecPacket.DataSize);
                    NetworkPacketReader.Instance.ReadPacket(RecPacket);
                    RelayPacketToClient(RecPacket, recChannelId);
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
