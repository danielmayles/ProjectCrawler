using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject[] RoomPrefabs;
    public int AmountOfRoomsHeigh;
    public int AmountOfRoomsWidth; 
    public static RoomManager Instance;
    private List<Room> Rooms = new List<Room>();
    private Vector3 CurrentRoomSpawnPoint = Vector3.zero;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public Room AddNewRoom(bool ReplicateOverNetwork = false)
    {
        int RoomPrefabIndex = Random.Range(0, RoomPrefabs.Length);
        Room newRoom = SpawnRoom(Rooms.Count, RoomPrefabIndex, CurrentRoomSpawnPoint);
        //CurrentRoomSpawnPoint.x += newRoom.GetRoomSize().x;
        if (ReplicateOverNetwork)
        {
            NetworkPacketSender.SendSpawnRoom(newRoom.GetRoomIndex(), RoomPrefabIndex, CurrentRoomSpawnPoint);
        }
        Rooms.Add(newRoom);
        return newRoom;
    }

    public Room GetRoom(int RoomIndex)
    {
        return Rooms[RoomIndex];
    }
     
    public void SetNewRoom(int RoomIndex, int RoomPrefabIndex, Vector3 Position)
    {
        if(Rooms.Count <= RoomIndex)
        {
            Room[] newRoomElements = new Room[RoomIndex - (Rooms.Count - 1)];
            Rooms.AddRange(newRoomElements);
        }

        Rooms[RoomIndex] = SpawnRoom(RoomIndex, RoomPrefabIndex, Position);
    }

    Room SpawnRoom(int RoomIndex, int RoomPrefabIndex, Vector3 SpawnPosition)
    {
        Room newRoom = Instantiate(RoomPrefabs[RoomPrefabIndex], SpawnPosition, RoomPrefabs[RoomPrefabIndex].transform.rotation, transform).GetComponent<Room>();
        newRoom.SetRoomIndex(RoomIndex);
        return newRoom;
    }
}
