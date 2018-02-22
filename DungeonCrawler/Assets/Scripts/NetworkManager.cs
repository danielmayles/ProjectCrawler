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
    PlayerReady,
    StartGame,
    SpawnPlayer,
    SpawnRoom,
    PlayerPosition,
    PlayerTransform,
    RagdollPlayer,
    PlayerInputUpdate,
    PlayerPositionUpdate,
    AddPlayerToRoom,
    RemovePlayerFromRoom,
    RequestCurrentPlayers,
    PlayerData,
    SpawnPlayerInRoom,
    RequestLevel,
    LevelData,
    RequestRoomData,
    RoomData
}

public enum PacketTargets
{
    ServerOnly = -1,
    RelayToAllClients = -2,
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public int ServerPort = 8080;
    public int MaxConnections = 100;

    private Dictionary<int, Connection> Connections = new Dictionary<int, Connection>();
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

    void AddConnection(int ConnectionID)
    {
        Connection NewConnection = ScriptableObject.CreateInstance<Connection>();
        NewConnection.ConnectionID = ConnectionID;    
        Connections.Add(ConnectionID, NewConnection);
    }

    void RemoveConnection(int ConnectionID)
    {
        Connections.Remove(ConnectionID);
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
        foreach (KeyValuePair<int, Connection> connection in Connections)
        {
            if (connection.Key != SenderConnectionID)
            {
                Packet.SetPacketTarget(connection.Key);
                SendPacketToClient(Packet, QosChannelID);
            }
        }
    }

    public void SendPacketToAllClients(NetworkPacket Packet, int QosChannelID)
    {
        foreach (KeyValuePair<int, Connection> connection in Connections)
        {
            Packet.SetPacketTarget(connection.Key);
            SendPacketToClient(Packet, QosChannelID);    
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

    public QosType GetQOSType(int ChannelID)
    {
        if(ChannelID == ReliableChannelId)
        {
            return QosType.Reliable;
        }
        else if (ChannelID == ReliableSequencedChannelId)
        {
            return QosType.ReliableSequenced;
        }
        else if(ChannelID == UnreliableChannelId)
        {
            return QosType.Unreliable;
        }
        Debug.Log("ChannelID " + ChannelID + " has not been implemented defaulting to Unreliable");
        return QosType.Unreliable;
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
                    RecPacket.SetIsTargetRoom(BitConverter.ToBoolean(recBuffer, 8));
                    RecPacket.SetPacketData(recBuffer, 13, BitConverter.ToInt32(recBuffer, 9));
                    NetworkPacketReader.ReadPacket(RecPacket, recConnectionId, true);
              
                    if (RecPacket.GetPacketTarget() != (int)PacketTargets.ServerOnly)
                    {
                        if (RecPacket.GetPacketTarget() == (int)PacketTargets.RelayToAllClients)
                        {
                            RelayPacketToAllClients(RecPacket, recConnectionId, recChannelId);
                        }
                        else if (RecPacket.GetIsTargetRoom())
                        {
                            NetworkPacketSender.RelayPacketToPlayersInRoom(RecPacket, LevelManager.Instance.GetRoom(RecPacket.GetPacketTarget()), GetQOSType(recChannelId), false);
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
