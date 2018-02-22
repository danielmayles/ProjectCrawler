using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject PlayerPrefab;
    public GameObject ControllerablePlayerPrefab;
    public GameObject ServerPlayerPrefab;
    public Vector3 PlayerPoolSpawnPosition;

    private Dictionary<int, Player> Players = new Dictionary<int, Player>();
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public Player SpawnPlayer(int ConnectionID)
    {
#if SERVER
        return SpawnServerPlayer(ConnectionID);
#else
        if (ConnectionID == NetworkDetails.GetLocalConnectionID())
        {
           return SpawnControllerablePlayer(ConnectionID);
        }
     
        return SpawnNormalPlayer(ConnectionID);       
#endif
    }

    public Player SpawnNormalPlayer(int ConnectionID)
    {
        Player player = Instantiate(PlayerPrefab, PlayerPoolSpawnPosition, PlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(ConnectionID, player);

        return player;
    }

    public Player SpawnControllerablePlayer(int ConnectionID)
    {
        Player player = Instantiate(ControllerablePlayerPrefab, PlayerPoolSpawnPosition, ControllerablePlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(ConnectionID, player);

        return player;
    }

    public Player SpawnServerPlayer(int ConnectionID)
    {
        Player player = Instantiate(ServerPlayerPrefab, PlayerPoolSpawnPosition, ServerPlayerPrefab.transform.rotation, transform).GetComponent<Player>();
        player.InitPlayer(ConnectionID);
        player.SetAlive();
        Players.Add(ConnectionID, player);
        return player;
    }

    public void RequestCurrentPlayers()
    {
        NetworkPacketSender.RequestCurrentPlayers();
    }

    public byte[] GetPlayersAsBytes()
    {
        List<byte> Data = new List<byte>();
        Data.AddRange(BitConverter.GetBytes(Players.Count));
        foreach (int PlayerConnectionID in Players.Keys)
        {
            Data.AddRange(BitConverter.GetBytes(PlayerConnectionID));
            Data.AddRange(BitConverter.GetBytes(Players[PlayerConnectionID].CurrentRoom.GetRoomIndex()));
        }
        return Data.ToArray();
    }

    public void ReadInPlayersAsBytes(byte[] Bytes)
    {
        int CurrentByteIndex = 0;
        int PlayerCount = BitConverter.ToInt32(Bytes, 0);
        CurrentByteIndex += 4;
        Players.Clear();

        for (int i = 0; i < PlayerCount; i++)
        {
            int PlayerConnectionIndex = BitConverter.ToInt32(Bytes, CurrentByteIndex);
            CurrentByteIndex += 4;
            Player player = SpawnPlayer(PlayerConnectionIndex);

            if(!Players.ContainsKey(PlayerConnectionIndex))
            {
                Players.Add(PlayerConnectionIndex, player);
            }

            int PlayerRoomIndex = BitConverter.ToInt32(Bytes, CurrentByteIndex);
            CurrentByteIndex += 4;
            LevelManager.Instance.GetRoom(0).PlayerJoinRoom(PlayerConnectionIndex);
        }
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
