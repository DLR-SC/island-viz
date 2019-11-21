using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MultiplayerPlayer : NetworkBehaviour
{
    [ClientCallback]
    void Start()
    {
        if (isLocalPlayer)
        {
            //MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            //foreach (var item in meshRenderers)
            //{
            //    Destroy(item);
            //}
        }
        else
        {
            SteamVR_TrackedObject[] meshRenderers = GetComponentsInChildren<SteamVR_TrackedObject>();
            foreach (var item in meshRenderers)
            {
                Destroy(item);
            }
        }
    }
}
