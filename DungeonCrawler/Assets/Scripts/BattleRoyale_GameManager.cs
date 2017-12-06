using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoyale_GameManager : MonoBehaviour
{
    public static BattleRoyale_GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlayerReady(int ConnectionID)
    {
        SpawnRoomAndPlayer(ConnectionID);
    }

    public void SpawnRoomAndPlayer(int ConnectionID)
    {
        Room NewRoom = RoomManager.Instance.AddNewRoom(true);
        PlayerManager.Instance.ServerSpawnPlayer(ConnectionID, NewRoom.GetRoomIndex());
    }
}
