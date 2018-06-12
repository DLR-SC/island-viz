using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Valve.VR.InteractionSystem.Hand))]
public class ScreenshotFunctionality : MonoBehaviour {

    private Valve.VR.InteractionSystem.Hand hand;

	// Use this for initialization
	void Start () {
        hand = GetComponent<Valve.VR.InteractionSystem.Hand>();
	}

    void Update()
    {
        if(hand.controller != null)
            if (hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
            {
                takeScreenshot();
            } 
        /*
        if(hand.controller != null)
            if (hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_DPad_Up))
            {
                Application.Quit();
            } 
         */
    }


     void takeScreenshot()
     {
         Debug.Log("Taking Screenshot");
         string fileName = "Screenshot " + System.DateTime.Now.Second + ".png";
         ScreenCapture.CaptureScreenshot(fileName, 4);
     }
}
