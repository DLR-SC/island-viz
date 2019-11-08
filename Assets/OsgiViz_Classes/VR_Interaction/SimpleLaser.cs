using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ValveIS = Valve.VR.InteractionSystem;
using OsgiViz.Core;

namespace OsgiViz
{
    // Use with SteamVR.Hand Component
    // Basic Idea: Upon activation, this component simply raycasts forward until something is hit,
    // then it moves the "hover-transform" of the SteamVR.Hand to this position, so the Hand component can handle the interaction.
    public class SimpleLaser : MonoBehaviour
    {

        public float maxDistance = 100f;
        public float laserThickness = 0.001f;
        public bool active = false;
        public Material laserMaterial;

        private Transform handHoverTransform;
        private Vector3 initialLocalPosHT;

        private GameObject beamObj;
        private ValveIS.Hand hand;
        private Ray ray = new Ray(Vector3.zero, Vector3.forward);
        private int layerMask;


        // Use this for initialization
        void Start()
        {
            hand = GetComponent<ValveIS.Hand>();
            layerMask =  1 << LayerMask.NameToLayer("InteractionSystemLayer");
            handHoverTransform = hand.hoverSphereTransform;
            initialLocalPosHT = handHoverTransform.localPosition;

            #region init laserbeam
            beamObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beamObj.GetComponent<MeshRenderer>().sharedMaterial = laserMaterial;
            beamObj.name = "LaserBeam";
            beamObj.transform.SetParent(hand.transform);
            beamObj.transform.localRotation = Quaternion.identity;
            beamObj.transform.localPosition = Vector3.zero;
            beamObj.transform.localScale = new Vector3(laserThickness, laserThickness, 1f);
            beamObj.SetActive(false);
            #endregion

            
        }

        private void deactivate()
        {
            active = false;
            handHoverTransform.localPosition = initialLocalPosHT;
            beamObj.SetActive(false);
        }

        private void activate()
        {
            beamObj.SetActive(true);
            active = true;
        }

        private void trace()
        {
            Vector3 startingRaycastPos = hand.transform.position;
            
            ray.origin = startingRaycastPos;
            ray.direction = hand.transform.forward;
             
            RaycastHit hit;
            bool h = Physics.Raycast(ray, out hit, maxDistance, layerMask);

            if (h == true)
            {
                Vector3 newBeamScale = beamObj.transform.localScale;
                newBeamScale.z = hit.distance / GlobalVar.CurrentZoom;
                beamObj.transform.localScale = newBeamScale;
                beamObj.transform.localPosition = Vector3.zero + Vector3.forward * 0.5f * newBeamScale.z;

                handHoverTransform.position = hit.point;
            }
            else
            {
                handHoverTransform.localPosition = initialLocalPosHT;

                Vector3 newBeamScale = beamObj.transform.localScale;
                newBeamScale.z = maxDistance;
                beamObj.transform.localScale = newBeamScale;
                beamObj.transform.localPosition = Vector3.zero + Vector3.forward*0.5f*maxDistance;
            }

        }

        // Update is called once per frame
        void Update()
        {
            
            if (hand.controller != null)
            {
                if (hand.controller.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
                {
                    activate();
                }
                else
                {
                    deactivate();
                }

                if (active)
                    trace();
            }
        }
        
    }
}
