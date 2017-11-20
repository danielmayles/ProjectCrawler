using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public enum NetworkPacketHeader
{
    PlayerData,
}

[Serializable]
public struct NetworkPacket
{
    public int IntendedRecipientConnectionID;
    public NetworkPacketHeader MessageType;
    public byte[] Data;
    public int DataSize;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    public bool IsServer;
    public int MaxConnections = 100;
    public string IpAddress = "localhost";
    public int socketPort = 8080;
    public int connectionSocketPort = 8080;
    public int ReceivePacketSize = 2048;

    private int ReliableSequencedChannelId;
    private int ReliableChannelId;
    private int UnreliableChannelId;
    int socketId;
    private bool NetworkActive = false;
    private int AmountOfActiveConnections;
    private List<Connection> Connections = new List<Connection>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        InitNetwork();
        Connect();
        StartNetworking();
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

    void Connect()
    {
        byte error;    
        int ConnectionID = NetworkTransport.Connect(socketId, IpAddress, connectionSocketPort, 0, out error);
        
        if((NetworkError)error == NetworkError.Ok)
        {
            AddConnection(ConnectionID);
        }
        else
        {
            Debug.Log("NetworkManager Could Not Connect");
        }
    }

    public void SendPacketUnreliable(int ConnectionIndex, NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packetHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packetData.Length));
        NewPacket.AddRange(packetData);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, UnreliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketUnreliable(int ConnectionIndex, NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, UnreliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketReliable(int ConnectionIndex, NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packetHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packetData.Length));
        NewPacket.AddRange(packetData);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketReliable(int ConnectionIndex, NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketReliableSequenced(int ConnectionIndex, NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packetHeader));
        NewPacket.AddRange(BitConverter.GetBytes(packetData.Length));
        NewPacket.AddRange(packetData);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableSequencedChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void SendPacketReliableSequenced(int ConnectionIndex, NetworkPacket packet)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableSequencedChannelId, NewPacket.ToArray(), NewPacket.Count, out error);
    }

    public void StartNetworking()
    {
        NetworkActive = true;
        StartCoroutine(NetworkUpdate());
    }

    public void StopNetworking()
    {
        NetworkActive = false;
    }

    int AddConnection(int ConnectionID)
    {
        AmountOfActiveConnections++;
        Connection NewConnection = new Connection(ConnectionID);
        for(int i = 0; i < Connections.Count; i++)
        {
            if(Connections[i].isActive == false)
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

    public int GetAmountOfActiveConnections()
    {
        return AmountOfActiveConnections;
    }

    IEnumerator NetworkUpdate()
    { 
        while (NetworkActive)
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
                    Debug.Log("Connect Event Received");
                    AddConnection(recConnectionId);
                    break;
                case NetworkEventType.DataEvent:
                    if(IsServer)
                    {
                       
                    }



                    NetworkPacketHeader Header = (NetworkPacketHeader)BitConverter.ToInt32(recBuffer, 0);
                    int DataSize = BitConverter.ToInt32(recBuffer, 4);
                    byte[] data = new byte[DataSize];
                    Array.Copy(recBuffer, 8, data, 0, DataSize);
                    NetworkPacketReader.Instance.ReadPacket(Header, data);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("remote client event disconnected");
                    RemoveConnection(recConnectionId);
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}