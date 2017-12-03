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
    
    public void SpawnPlayer(int ConnectionID)
    {
        Player player = Instantiate(PlayerPrefab, SpawnPoint.transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(player);
    }

    public void SpawnControllerablePlayer(int ConnectionID)
    {
        Player player = Instantiate(ControllerablePlayerPrefab, SpawnPoint.transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(player);
    }

    public Player GetPlayer(int ConnectionID)
    {
        for(int i = 0; i < Players.Count; i++)
        {
            if(Players[i].GetPlayerConnectionID() == ConnectionID)
            {
                return Players[i];
            }
        }
        return null;
    }

    public int GetAmountOfPlayers()
    {
        return Players.Count;
    }
}
