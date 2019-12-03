// #########################
// This class is deprecated!
// #########################

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.Core;

namespace OsgiViz
{
    public class InverseMultiTouchController : MonoBehaviour
    {
        private GameObject transformCandidate;
        private GameObject mainSliceContainer;
        private GameObject downwardConnectionContainer;
        public List<ServiceSlice> serviceSlices;

        private List<GameObject> touchingControllerList;
        private List<Hand> usingControllerList;
        private List<Vector3> previousControllerPositions;

        private Vector3 currentTranslationVelocity = new Vector3(0f, 0f, 0f);
       // private float translationSpeedCutoff = 0.075f;
        private float translationSpeedCutoff = 0.5f;
        private float pivotTransferCutoff = 1.25f;
        private float effectivePivotTransferCutoff;
        private float effectiveTranslationSpeedCutoff;
        private float translationMult = 1f;
        private float scaleMult = 2.0f;
        private float rotationMult = 1f;

        public float drag;
        //private Light mainLight;
        private float originalLightRange;
        private int clippingCenterShaderID;
        private int hologramScaleShaderID;

        private float effectiveDrag;

        void Awake()
        {
            touchingControllerList = new List<GameObject>();
            usingControllerList = new List<Hand>();
            previousControllerPositions = new List<Vector3>();
            serviceSlices = new List<ServiceSlice>();

            clippingCenterShaderID = Shader.PropertyToID("hologramCenter");
            hologramScaleShaderID = Shader.PropertyToID("hologramScale");
            
            transformCandidate = IslandVizVisualization.Instance.VisualizationRoot.gameObject;

            mainSliceContainer = GameObject.Find("ServiceSliceContainer");
            downwardConnectionContainer = GameObject.Find("DownwardConnectionContainer");
            foreach (Transform child in mainSliceContainer.transform)
            {
                serviceSlices.Add(child.GetComponent<ServiceSlice>());
            }

            effectiveDrag = drag;

            effectiveTranslationSpeedCutoff = translationSpeedCutoff;
            effectivePivotTransferCutoff = pivotTransferCutoff;
        }
                

        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == "GameController" && collider.gameObject.GetComponent<Hand>() != null)
            {
                if (!touchingControllerList.Contains(collider.gameObject))
                    touchingControllerList.Add(collider.gameObject);
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.tag == "GameController" && collider.gameObject.GetComponent<Hand>() != null)
            {
                if (touchingControllerList.Contains(collider.gameObject))
                    touchingControllerList.Remove(collider.gameObject);
            }
        }


        // Update is called once per frame
        void Update()
        {
            //Identify which controllers are using the object from the touching list
            usingControllerList.Clear();
            if (touchingControllerList.Count > 0)
            {
                foreach (GameObject touchingController in touchingControllerList)
                {
                    Hand hand = touchingController.GetComponent<Hand>();
                    if (hand != null && hand.GetStandardInteractionButton())
                    {
                        usingControllerList.Add(hand);
                    }
                }
            }

            //Handle Movement
            if (usingControllerList.Count == 1)
            {
                currentTranslationVelocity = -usingControllerList[0].GetTrackedObjectVelocity();
                UpdateTranslation(false);
            }
            else if (usingControllerList.Count == 2)
            {
                //current pivot
                Vector3 origin1 = usingControllerList[0].gameObject.transform.GetChild(0).position;
                Vector3 origin2 = usingControllerList[1].gameObject.transform.GetChild(0).position;
                Vector3 currentPivot = (origin1 + origin2) / 2f;

                //next pivot
                Vector3 controllerVelocity1 = usingControllerList[0].GetTrackedObjectVelocity();
                Vector3 controllerVelocity2 = usingControllerList[1].GetTrackedObjectVelocity();
                Vector3 nextOrigin1 = controllerVelocity1 * Time.deltaTime + origin1;
                Vector3 nextOrigin2 = controllerVelocity2 * Time.deltaTime + origin2;
                Vector3 nextPivot = (nextOrigin1 + nextOrigin2) / 2f;

                //For an ideal scale/rotate gesture the pivot would stay the same. For real world applications
                //the pivotTransferCutoff allows for some sloppiness in the gesture
                if (Vector3.Distance(currentPivot, nextPivot) < effectivePivotTransferCutoff)
                {
                    Vector3 diffCurrent = origin1 - origin2;
                    Vector3 diffNext = nextOrigin1 - nextOrigin2;
                    float scalingFactor = diffCurrent.magnitude / diffNext.magnitude;
                    scalingFactor = 1.0f / scalingFactor;
                    Vector3 scaleRotPivot = new Vector3(currentPivot.x, GlobalVar.hologramTableHeight, currentPivot.z);

                    float radCurrent = Mathf.Atan2(diffCurrent.x, diffCurrent.z);
                    float radNext = Mathf.Atan2(diffNext.x, diffNext.z);
                    float rotationAngle = -Mathf.Rad2Deg * (radNext - radCurrent) * rotationMult;

                    RotateAndScale(scaleRotPivot, rotationAngle, scalingFactor);
                }
            }
            else
            {
                UpdateTranslation(true);
            }

            Shader.SetGlobalVector(clippingCenterShaderID, IslandVizVisualization.Instance.Table.transform.position); // TODO: nötig?
            //Shader.SetGlobalFloat(hologramScaleShaderID, GlobalVar.CurrentZoomLevel * 0.8f);
        }



        private void UpdateTranslation(bool useDrag)
        {
            #region translation constraint
            // TODO
            //bool boundaryHit = false;
            ////Vector3 currentDifferenceFromCenter = GlobalVar.worldCenter - hologramCenter.transform.position;
            //Vector3 currentDifferenceFromCenter = GlobalVar.worldCenter - transformCandidate.transform.position;

            //if (currentDifferenceFromCenter.magnitude * transformCandidate.transform.localScale.x > (GlobalVar.worldRadius + GlobalVar.translationCutoff))
            //{
            //    Debug.Log("currentDifferenceFromCenter.magnitude = " + currentDifferenceFromCenter.magnitude);
            //    Debug.Log("(GlobalVar.worldRadius + GlobalVar.translationCutoff) = " + (GlobalVar.worldRadius + GlobalVar.translationCutoff));
            //    Debug.LogError("recenterForce");
            //    boundaryHit = true;
            //    Vector3 recenterForce = 100f * (1.0f / GlobalVar.inverseHologramScale) * currentDifferenceFromCenter;
            //    currentTranslationVelocity.x = -recenterForce.x;
            //    currentTranslationVelocity.z = -recenterForce.z;
            //}
            #endregion

            if (useDrag && currentTranslationVelocity != Vector3.zero)
            {
                //currentTranslationVelocity /= 2f * (1f - (GlobalVar.CurrentZoomLevel * Time.deltaTime));

                currentTranslationVelocity -= currentTranslationVelocity * (2f - GlobalVar.CurrentZoom * 5f) * Time.deltaTime;
            }

            currentTranslationVelocity = ClampTranslationVelocityVector(currentTranslationVelocity);
            transformCandidate.transform.Translate(-currentTranslationVelocity * Time.deltaTime * translationMult, Space.World);
        }

        public void RotateAndScale(Vector3 origin, float amountRot, float amountScale)
        {
            // Scale Constraints
            if (GlobalVar.CurrentZoom * amountScale > GlobalVar.MaxZoom 
                || GlobalVar.CurrentZoom * amountScale < GlobalVar.MinZoom)
            {
                amountScale = 1.0f;
            }
            
            Vector3 scaleVec = new Vector3(amountScale, amountScale, amountScale);
            Helperfunctions.scaleFromPivot(transformCandidate.transform, origin, scaleVec);
            transformCandidate.transform.RotateAround(origin, Vector3.up, -amountRot);

            #region Update due to scale change
            GlobalVar.CurrentZoom = transformCandidate.transform.localScale.x;
            IslandVizVisualization.Instance.OnVisualizationScaleChanged();
            //mainLight.range = originalLightRange * GlobalVar.CurrentZoomLevel;
            effectiveDrag = drag * 1.0f / GlobalVar.CurrentZoom;
            effectiveTranslationSpeedCutoff = translationSpeedCutoff * GlobalVar.CurrentZoom;
            effectivePivotTransferCutoff = pivotTransferCutoff * GlobalVar.CurrentZoom;
            #endregion

            #region Correct Y Position of ServiceSlices
            foreach (ServiceSlice slice in serviceSlices)
            {
                Vector3 correctedPosition = slice.transform.position;
                //correctedPosition.y = Mathf.Max(slice.height, slice.height * GlobalVar.inverseHologramScale);
                correctedPosition.y = GlobalVar.hologramTableHeight + (slice.height - GlobalVar.hologramTableHeight) * GlobalVar.CurrentZoom;
                slice.transform.position = correctedPosition;
            }
            #endregion

            #region Correct Height of downward Connections
            Vector3 oldDClocalScale = downwardConnectionContainer.transform.localScale;
            Vector3 oldDCposition = downwardConnectionContainer.transform.position;
            oldDClocalScale.y = GlobalVar.CurrentZoom;
            oldDCposition.y = -(GlobalVar.CurrentZoom * GlobalVar.hologramTableHeight) + GlobalVar.hologramTableHeight;
            downwardConnectionContainer.transform.localScale = oldDClocalScale;
            downwardConnectionContainer.transform.position = oldDCposition;
            #endregion
        }

        public void Reposition(Vector3 newPos)
        {
            transformCandidate.transform.position = newPos;
        }

        // Tracking issues and the drag can cause the controller velocity to spike and cause problems, so we clamp the values.
        private Vector3 ClampTranslationVelocityVector (Vector3 vector)
        {
            return new Vector3(Mathf.Clamp(vector.x, -3f, 3f), 0f, Mathf.Clamp(vector.z, -3f, 3f));
        }
    }
}