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
    public NetworkPacketHeader MessageType;
    public byte[] Data;
    public int DataSize;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    public int MaxConnections = 100;
    public string IpAddress = "localhost";
    public int socketPort = 8080;
    public int connectionSocketPort = 8080;
    public int packetSize = 1024;

    private int ReliableSequencedChannelId;
    private int ReliableChannelId;
    private int UnreliableChannelId;
    int socketId;
    private bool NetworkActive = false;
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

    public void SendPacket(int ConnectionIndex, NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        byte[] buffer = new byte[packetSize];
        byte[] messageType = BitConverter.GetBytes((int)packetHeader);

        Array.Copy(messageType, buffer, sizeof(NetworkPacketHeader));
        Array.Copy(packetData, 0, buffer, sizeof(NetworkPacketHeader), packetData.Length);

        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableSequencedChannelId, buffer, packetSize, out error);
    }

    public void SendPacket(int ConnectionIndex, NetworkPacket packet, int sizeOfPacketContents)
    {
        byte error;
        List<byte> NewPacket = new List<byte>();
        NewPacket.AddRange(BitConverter.GetBytes((int)packet.MessageType));
        NewPacket.AddRange(BitConverter.GetBytes(packet.DataSize));
        NewPacket.AddRange(packet.Data);
        NetworkTransport.Send(socketId, Connections[ConnectionIndex].ConnectionID, ReliableSequencedChannelId, NewPacket.ToArray(), packetSize, out error);
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

    void RemoveConnection(int ConnectionIndex)
    {
        Connections[ConnectionIndex].isActive = false;
    }

    IEnumerator NetworkUpdate()
    { 
        while (NetworkActive)
        {
            int recHostId;
            int recConnectionId;
            int recChannelId;
            byte[] recBuffer = new byte[packetSize];
            int dataSize;
            byte error;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, packetSize, out dataSize, out error);
            switch (recNetworkEvent)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log("Connect Event Received");
                    break;
                case NetworkEventType.DataEvent:
                    NetworkPacketReader.Instance.ReadPacket(recBuffer);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("remote client event disconnected");

                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}