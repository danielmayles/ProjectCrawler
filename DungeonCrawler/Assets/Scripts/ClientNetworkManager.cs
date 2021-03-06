﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class ClientNetworkManager : MonoBehaviour
{
    public static ClientNetworkManager Instance;
    public int MaxConnections = 10;
    public string ServerIpAddress = "localhost";
    public int ServerPort = 8080;
    public int ReceivePacketSize = 2048;

    public int ReliableSequencedChannelId;
    public int ReliableChannelId;
    public int UnreliableChannelId;
    int socketId;

    private int ServerConnectionID = -1;
    private bool NetworkActive = false;
    
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
        ConnectToServer();
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
        socketId = NetworkTransport.AddHost(topology);
    }

    void ConnectToServer()
    {
        byte error;    
        int ConnectionID = NetworkTransport.Connect(socketId, ServerIpAddress, ServerPort, 0, out error);
        
        if((NetworkError)error == NetworkError.Ok)
        {
            ServerConnectionID = ConnectionID;
        }
        else
        {
            Debug.Log("NetworkManager Could Not Connect");
        }
    }

    public void SendPacketToServer(NetworkPacket packet, QosType ChannelType)
    {
        if (ServerConnectionID != -1)
        {
            byte error;
            NetworkTransport.Send(socketId, ServerConnectionID, GetChannel(ChannelType), packet.GetBytes(), packet.GetTotalPacketSize(), out error);
        }
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

   // public int GetAmountOfActiveConnections()
    //{
     //   return AmountOfActiveConnections;
    //}

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
                    Debug.Log("Connect Client Event Received");

                    //Requests own ConnectionID From Server
                    NetworkPacket packet = ScriptableObject.CreateInstance<NetworkPacket>();
                    packet.PacketHeader = NetworkPacketHeader.RequestConnectionID;
                    packet.SetPacketTarget(PacketTargets.ServerOnly);
                    SendPacketToServer(packet, QosType.Reliable);
                    break;
                case NetworkEventType.DataEvent:
                    NetworkPacket RecPacket = ScriptableObject.CreateInstance<NetworkPacket>();
                    RecPacket.SetPacketTarget(BitConverter.ToInt32(recBuffer, 0));
                    RecPacket.PacketHeader = (NetworkPacketHeader)BitConverter.ToInt32(recBuffer, 4);
                    RecPacket.SetIsTargetRoom(BitConverter.ToBoolean(recBuffer, 8));
                    RecPacket.SetPacketData(recBuffer, 13, BitConverter.ToInt32(recBuffer, 9));
                    NetworkPacketReader.ReadPacket(RecPacket, recConnectionId, false);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("remote client event disconnected");
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}