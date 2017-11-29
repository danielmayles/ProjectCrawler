using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public enum NetworkPacketHeader
{
    //QQQ ConnectionID to be sent seprate from spawning of players I think....
    ConnectionID,
    InitPlayer,
}

public class NetworkServer : MonoBehaviour
{
    public static NetworkServer Instance;
    public int ServerPort = 8080;
    public int MaxConnections = 100;

    private List<Connection> Connections = new List<Connection>();
    private int AmountOfActiveConnections;
    private int ReceivePacketSize = 2048;
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

    public void Start()
    {
        InitNetwork();
        StartCoroutine(NetworkUpdate());
    }

    void InitNetwork()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        ReliableSequencedChannelId = config.AddChannel(QosType.ReliableSequenced);
        ReliableChannelId = config.AddChannel(QosType.Reliable);
        UnreliableChannelId = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, MaxConnections);
        socketId = NetworkTransport.AddHost(topology, ServerPort);
    }

    int AddConnection(int ConnectionID)
    {
        NetworkPacket Packet = new NetworkPacket();
        Packet.PacketHeader = NetworkPacketHeader.InitPlayer;
        Packet.Data = BitConverter.GetBytes(ConnectionID);
        Packet.DataSize = sizeof(int);
        SendPacketToAllClients(Packet, QosType.Reliable);
        
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

    public void SendPacketToClient(NetworkPacket packet, int ChannelID)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.PacketHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, packet.IntendedRecipientConnectionID, ChannelID, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToClient(NetworkPacket packet, QosType ChannelType)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.PacketHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, packet.IntendedRecipientConnectionID, GetChannel(ChannelType), NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketToAllClients(NetworkPacket Packet, QosType QualityOfServiceType)
    {
        for(int i = 0; i < Connections.Count; i++)
        {
            Packet.IntendedRecipientConnectionID = Connections[i].ConnectionID;
            SendPacketToClient(Packet, QualityOfServiceType);
        }
    }

    public int GetChannel(QosType QOSChannel)
    {
        int ChannelID;
        switch (QOSChannel)
        {
            case QosType.Reliable:
                ChannelID = ReliableChannelId;
                break;

            case QosType.ReliableSequenced:
                ChannelID = ReliableSequencedChannelId;
                break;

            case QosType.Unreliable:
                ChannelID = UnreliableChannelId;
                break;
            default:
                Debug.Log("QOS TYPE " + QOSChannel + " has not been implemented defaulting to Unreliable");
                ChannelID = UnreliableChannelId;
                break;
        }
        return ChannelID;
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
                    RecPacket.PacketHeader = (NetworkPacketHeader)BitConverter.ToInt32(recBuffer, 4);
                    RecPacket.DataSize = BitConverter.ToInt32(recBuffer, 8);
                    Array.Copy(recBuffer, 12, RecPacket.Data, 0, RecPacket.DataSize);
                    NetworkPacketReader.Instance.ReadPacket(RecPacket);
                    SendPacketToClient(RecPacket, recChannelId);
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
