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
    ArmDirection,
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
    private List<int> CurrentConnectedClientConnectionIDs = new List<int>();
    private int AmountOfActiveConnections;
    private int ReceivePacketSize = 2048;
    private int ReliableSequencedChannelId;
    private int ReliableChannelId;
    private int UnreliableChannelId;
    private int socketId;

    //Debug Varibles
    int Debug_AmountOfPacketsSentPerSecond;
    int Debug_AmountOfPacketsReceivedPerSecond;
    int Debug_TotalSizeOfPacketsSentPerSecond;
    int Debug_TotalSizeOfPacketsReceivedPerSecond;

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
        StartCoroutine(NetworkDebugLoop());
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
        CurrentConnectedClientConnectionIDs.Add(ConnectionID);
    }

    void RemoveConnection(int ConnectionID)
    {
        Connections.Remove(ConnectionID);
        CurrentConnectedClientConnectionIDs.Remove(ConnectionID);
    }

    public int GetAmountOfConnections()
    {
        return Connections.Count;
    }

    public void SendPacketToClient(NetworkPacket packet, int QosChannelID)
    {
        Debug.Log("Sent Packet: " + packet.PacketHeader + " Size: " + packet.GetDataSize());

        byte error;
        NetworkTransport.Send(socketId, packet.GetPacketTarget(), QosChannelID, packet.GetBytes(), packet.GetTotalPacketSize(), out error);

        Debug_AmountOfPacketsSentPerSecond++;
        Debug_TotalSizeOfPacketsSentPerSecond += packet.GetTotalPacketSize();
    }

    public void SendPacketToClient(NetworkPacket packet, QosType QosChannel)
    {
        Debug.Log("Sent Packet: " + packet.PacketHeader + " Size: " + packet.GetDataSize());

        byte error;
        NetworkTransport.Send(socketId, packet.GetPacketTarget(), GetChannel(QosChannel), packet.GetBytes(), packet.GetTotalPacketSize(), out error);

        Debug_AmountOfPacketsSentPerSecond++;
        Debug_TotalSizeOfPacketsSentPerSecond += packet.GetTotalPacketSize();
    }

    public void RelayPacketToAllClients(NetworkPacket Packet, int SenderConnectionID, int QosChannelID)
    {
        for(int i = 0; i < CurrentConnectedClientConnectionIDs.Count; i++)
        { 
            if (CurrentConnectedClientConnectionIDs[i] != SenderConnectionID)
            {
                Packet.SetPacketTarget(CurrentConnectedClientConnectionIDs[i]);
                SendPacketToClient(Packet, QosChannelID);
            }
        }
    }

    public void SendPacketToAllClients(NetworkPacket Packet, int QosChannelID)
    {
        for (int i = 0; i < CurrentConnectedClientConnectionIDs.Count; i++)
        {
            Packet.SetPacketTarget(CurrentConnectedClientConnectionIDs[i]);
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
                    Debug.Log("Received Packet: " + RecPacket.PacketHeader + " Size: " + RecPacket.GetDataSize());
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
               
                    Debug_AmountOfPacketsReceivedPerSecond++;
                    Debug_TotalSizeOfPacketsReceivedPerSecond += RecPacket.GetTotalPacketSize();
                    break;
                case NetworkEventType.DisconnectEvent:
                    RemoveConnection(recHostId);
                    Debug.Log("remote client event disconnected");
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator NetworkDebugLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            //Debug.Log("Server - Amount of Packets Received Per Second: " + Debug_AmountOfPacketsReceivedPerSecond);
            //Debug.Log("Server - Amount of Packets Sent Per Second: " + Debug_AmountOfPacketsSentPerSecond);

           // Debug.Log("Server - Total Size of Packets Received Per Second: " + Debug_TotalSizeOfPacketsReceivedPerSecond);
           // Debug.Log("Server - Total Size of Packets Sent Per Second: " + Debug_TotalSizeOfPacketsSentPerSecond);

            Debug_AmountOfPacketsReceivedPerSecond = 0;
            Debug_AmountOfPacketsSentPerSecond = 0;
            Debug_TotalSizeOfPacketsReceivedPerSecond = 0;
            Debug_TotalSizeOfPacketsSentPerSecond = 0;
        }
    }
}
