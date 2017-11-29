using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private List<Player> PlayersInRoom = new List<Player>();
    private List<Room> NearbyRooms = new List<Room>();


    public Player[] GetPlayersAndNearbyPlayers()
    {
        List<Player> NearByPlayers = new List<Player>();
        NearByPlayers.AddRange(PlayersInRoom);
        for(int i = 0; i < NearbyRooms.Count; i++)
        {
            NearByPlayers.AddRange(NearbyRooms[i].GetPlayersInRoom());
        }

        return NearByPlayers.ToArray();
    }


    public Player[] GetPlayersInRoom()
    {
        List<Player> Players = new List<Player>();
        Players.AddRange(PlayersInRoom);
        return Players.ToArray();
    }


}
