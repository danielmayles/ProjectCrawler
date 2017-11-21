using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private List<Player> Players = new List<Player>();
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
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
