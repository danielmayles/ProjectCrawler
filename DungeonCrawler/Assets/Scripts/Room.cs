using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public float RoomWidth;
    public float RoomHeight;
    public GameObject[] RoomEntrances;

    private List<int> PlayerConnectionIDsInRoom = new List<int>();
    private int RoomIndex;
    private int RoomPrefabIndex;

    public void SetRoomIndex(int newIndex)
    {
        RoomIndex = newIndex;
    }

    public int GetRoomIndex()
    {
        return RoomIndex;
    }

    public void SetRoomPrefabIndex(int PrefabIndex)
    {
        RoomPrefabIndex = PrefabIndex;
    }

    public int GetRoomPrefabIndex()
    {
        return RoomPrefabIndex;
    }

    public int[] GetPlayersInRoom()
    {
        return PlayerConnectionIDsInRoom.ToArray();
    }

    public Vector2 GetRoomSize()
    {
        return new Vector2(RoomWidth, RoomHeight);
    }

    public void PlayerJoinRoom(int PlayerConnectionID, Vector3 Position)
    {
        Player player = PlayerManager.Instance.GetPlayer(PlayerConnectionID);
        player.OnPlayerChangeRooms(this);
        if (!PlayerConnectionIDsInRoom.Contains(PlayerConnectionID))
        {
            PlayerConnectionIDsInRoom.Add(PlayerConnectionID);
        }

        if (NetworkDetails.IsLocalConnectionID(PlayerConnectionID))
        {
            for (int i = 0; i < PlayerConnectionIDsInRoom.Count; i++)
            {
                PlayerManager.Instance.GetPlayer(PlayerConnectionIDsInRoom[i]).SetIsVisible(true);
            }
        }

#if SERVER
       NetworkPacketSender.SendRoomData(PlayerConnectionID, RoomIndex);
       NetworkPacketSender.AddPlayerToRoom(PlayerConnectionID, this, Position);
#endif

       player.transform.position = Position;
       player.SetIsVisible(true);
    }

    public void PlayerLeaveRoom(int PlayerConnectionID)
    {
#if SERVER
        NetworkPacketSender.RemovePlayerFromRoom(PlayerConnectionID, this);
#endif

        PlayerConnectionIDsInRoom.Remove(PlayerConnectionID);
        if (NetworkDetails.IsLocalConnectionID(PlayerConnectionID))
        {
            for (int i = 0; i < PlayerConnectionIDsInRoom.Count; i++)
            {
                PlayerManager.Instance.GetPlayer(PlayerConnectionIDsInRoom[i]).SetIsVisible(false);
            }
        }
        else
        {
            PlayerManager.Instance.GetPlayer(PlayerConnectionID).SetIsVisible(false);
        }
    }

    public byte[] GetRoomAsBytes()
    {
        List<byte> Data = new List<byte>();
        Data.AddRange(BitConverter.GetBytes(RoomIndex));
        Data.AddRange(BitConverter.GetBytes(PlayerConnectionIDsInRoom.Count));
        for (int i = 0; i < PlayerConnectionIDsInRoom.Count; i++)
        {
            Data.AddRange(BitConverter.GetBytes(PlayerConnectionIDsInRoom[i]));
            Data.AddRange(Serializer.GetBytes(PlayerManager.Instance.GetPlayer(PlayerConnectionIDsInRoom[i]).transform.position));
        }        
        return Data.ToArray();
    }

    public void ReadInRoomAsBytes(byte[] RoomBytes)
    {
        PlayerConnectionIDsInRoom.Clear();

        int CurrentByteIndex = 4;
        int PlayerCount = BitConverter.ToInt32(RoomBytes, CurrentByteIndex);
        CurrentByteIndex += 4;

        for (int i = 0; i < PlayerCount; i++)
        { 
            int PlayerConnectionID = BitConverter.ToInt32(RoomBytes, CurrentByteIndex);
            PlayerConnectionIDsInRoom.Add(PlayerConnectionID);
            CurrentByteIndex += 4;
            Player playerInRoom = PlayerManager.Instance.GetPlayer(PlayerConnectionID);
            playerInRoom.transform.position = Serializer.DeserializeToVector3(RoomBytes, CurrentByteIndex);
            CurrentByteIndex += 12;
        }
    }
}
