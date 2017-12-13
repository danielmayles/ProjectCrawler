using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject PlayerPrefab;
    public GameObject ControllerablePlayerPrefab;

    private List<Player> Players = new List<Player>();
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void SpawnPlayer(int ConnectionID, int RoomIndex)
    {
        Player player = Instantiate(PlayerPrefab, RoomManager.Instance.GetRoom(RoomIndex).transform.position, PlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(player);
    }

    public void SpawnControllerablePlayer(int ConnectionID, int RoomIndex)
    {
        Player player = Instantiate(ControllerablePlayerPrefab, RoomManager.Instance.GetRoom(RoomIndex).transform.position, ControllerablePlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(player);
    }

    public void ServerSpawnPlayer(int ConnectionID, int RoomIndex)
    {
        SpawnPlayer(ConnectionID, RoomIndex);
        NetworkPacketSender.SendSpawnPlayer(ConnectionID, RoomIndex);
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
