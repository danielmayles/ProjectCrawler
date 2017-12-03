using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public enum NetworkPacketHeader
{
    ConnectionID,
    RequestConnectionID,
    SpawnPlayer,
}

public enum PacketTargets
{
    ServerOnly = -1,
    RelayToAllClients = -2,
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
        AmountOfActiveConnections++;
        Connection NewConnection = ScriptableObject.CreateInstance<Connection>();
        NewConnection.ConnectionID = ConnectionID;
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

    public void SendPacketToClient(NetworkPacket packet, int QosChannelID)
    {
        byte error;
        NetworkTransport.Send(socketId, packet.GetPacketTarget(), QosChannelID, packet.GetBytes(), packet.GetTotalPacketSize(), out error);
    }

    public void SendPacketToClient(NetworkPacket packet, QosType QosChannel)
    {
        byte error;
        NetworkTransport.Send(socketId, packet.GetPacketTarget(), GetChannel(QosChannel), packet.GetBytes(), packet.GetTotalPacketSize(), out error);
    }

    public void RelayPacketToAllClients(NetworkPacket Packet, int SenderConnectionID, int QosChannelID)
    {
        for(int i = 0; i < Connections.Count; i++)
        {
            if (Connections[i].ConnectionID != SenderConnectionID)
            {
                Packet.SetPacketTarget(Connections[i].ConnectionID);
                SendPacketToClient(Packet, QosChannelID);
            }
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
                    break;
                case NetworkEventType.DataEvent:
                    NetworkPacket RecPacket = ScriptableObject.CreateInstance<NetworkPacket>();
                    RecPacket.SetPacketTarget(BitConverter.ToInt32(recBuffer, 0));                    
                    RecPacket.PacketHeader = (NetworkPacketHeader)BitConverter.ToInt32(recBuffer, 4);
                    RecPacket.SetPacketData(recBuffer, 12, BitConverter.ToInt32(recBuffer, 8));
                    NetworkPacketReader.Instance.ReadPacket(RecPacket, recConnectionId, true);
              
                    if (RecPacket.GetPacketTarget()!= (int)PacketTargets.ServerOnly)
                    {
                        if (RecPacket.GetPacketTarget() == (int)PacketTargets.RelayToAllClients)
                        {
                            RelayPacketToAllClients(RecPacket, recConnectionId, recChannelId);
                        }
                        else
                        {
                            SendPacketToClient(RecPacket, recChannelId);
                        }
                    }
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
