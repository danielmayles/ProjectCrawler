using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public float RoomWidth;
    public float RoomHeight;
    public GameObject[] RoomEntrances;

    private Dictionary<int, Player> PlayersInRoom = new Dictionary<int, Player>();
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
        return new List<Player>(PlayersInRoom.Values).ToArray();
    }

    public Vector2 GetRoomSize()
    {
        return new Vector2(RoomWidth, RoomHeight);
    }

    public void PlayerJoinRoom(int PlayerConnectionID)
    {
        Player player = PlayerManager.Instance.GetPlayer(PlayerConnectionID);
        player.CurrentRoom = this;
        if (!PlayersInRoom.ContainsKey(PlayerConnectionID))
        {
            PlayersInRoom.Add(PlayerConnectionID, player);
        }

        //QQQ Send over the entrance Index over the network
        player.transform.position = RoomEntrances[0].transform.position;
        player.IsInRoom();
    }

    public void PlayerLeaveRoom(int PlayerConnectionID)
    {
        PlayerLeaveRoom(PlayerConnectionID);
    }
}
