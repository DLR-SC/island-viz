using IslandViz.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace IslandScrollerPlugin
{
    // public class IslandScrolling : IslandVizPlugin
    // {
    //     public static IslandScrolling Instance { private set; get; }

    //     public GameObject ScrollAreaPrefab;
    //     public Vector3 ScrollAreaPosition;
    //     public float ScrollVelocityMultiplier;
    //     public float ZoomVelocityMultiplier;

    //     //private float3 scrollVelocity;
    //     //private float zoomVelocity;

    //     private List<Hand> currentTouchingHands;
    //     private List<Hand> currentScrollingHands;

    //     private Coroutine SingleHandScrollRoutine;
    //     private Coroutine TwoHandScrollRoutine;

    //     private Hand currentSingleHandScrollingHand;
    //     private float lastHandDistance;

    //     public override IEnumerator Init()
    //     {
    //         Instantiate(ScrollAreaPrefab, ScrollAreaPosition, Quaternion.identity, transform);

    //         InputManager.Instance.OnControllerButtonEvent += OnGripDown;

    //         currentTouchingHands = new List<Hand>();
    //         currentScrollingHands = new List<Hand>();

    //         Instance = this;

    //         yield return null;
    //     }





    //     private void OnTriggerEnter(Collider other)
    //     {
    //         if (other.CompareTag("Player") && other.transform.parent.GetComponent<Hand>() != null && !currentTouchingHands.Contains(other.transform.parent.GetComponent<Hand>()))
    //         {
    //             currentTouchingHands.Add(other.transform.parent.GetComponent<Hand>());
    //         }
    //     }

    //     private void OnTriggerExit(Collider other)
    //     {
    //         if (other.CompareTag("Player") && other.transform.parent.GetComponent<Hand>() != null && currentTouchingHands.Contains(other.transform.parent.GetComponent<Hand>()))
    //         {
    //             Hand hand = other.transform.parent.GetComponent<Hand>();

    //             // Remove hand from lists.
    //             if (currentScrollingHands.Contains(hand))
    //             {
    //                 currentScrollingHands.Remove(hand);
    //             }
    //             currentTouchingHands.Remove(hand);

    //             // Stop running scrolling coroutines.
    //             if (SingleHandScrollRoutine != null && currentSingleHandScrollingHand == other.transform.parent.GetComponent<Hand>())
    //             {
    //                 StopSingleHandScrolling();
    //             }
    //             else if (TwoHandScrollRoutine != null)
    //             {
    //                 StopTwoHandScrolling();
    //             }
    //         }
    //     }




    //     private void OnGripDown(InputManager.Button button, InputManager.PressType type, Hand hand)
    //     {
    //         if (button != InputManager.Button.Trigger)
    //         {
    //             return;
    //         }

    //         // Start Scrolling/Zooming.
    //         if (type == InputManager.PressType.PressDown && currentTouchingHands.Contains(hand) && !currentScrollingHands.Contains(hand))
    //         {
    //             currentScrollingHands.Add(hand);

    //             if (currentScrollingHands.Count == 1 && SingleHandScrollRoutine == null)
    //             {
    //                 SingleHandScrollRoutine = StartCoroutine(SingleHandScrolling(hand));
    //             }
    //             else if (currentScrollingHands.Count == 2)
    //             {
    //                 if (SingleHandScrollRoutine != null)
    //                 {
    //                     StopSingleHandScrolling();
    //                 }
    //                 TwoHandScrollRoutine = StartCoroutine(TwoHandScrolling(currentScrollingHands[0], currentScrollingHands[1]));
    //             }
    //         }

    //         // Stop Scrolling/Zooming.
    //         if (type == InputManager.PressType.PressUp && currentScrollingHands.Contains(hand))
    //         {
    //             currentScrollingHands.Remove(hand);

    //             if (currentScrollingHands.Count == 0 && SingleHandScrollRoutine != null)
    //             {
    //                 StopSingleHandScrolling();
    //             }
    //             else if (currentScrollingHands.Count == 1)
    //             {
    //                 if (TwoHandScrollRoutine != null)
    //                 {
    //                     StopTwoHandScrolling();
    //                 }
    //             }
    //         }
    //     }

    //     /// <summary>
    //     /// Stops the SingleHandScrolling coroutine.
    //     /// </summary>
    //     private void StopSingleHandScrolling()
    //     {
    //         StopCoroutine(SingleHandScrollRoutine);
    //         currentSingleHandScrollingHand = null;
    //         //scrollVelocity = float3.zero;
    //         SingleHandScrollRoutine = null;
    //     }

    //     /// <summary>
    //     /// Stops the TwoHandScrolling coroutine and start the SingleHandScrolling, if there is still one hand scrolling.
    //     /// </summary>
    //     private void StopTwoHandScrolling()
    //     {
    //         StopCoroutine(TwoHandScrollRoutine);
    //         //scrollVelocity = float3.zero;
    //         TwoHandScrollRoutine = null;

    //         if (currentScrollingHands.Count == 1)
    //         {
    //             SingleHandScrollRoutine = StartCoroutine(SingleHandScrolling(currentScrollingHands[0]));
    //         }
    //     }



    //     IEnumerator SingleHandScrolling(Hand hand)
    //     {
    //         currentSingleHandScrollingHand = hand;

    //         while (currentSingleHandScrollingHand != null)
    //         {
    //             yield return new WaitForFixedUpdate();

    //             Vector3 handVelocity = currentSingleHandScrollingHand.GetTrackedObjectVelocity();
    //             VisualizationManager.IslandPositionDelta = new float3(handVelocity.x, 0f, handVelocity.z) * ScrollVelocityMultiplier;
    //         }
    //     }

    //     IEnumerator TwoHandScrolling(Hand handA, Hand handB)
    //     {
    //         lastHandDistance = -999f;

    //         while (currentScrollingHands.Count == 2)
    //         {
    //             yield return new WaitForFixedUpdate();

    //             float distance = Vector3.Distance(handA.transform.position, handB.transform.position);

    //             if (lastHandDistance == -999f)
    //             {
    //                 lastHandDistance = distance;
    //                 continue; // No last value exists -> skip
    //             }

    //             float scaleDelta = (lastHandDistance - distance) * -ZoomVelocityMultiplier;

    //             if (scaleDelta > 0 || VisualizationManager.CurrentIslandScale + scaleDelta > 0.2f)
    //             {
    //                 VisualizationManager.IslandScaleDelta = scaleDelta;
    //             }

    //             //Debug.Log("ZoomVelocity: " + ZoomVelocity);
    //         }
    //     }
    // }

}
