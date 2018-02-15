using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] RoomPrefabs;
    public static LevelManager Instance;
    private Dictionary<int, Room> Rooms = new Dictionary<int, Room>();
    private Room CurrentRoom;
 
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

#if SERVER
        SpawnLevel(20);
#endif
    }

    public Room GetRoom(int RoomIndex)
    {
        return Rooms[RoomIndex];
    }

    Room SpawnRoom(int RoomIndex, int RoomPrefabIndex, Vector3 SpawnPosition)
    {
        Room newRoom = Instantiate(RoomPrefabs[RoomPrefabIndex], SpawnPosition, RoomPrefabs[RoomPrefabIndex].transform.rotation, transform).GetComponent<Room>();
        newRoom.SetRoomIndex(RoomIndex);
        newRoom.SetRoomPrefabIndex(RoomPrefabIndex);
        Rooms.Add(RoomIndex, newRoom);
        return newRoom;
    }

    public void SpawnLevel(int AmountOfRooms)
    {
        Vector3 RoomPosition = Vector3.zero;
        for(int i = 0; i < AmountOfRooms; i++)
        {
            Room newRoom = SpawnRoom(i, 0, RoomPosition);
            RoomPosition.x += newRoom.RoomWidth;
        }
    }

    public byte[] GetLevelAsBytes()
    {
        List<byte> Data = new List<byte>();
        Data.AddRange(BitConverter.GetBytes(Rooms.Count));
        for(int i = 0; i < Rooms.Count; i++)
        {
            Data.AddRange(BitConverter.GetBytes(Rooms[i].GetRoomIndex()));
            Data.AddRange(BitConverter.GetBytes(Rooms[i].GetRoomPrefabIndex()));
            Data.AddRange(Serializer.GetBytes(Rooms[i].transform.position));
        }
        return Data.ToArray();
    }

    public void ReadInLevelBytes(byte[] LevelBytes)
    {
        int RoomCount = BitConverter.ToInt32(LevelBytes, 0);
        Rooms.Clear();
        for(int i = 0; i < RoomCount; i++)
        {
            int RoomIndex = BitConverter.ToInt32(LevelBytes, 4 + (4 * i));
            int RoomPrefabIndex= BitConverter.ToInt32(LevelBytes, 8 + (4 * i));
            Vector3 RoomPosition = Serializer.DeserializeToVector3(LevelBytes, 12 + (12 * i));
            SpawnRoom(RoomIndex, RoomPrefabIndex, RoomPosition);
        }
    }

    public byte[] GetRoomAsBytes(int RoomIndex)
    {
        List<byte> Data = new List<byte>();
        Data.AddRange(BitConverter.GetBytes(RoomIndex));

        Player[] PlayersInRoom = Rooms[RoomIndex].GetPlayersInRoom();
        Data.AddRange(BitConverter.GetBytes(PlayersInRoom.Length));
        for (int i = 0; i < PlayersInRoom.Length; i++)
        {
            Data.AddRange(BitConverter.GetBytes(PlayersInRoom[i].GetPlayerConnectionID()));
        }        
        return Data.ToArray();
    }

    public void ReadInRoomAsBytes(byte[] RoomBytes)
    {   
        int RoomIndex = BitConverter.ToInt32(RoomBytes, 0);
        CurrentRoom = SpawnRoom(RoomIndex, 0, Vector3.zero);

        int PlayerCount = BitConverter.ToInt32(RoomBytes, 4);
        for (int i = 0; i < PlayerCount; i++)
        { 
            int PlayerConnectionID = BitConverter.ToInt32(RoomBytes, 8 + (4 * i));
            CurrentRoom.PlayerJoinRoom(PlayerManager.Instance.GetPlayer(PlayerConnectionID));
        }
    }
}
