using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private List<Character> PlayersInRoom = new List<Character>();
    private List<Room> NearbyRooms = new List<Room>();


    public Character[] GetPlayersAndNearbyPlayers()
    {
        List<Character> NearByPlayers = new List<Character>();
        NearByPlayers.AddRange(PlayersInRoom);
        for(int i = 0; i < NearbyRooms.Count; i++)
        {
            NearByPlayers.AddRange(NearbyRooms[i].GetPlayersInRoom());
        }

        return NearByPlayers.ToArray();
    }


    public Character[] GetPlayersInRoom()
    {
        List<Character> Players = new List<Character>();
        Players.AddRange(PlayersInRoom);
        return Players.ToArray();
    }


}
