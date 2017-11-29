using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject PlayerPrefab;
    public GameObject ControllerablePlayerPrefab;
    public GameObject SpawnPoint;

    private List<Player> Players = new List<Player>();
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    
    public void AddPlayer(int ConnectionID)
    {
        Player player = Instantiate(PlayerPrefab, SpawnPoint.transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(player);
    }

    public void AddControllerablePlayer(int ConnectionID)
    {
        Player player = Instantiate(ControllerablePlayerPrefab, SpawnPoint.transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(player);
    }

    public Player GetPlayer(int PlayerIndex)
    {
        return Players[PlayerIndex];
    }

    public int GetAmountOfPlayers()
    {
        return Players.Count;
    }
}
