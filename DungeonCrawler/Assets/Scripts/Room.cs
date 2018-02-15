using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public float RoomWidth;
    public float RoomHeight;

    private List<Player> PlayersInRoom = new List<Player>();
    private List<Room> NearbyRooms = new List<Room>();
    private Bounds RoomBounds;
    private int RoomIndex;
    private int RoomPrefabIndex;

    public void SetRoomIndex(int newIndex)
    {
        RoomIndex = newIndex;
    }

    public int GetRoomIndex()
    {
        return RoomIndex;
    }

    public void SetRoomPrefabIndex(int PrefabIndex)
    {
        RoomPrefabIndex = PrefabIndex;
    }

    public int GetRoomPrefabIndex()
    {
        return RoomPrefabIndex;
    }

    public Player[] GetPlayersInRoom()
    {
        return PlayersInRoom.ToArray();
    }

    public Vector2 GetRoomSize()
    {
        return new Vector2(RoomWidth, RoomHeight);
    }

    public void PlayerJoinRoom(int PlayerConnectionID)
    {
        PlayerLeaveRoom(PlayerConnectionID);
        PlayerJoinRoom(PlayerManager.Instance.GetPlayer(PlayerConnectionID));
    }

    public void PlayerLeaveRoom(int PlayerConnectionID)
    {
        PlayerLeaveRoom(PlayerManager.Instance.GetPlayer(PlayerConnectionID));
    }

    public void PlayerJoinRoom(Player player)
    {
        player.CurrentRoom.PlayerLeaveRoom(player);
        player.CurrentRoom = this;
        PlayersInRoom.Add(player);
    }

    public void PlayerLeaveRoom(Player player)
    {
        PlayersInRoom.Remove(player);
    }
}
