using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MultiplayerPlayer : NetworkBehaviour
{
    public Transform Head;
    public Transform HandLeft;
    public Transform HandRight;

    public GameObject AdditionalComponentsContainer;

    private Vector3 Hand1Position = Vector3.zero;
    private Vector3 Hand2Position = Vector3.zero;
    private Quaternion Hand1Rotation = Quaternion.identity;
    private Quaternion Hand2Rotation = Quaternion.identity;

    private Valve.VR.InteractionSystem.Hand[] Hands; // Note: The Hand components must be disabled!

    private AdditionalIslandVizComponent[] inputComponents; // Array of all additional input componets.



    [ClientCallback]
    void Start()
    {
        Hands = new Valve.VR.InteractionSystem.Hand[] { HandLeft.GetComponent<Valve.VR.InteractionSystem.Hand>(), HandRight.GetComponent<Valve.VR.InteractionSystem.Hand>() };

        if (isLocalPlayer)
        {
            Debug.Log("LOCAL PLAYER CONNECTED!");

            IslandVizInteraction.Instance.OnControllerButtonEvent += OnPlayerInput;

            Destroy(Hands[0]);
            Destroy(Hands[1]);

            Destroy(Head.GetChild(0).gameObject);
            Destroy(HandLeft.GetChild(0).gameObject);
            Destroy(HandRight.GetChild(0).gameObject);

            Destroy(AdditionalComponentsContainer);
        }
        else
        {
            Debug.Log("NOT A LOCAL PLAYER CONNECTED!");

            inputComponents = AdditionalComponentsContainer.GetComponents<AdditionalIslandVizComponent>();
            if (inputComponents.Length > 0)
                StartCoroutine(InitComponents());
        }
    }


    /// <summary>
    /// Initialize all input components. Called by IslandVizBehavior.
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitComponents()
    {
        foreach (var item in inputComponents)
        {
            if (item.enabled)
                yield return item.Init();
        }
    }



    // ################
    // Head & Controller Movement
    // ################

    #region Head & Controller Movement

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

    #endregion



    // ################
    // Events
    // ################

    #region Events

        // This is currently designed for two players!

    [ClientCallback]
    public void OnPlayerInput (IslandVizInteraction.Button button, IslandVizInteraction.PressType type, Valve.VR.InteractionSystem.Hand hand)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (hand == IslandVizInteraction.Instance.Player.leftHand)
        {
            CmdControllerButtonEvent(button, type, 0);
        }
        else if (hand == IslandVizInteraction.Instance.Player.rightHand)
        {
            CmdControllerButtonEvent(button, type, 1);
        }
    }

    [Command]
    public void CmdControllerButtonEvent(IslandVizInteraction.Button button, IslandVizInteraction.PressType type, int handID)
    {
        RpcControllerButtonEvent(button, type, handID);
    }

    [ClientRpc]
    public void RpcControllerButtonEvent(IslandVizInteraction.Button button, IslandVizInteraction.PressType type, int handID)
    {
        //Debug.LogError("RpcControllerButtonEvent ");

        if (!isLocalPlayer && Hands[handID] != IslandVizInteraction.Instance.Player.leftHand && Hands[handID] != IslandVizInteraction.Instance.Player.rightHand)
        {
            IslandVizInteraction.Instance.OnControllerButtonEvent(IslandVizInteraction.Button.Touchpad, type, Hands[handID]);
            Debug.LogError("Other player pressed " + button);
        }
    }

    #endregion

}
