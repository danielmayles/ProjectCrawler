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

    public void PlayerReady()
    {
        SetupGame();
    }

    public void SetupGame()
    {
        RoomManager.Instance.StartSpawnRooms();

        //QQQ Do spawning players! 
        NetworkPacketSender.SendSpawnPlayer()
    }
}
