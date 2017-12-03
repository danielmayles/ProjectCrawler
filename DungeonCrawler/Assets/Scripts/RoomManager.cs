using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject[] RoomPrefabs;
  
    void Start()
    {
        StartSpawnRooms();
    }

    public void StartSpawnRooms()
    {
        StartCoroutine(SpawnRooms());
    }

    public IEnumerator SpawnRooms()
    {
        yield return new WaitForEndOfFrame();
    }

}
