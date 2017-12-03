using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public Room CurrentRoom;
    private int ConnectionID;

    public void InitPlayer(int ConnectionID)
    {
        this.ConnectionID = ConnectionID;
    }

    public void SetAlive()
    {
        isAlive = true;
    }

    public void Kill()
    {
        isAlive = false;
    }
     
    public int GetPlayerConnectionID()
    {
        return ConnectionID;
    }
}
