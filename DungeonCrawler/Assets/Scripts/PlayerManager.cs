using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject PlayerPrefab;
    public GameObject ControllerablePlayerPrefab;
    public GameObject ServerPlayerPrefab;

    private Dictionary<int, Player> Players = new Dictionary<int, Player>();
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void SpawnPlayer(int ConnectionID, int RoomIndex)
    {
        Player player = Instantiate(PlayerPrefab, LevelManager.Instance.GetRoom(RoomIndex).transform.position, PlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(ConnectionID, player);
    }

    public void SpawnControllerablePlayer(int ConnectionID, int RoomIndex)
    {
        Player player = Instantiate(ControllerablePlayerPrefab, LevelManager.Instance.GetRoom(RoomIndex).transform.position, ControllerablePlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(ConnectionID, player);
    }

    public void SpawnServerPlayer(int ConnectionID, int RoomIndex)
    {
        Player player = Instantiate(ServerPlayerPrefab, LevelManager.Instance.GetRoom(RoomIndex).transform.position, ServerPlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(ConnectionID, player);
    }

    public void SendSpawnPlayer(int ConnectionID, int RoomIndex)
    {
        SpawnServerPlayer(ConnectionID, RoomIndex);
        NetworkPacketSender.SendSpawnPlayer(ConnectionID, RoomIndex);
    }

    public Player GetPlayer(int ConnectionID)
    {
        return Players[ConnectionID];
    }

    public int GetAmountOfPlayers()
    {
        return Players.Count;
    }
}
