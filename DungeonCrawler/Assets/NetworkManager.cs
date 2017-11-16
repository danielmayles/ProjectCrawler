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
public struct NetworkPacket<T>
{
    public NetworkPacketHeader MessageType;
    public T packetContents;
}

public class NetworkManager : MonoBehaviour
{
    public int MaxConnections = 100;
    public string IpAddress = "localhost";
    public int socketPort = 8080;
    public int connectionSocketPort = 8080;
    public bool AutoDiscoverServer = false;
    public int packetSize = 1024;

    private int myReliableChannelId;
    int socketId;
    int connectionId;

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
        myReliableChannelId = config.AddChannel(QosType.ReliableSequenced);
        HostTopology topology = new HostTopology(config, MaxConnections);
        socketId = NetworkTransport.AddHost(topology, socketPort);
    }

    void Connect()
    {
        byte error;
        connectionId = NetworkTransport.Connect(socketId, IpAddress, connectionSocketPort, 0, out error);
        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.Log("NetworkManager Could Not Connect");
        }
    }

    void SendPacket<T>(NetworkPacketHeader packetHeader, byte[] packetData)
    {
        byte error;
        byte[] buffer = new byte[packetSize];
        byte[] messageType = BitConverter.GetBytes((int)packetHeader);

        Array.Copy(messageType, buffer, sizeof(NetworkPacketHeader));
        Array.Copy(packetData, 0, buffer, sizeof(NetworkPacketHeader), packetData.Length);

        NetworkTransport.Send(socketId, connectionId, myReliableChannelId, buffer, packetSize, out error);
    }

    void SendPacket<T>(NetworkPacket<T> packet, int sizeOfPacketContents)
    {
        byte error;
        byte[] buffer = new byte[packetSize];
        Array.Copy(BitConverter.GetBytes((int)packet.MessageType), buffer, sizeof(NetworkPacketHeader));

        Stream stream = new MemoryStream(buffer, sizeof(NetworkPacketHeader), packetSize - sizeof(NetworkPacketHeader));
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, packet.packetContents);

        NetworkTransport.Send(socketId, connectionId, myReliableChannelId, buffer, packetSize, out error);
    }

    void Update()
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
                NetworkPacket<String> testpacket = new NetworkPacket<String>();
                testpacket.MessageType = NetworkPacketHeader.LocationChosen;
                testpacket.packetContents = "Test";
                SendPacket<String>(testpacket, testpacket.packetContents.Length);
                Debug.Log("Connect Event Received");
                break;
            case NetworkEventType.DataEvent:
                ReadPacket(recBuffer);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("remote client event disconnected");
                break;
        }
    }

    void ReadPacket(byte[] packetData)
    {
        NetworkPacketHeader packetHeader = (NetworkPacketHeader)BitConverter.ToInt32(packetData, 0);
        switch (packetHeader)
        {
            case NetworkPacketHeader.HoloLensData:
                break;

            case NetworkPacketHeader.LocationChosen:
                Stream stream = new MemoryStream(packetData, sizeof(NetworkPacketHeader), packetSize - sizeof(NetworkPacketHeader));
                BinaryFormatter formatter = new BinaryFormatter();
                string test = (string)formatter.Deserialize(stream);
                Debug.Log(test);
                break;
        }
    }
