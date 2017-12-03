using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject[] RoomPrefabs;
    public int AmountOfRoomsHeigh;
    public int AmountOfRoomsWidth;

    private Room[] Rooms;
    public static RoomManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        Rooms = new Room[AmountOfRoomsHeigh * AmountOfRoomsWidth];
    }

    public void StartSpawnRooms()
    {
        StartCoroutine(SpawnRooms());
    }

    public IEnumerator SpawnRooms()
    {
        Vector3 CurrentSpawnPoint = Vector3.zero;  
        int CurrentRoomIndex = 0;
     
        for (int y = 0; y < AmountOfRoomsHeigh; y++)
        {
            for (int x = 0; x < AmountOfRoomsWidth; x++)
            {
                CurrentRoomIndex = x + y;
                int RoomPrefabIndex = Random.Range(0, RoomPrefabs.Length);
                SpawnRoom(CurrentRoomIndex, RoomPrefabIndex, CurrentSpawnPoint);
                NetworkPacketSender.SendSpawnRoom(CurrentRoomIndex, RoomPrefabIndex, CurrentSpawnPoint);
                CurrentSpawnPoint.x += Rooms[CurrentRoomIndex].GetRoomSize().x;
            }
            CurrentSpawnPoint.x = 0;
            CurrentSpawnPoint.y += Rooms[CurrentRoomIndex].GetRoomSize().y;
            yield return new WaitForEndOfFrame();
        }
    }

    public void SpawnRoom(int RoomIndex, int RoomPrefabIndex, Vector3 SpawnPosition)
    {
        Debug.Log("Room Index " + RoomIndex);
        Debug.Log("Room Prefab Index " + RoomPrefabIndex);
        Rooms[RoomIndex] = Instantiate(RoomPrefabs[RoomPrefabIndex], SpawnPosition, RoomPrefabs[RoomPrefabIndex].transform.rotation, transform).GetComponent<Room>();
    }
}
