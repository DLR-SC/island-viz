using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;


public class ScreenshotFunctionality : AdditionalIslandVizComponent
{

    private bool initiated = false;


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
        IslandVizInteraction.Instance.OnControllerGripDown += OnButtonDown;

        yield return null;

        initiated = true;

        //ControllerButtonHints.ShowTextHint(Player.instance.rightHand, EVRButtonId.k_EButton_Grip, "Take Screenshot");
    }
    #endregion


    private void OnButtonDown(Hand hand)
    {
        if (initiated)
            StartCoroutine(TakeScreenshot());
    }

     IEnumerator TakeScreenshot()
     {
        IslandVizUI.Instance.MakeNotification(0.8f, "Taking a screenshot in 3 ...");
        yield return new WaitForSeconds(1f);
        IslandVizUI.Instance.MakeNotification(0.8f, "Taking a screenshot in 2 ...");
        yield return new WaitForSeconds(1f);
        IslandVizUI.Instance.MakeNotification(0.8f, "Taking a screenshot in 1 ...");
        yield return new WaitForSeconds(1f);

        string fileName = "Screenshot " + System.DateTime.Now.Second + ".png";
        ScreenCapture.CaptureScreenshot(fileName, 4);
     }
}
