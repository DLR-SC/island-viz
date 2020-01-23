using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MultiplayerPlayer : NetworkBehaviour
{
    public Transform Head;
    public Transform HandLeft;
    public Transform HandRight;

    private Vector3 Hand1Position = Vector3.zero;
    private Vector3 Hand2Position = Vector3.zero;
    private Quaternion Hand1Rotation = Quaternion.identity;
    private Quaternion Hand2Rotation = Quaternion.identity;



    [ClientCallback]
    void Start()
    {
        if (isLocalPlayer)
        {
            Debug.Log("LOCAL PLAYER CONNECTED!");
        }
        else
        {
            Debug.Log("NOT A LOCAL PLAYER CONNECTED!");
        }
    }

    [ClientCallback]
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            if (Valve.VR.InteractionSystem.Player.instance.leftHand != null)
            {
                Hand1Position = Valve.VR.InteractionSystem.Player.instance.leftHand.transform.position;
                Hand1Rotation = Valve.VR.InteractionSystem.Player.instance.leftHand.transform.rotation;
            }
            if (Valve.VR.InteractionSystem.Player.instance.rightHand != null)
            {
                Hand2Position = Valve.VR.InteractionSystem.Player.instance.rightHand.transform.position;
                Hand2Rotation = Valve.VR.InteractionSystem.Player.instance.rightHand.transform.rotation;
            }

            CmdUpdatePosition(Camera.main.transform.position, Camera.main.transform.rotation, Hand1Position, Hand1Rotation, Hand2Position, Hand2Rotation);
        }
    }

    [Command]
    public void CmdUpdatePosition (Vector3 HeadPos, Quaternion HeadRota, Vector3 Hand1Pos, Quaternion Hand1Rota, Vector3 Hand2Pos, Quaternion Hand2Rota)
    {
        RpcUpdatePosition(HeadPos, HeadRota, Hand1Pos, Hand1Rota, Hand2Pos, Hand2Rota);
    }

    [ClientRpc]
    public void RpcUpdatePosition(Vector3 HeadPos, Quaternion HeadRota, Vector3 Hand1Pos, Quaternion Hand1Rota, Vector3 Hand2Pos, Quaternion Hand2Rota)
    {
        Head.GetComponent<Rigidbody>().rotation = HeadRota;
        Head.GetComponent<Rigidbody>().position = HeadPos;
        HandLeft.GetComponent<Rigidbody>().rotation = Hand1Rota;
        HandLeft.GetComponent<Rigidbody>().position = Hand1Pos;
        HandRight.GetComponent<Rigidbody>().rotation = Hand2Rota;
        HandRight.GetComponent<Rigidbody>().position = Hand2Pos;
    }
}
