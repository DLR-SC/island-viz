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

    private void Start(){} // When this has no Start method, you will not be able to disable this in the editor.

    /// <summary>
    /// Initialize this input component. 
    /// This method is called by the IslandVizInteraction class.
    /// </summary>
    public override IEnumerator Init()
    {
        IslandVizInteraction.Instance.OnControllerButtonEvent += OnButtonDown;

        yield return null;

        initiated = true;

        //StartCoroutine(ShowToolTipp());
    }
    #endregion


    private void OnButtonDown(IslandVizInteraction.Button button, IslandVizInteraction.PressType type, Hand hand)
    {
        if (initiated && button == IslandVizInteraction.Button.Menu && type == IslandVizInteraction.PressType.PressDown)
        {
            StartCoroutine(TakeScreenshot());
        }            
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


    IEnumerator ShowToolTipp ()
    {
        while (true)
        {
            ControllerButtonHints.ShowTextHint(Player.instance.rightHand, EVRButtonId.k_EButton_Grip, "Take Screenshot", false);
            ControllerButtonHints.ShowTextHint(Player.instance.leftHand, EVRButtonId.k_EButton_Grip, "Take Screenshot", false);
            yield return new WaitForFixedUpdate();
        }
    }
}
