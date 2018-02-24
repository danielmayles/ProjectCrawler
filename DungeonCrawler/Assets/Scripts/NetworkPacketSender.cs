using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPacketSender : MonoBehaviour
{
    public static void SendPacketToAllPlayers(NetworkPacket Packet, QosType QualityOfServiceType, bool ShouldServerReadPacket)
    {
#if SERVER
        if (ShouldServerReadPacket)
        {
            NetworkPacketReader.ReadPacket(Packet, NetworkDetails.GetLocalConnectionID(), true);
        }
        NetworkManager.Instance.SendPacketToAllClients(Packet, NetworkManager.Instance.GetChannel(QualityOfServiceType));
#else
        Packet.SetPacketTarget(PacketTargets.RelayToAllClients);
        ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
#endif
    }

    public static void RelayPacketToPlayersInRoom(NetworkPacket Packet, Room TargetRoom, QosType QualityOfServiceType, bool ShouldServerReadPacket)
    {
#if SERVER
        if (ShouldServerReadPacket)
        {
            NetworkPacketReader.ReadPacket(Packet, NetworkDetails.GetLocalConnectionID(), true);
        }

        Packet.SetIsTargetRoom(false);
        int[] PlayerConnectionIDs = TargetRoom.GetPlayersInRoom();
        for (int i = 0; i < PlayerConnectionIDs.Length; i++)
        {
            Packet.SetPacketTarget(PlayerConnectionIDs[i]);
            NetworkManager.Instance.SendPacketToClient(Packet, QualityOfServiceType);
        }
#else
        Packet.SetPacketTarget(TargetRoom.GetRoomIndex());
        Packet.SetIsTargetRoom(true);
        ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
#endif

    }

    public static void SendToServerOnly(NetworkPacket Packet, QosType QualityOfServiceType)
    {
        Packet.SetPacketTarget(PacketTargets.ServerOnly);
#if SERVER
        NetworkPacketReader.ReadPacket(Packet, NetworkDetails.GetLocalConnectionID(), true);
#else
        ClientNetworkManager.Instance.SendPacketToServer(Packet, QualityOfServiceType);
#endif
    }

    public static void SendPlayerReady(int PlayerConnectionID)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.PlayerReady;
        networkPacket.SetPacketData(BitConverter.GetBytes(PlayerConnectionID), 0, sizeof(int));
        SendToServerOnly(networkPacket, QosType.Reliable);
    }

    public static void SendSpawnPlayer(int PlayerConnectionID)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.SpawnPlayer;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        SendPacketToAllPlayers(networkPacket, QosType.Reliable, true);
    }

    public static void SendSpawnRoom(int RoomIndex, int RoomPrefabIndex, Vector3 Position)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.SpawnRoom;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(RoomIndex));
        data.AddRange(BitConverter.GetBytes(RoomPrefabIndex));
        data.AddRange(Serializer.GetBytes(Position));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        SendPacketToAllPlayers(networkPacket, QosType.Reliable, false);
    }

    public static void SendPlayerPosition(int PlayerConnectionID, int InputID, Vector3 NewPosition, Room PlayersCurrentRoom)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.PlayerPosition;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        data.AddRange(BitConverter.GetBytes(InputID));
        data.AddRange(Serializer.GetBytes(NewPosition));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        RelayPacketToPlayersInRoom(networkPacket, PlayersCurrentRoom, QosType.Unreliable, false);
    }

    public static void SendPlayerTransform(int PlayerConnectionID, Transform PlayerTransform)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.PlayerTransform;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        data.AddRange(Serializer.GetBytes(PlayerTransform.position));
        data.AddRange(Serializer.GetBytes(PlayerTransform.eulerAngles));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        SendPacketToAllPlayers(networkPacket, QosType.Unreliable, true);
    }

    public static void SendPlayerInput(int PlayerConnectionID, List<InputType> Inputs, int InputID, float DeltaTime)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.PlayerInputUpdate;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        data.AddRange(BitConverter.GetBytes(DeltaTime));
        data.AddRange(BitConverter.GetBytes(InputID));
        data.AddRange(BitConverter.GetBytes(Inputs.Count));
        for(int i = 0; i < Inputs.Count; i++)
        {
            data.AddRange(BitConverter.GetBytes((int)Inputs[i]));
        }
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        SendToServerOnly(networkPacket, QosType.Reliable);
        Inputs.Clear();
    }

    public static void SendRagdollPlayer(int PlayerConnectionID)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.RagdollPlayer;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        SendPacketToAllPlayers(networkPacket, QosType.Reliable, true);
    }

    public static void AddPlayerToRoom(int PlayerConnectionID, int RoomIndex, Vector3 PlayerPosition)
    {
        AddPlayerToRoom(PlayerConnectionID, LevelManager.Instance.GetRoom(RoomIndex), PlayerPosition);
    }

    public static void AddPlayerToRoom(int PlayerConnectionID, Room room, Vector3 PlayerPosition)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.AddPlayerToRoom;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        data.AddRange(BitConverter.GetBytes(room.GetRoomIndex()));
        data.AddRange(Serializer.GetBytes(PlayerPosition));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        RelayPacketToPlayersInRoom(networkPacket, room, QosType.Reliable, false);
    }

    public static void RemovePlayerFromRoom(int PlayerConnectionID, int RoomIndex)
    {
        RemovePlayerFromRoom(PlayerConnectionID, LevelManager.Instance.GetRoom(RoomIndex));
    }

    public static void RemovePlayerFromRoom(int PlayerConnectionID, Room room)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.RemovePlayerFromRoom;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        data.AddRange(BitConverter.GetBytes(room.GetRoomIndex()));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        networkPacket.SetPacketTarget(PlayerConnectionID);
        RelayPacketToPlayersInRoom(networkPacket, room, QosType.Reliable, false);
    }

    public static void SpawnPlayerInRoom(int PlayerConnectionID, int RoomIndex)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.SpawnPlayerInRoom;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
        data.AddRange(BitConverter.GetBytes(RoomIndex));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        RelayPacketToPlayersInRoom(networkPacket, LevelManager.Instance.GetRoom(RoomIndex), QosType.Reliable, true);
    }

    public static void SendRequestLevel()
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.RequestLevel;
        SendToServerOnly(networkPacket, QosType.Reliable);
    }

    public static void SendLevelData(int PlayerConnectionID)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.LevelData;
        networkPacket.SetPacketTarget(PlayerConnectionID);
        List<byte> data = new List<byte>();
        data.AddRange(LevelManager.Instance.GetLevelAsBytes());
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        NetworkManager.Instance.SendPacketToClient(networkPacket, QosType.Reliable);
    }

    public static void SendRequestRoomData(int RoomIndex)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.RequestRoomData;
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(RoomIndex));
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        SendToServerOnly(networkPacket, QosType.Reliable);
    }

    public static void SendRoomData(int PlayerConnectionID, int RoomIndex)
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.RoomData;
        networkPacket.SetPacketTarget(PlayerConnectionID);
        List<byte> data = new List<byte>();
        data.AddRange(LevelManager.Instance.GetRoom(RoomIndex).GetRoomAsBytes());
        networkPacket.SetPacketData(data.ToArray(), 0, data.Count);
        NetworkManager.Instance.SendPacketToClient(networkPacket, QosType.Reliable);
    }
    
    public static void RequestCurrentPlayers()
    {
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.PacketHeader = NetworkPacketHeader.RequestCurrentPlayers;
        SendToServerOnly(networkPacket, QosType.Reliable);
    }

    public static void SendPlayerData(int ConnectionID)
    {
        //QQQ Players not being transfered properly on first play
        byte[] PlayerData = PlayerManager.Instance.GetPlayersAsBytes();
        NetworkPacket networkPacket = ScriptableObject.CreateInstance<NetworkPacket>();
        networkPacket.SetPacketTarget(ConnectionID);
        networkPacket.PacketHeader = NetworkPacketHeader.PlayerData;
        networkPacket.SetPacketData(PlayerData, 0, PlayerData.Length);
        NetworkManager.Instance.SendPacketToClient(networkPacket, QosType.Reliable);
    }
}
