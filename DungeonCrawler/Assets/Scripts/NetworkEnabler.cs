using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEnabler : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public ClientNetworkManager ClientNetworkManager;

    void Awake()
    {
#if SERVER
        NetworkManager.enabled = true;
        ClientNetworkManager.enabled = false;
#else
        ClientNetworkManager.enabled = true;
        NetworkManager.enabled = false;
#endif
    }
}
