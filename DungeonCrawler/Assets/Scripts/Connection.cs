using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : ScriptableObject
{ 
    public int ConnectionID;
    public bool isActive;

    public Connection(int ConnectionID)
    {
        this.ConnectionID = ConnectionID;
        isActive = true;
    }
}
