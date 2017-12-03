using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetworkDetails
{
    private static int LocalConnectionID = -1;

    public static int GetLocalConnectionID()
    {
        return LocalConnectionID;
    }

    public static void SetLocalConnectionID(int ConnectionID)
    {
        LocalConnectionID = ConnectionID;
    }
}
