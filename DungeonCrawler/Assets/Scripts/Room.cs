using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private List<Player> PlayersInRoom = new List<Player>();
    private List<Room> NearbyRooms = new List<Room>();
    private Bounds RoomBounds;
    private int RoomIndex;

    private void Awake()
    {
        CalculateBounds();
    }

    public void SetRoomIndex(int newIndex)
    {
        RoomIndex = newIndex;
    }

    public int GetRoomIndex()
    {
        return RoomIndex;
    }

    public void CalculateBounds()
    {
        Renderer[] Renderers = GetComponentsInChildren<Renderer>();
        for(int i = 0; i < Renderers.Length; i++)
        {
            RoomBounds.Encapsulate(Renderers[i].bounds);
        }
    }

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

    public Vector2 GetRoomSize()
    {
        return RoomBounds.size;
    }
}
