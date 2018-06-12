using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace OsgiViz
{
    public class InteractableViaClickTouch : MonoBehaviour {

        public delegate void callback(Hand hand);

        //Register your functions, by adding them to this list, to receive a callback when a "click" is registered
        public List<callback> handleActivationDeactivation = new List<callback>(); 
        public bool simulateClickWithLongTouch = false;
        public float activationTime = 0.5f;
        public float activationCountdown = 0.5f;
        private bool active = true;


        private void click(Hand hand)
        {
            active = false;
            foreach(callback cb in handleActivationDeactivation)
                cb(hand);
        }

        private void HandHoverUpdate(Hand hand)
        {
            if (active && simulateClickWithLongTouch)
            {
                activationCountdown = activationCountdown - Time.deltaTime;
                if (activationCountdown < 0f)
                    click(hand);
            }

            if (hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
                click(hand);
        }

        private void OnHandHoverEnd(Hand hand)
        {
            active = true;
            activationCountdown = activationTime;
        }
    
    }
}
