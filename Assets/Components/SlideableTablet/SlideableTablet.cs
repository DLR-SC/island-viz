using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class SlideableTablet : AdditionalIslandVizComponent
{
    public GameObject Tablet; // The GameObject which holds the collider.

    public float Speed = 0.1f;

    public float minX;
    public float maxX;

    private RectTransform content;

    public Collider[] ButtonColliders;

    private Hand touchingHand;
    private Hand usingHand;
    private bool initiated = false;
    private bool tabletshowing = false;

    private string tabletTag = "SlideableTablet";


    private void Start()
    {
        
    }

    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Initialize this input component. 
    /// This method is called by the IslandVizInteraction class.
    /// </summary>
    public override IEnumerator Init()
    {
        IslandVizUI.Instance.UpdateLoadingScreenUI("InverseMultiTouchInput Construction", "");

        // Init GameObject       
        // TODO
        content = Tablet.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();

        // Physics Settings
        //content.tag = tabletTag;

        // Subscribe input methods
        IslandVizInteraction.Instance.OnControllerEnter += OnControllerEnterEvent;
        IslandVizInteraction.Instance.OnControllerExit += OnControllerExitEvent;
        IslandVizInteraction.Instance.OnControllerTriggerDown += OnControllerTriggerPressedEvent;
        IslandVizInteraction.Instance.OnControllerTriggerUp += OnControllerTriggerReleasedEvent;

        initiated = true;

        yield return null;
    }

    #endregion


    // ################
    // Interaction - Event Handling
    // ################

    #region Interaction - Event Handling

    private void OnControllerEnterEvent(Collider collider, Hand hand)
    {
        if (collider.tag == tabletTag && touchingHand != hand)
        {
            touchingHand = hand;
        }
    }

    private void OnControllerExitEvent(Collider collider, Hand hand)
    {
        if (collider.tag == tabletTag && touchingHand == hand)
        {
            touchingHand = null;
        }
    }

    private void OnControllerTriggerPressedEvent(Hand hand)
    {
        if (usingHand != hand && touchingHand == hand)
        {
            usingHand = hand;
            Debug.Log("added using hand");
        }
    }

    private void OnControllerTriggerReleasedEvent(Hand hand)
    {
        if (usingHand == hand)
        {
            usingHand = null;
        }
    }

    #endregion



    // ################
    // Interaction - Movement
    // ################

    #region Interaction - Movement

    void Update()
    {
        if (!initiated)
        {
            return;
        }

        //Handle Movement
        if (usingHand != null)
        {

            Debug.Log("usingHand.GetTrackedObjectVelocity(): " + usingHand.GetTrackedObjectVelocity());
            Debug.Log("content.localPosition1: " + content.localPosition);

            //// (B - A)
            //Vector3 distBetween = (usingHand.transform.position - transform.position);
            //// A's local X-Axis
            //Vector3 xAxis = transform.right;

            //// Vector projection on A's Z-Axis (assuming the Y-Axis isn't a factor
            //Vector3 xAxisDist = Vector3.Project(distBetween, xAxis);

            content.localPosition += usingHand.GetTrackedObjectVelocity() * 2;
            Debug.Log("content.localPosition2: " + content.localPosition);
            content.localPosition = new Vector3(Mathf.Clamp(content.localPosition.x, minX, maxX), 0f, 0f);
            Debug.Log("content.localPosition3: " + content.localPosition);
        }
    }


    public void ToggleTablet (Text text)
    {
        StartCoroutine(ShowHideTablet(text));
    }

    IEnumerator ShowHideTablet (Text text)
    {
        float value = 0f;

        Vector3 startPos = tabletshowing ? new Vector3(maxX, 0f, 0f) : new Vector3(minX, 0f, 0f);
        Vector3 endPos = tabletshowing ? new Vector3(minX, 0f, 0f) : new Vector3(maxX, 0f, 0f);

        while (value <= 1)
        {
            content.localPosition = Vector3.Lerp(startPos, endPos, value);
            value += Speed;
            yield return new WaitForFixedUpdate();
        }

        tabletshowing = !tabletshowing;
        text.text = tabletshowing ? "<" : ">";

        foreach (var item in ButtonColliders)
        {
            item.enabled = tabletshowing;
        }
    }

    #endregion
}
