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
        NetworkPacketSender.SendLevelData(ConnectionID);
        NetworkPacketSender.SendPlayerData(ConnectionID);
        NetworkPacketSender.SendSpawnPlayer(ConnectionID);
        LevelManager.Instance.GetRoom(0).PlayerJoinRoom(ConnectionID, Vector3.zero);
    }
}
