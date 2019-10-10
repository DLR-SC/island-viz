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
        private float translationSpeedCutoff = 0.075f;
        private float pivotTransferCutoff = 1.25f;
        private float effectivePivotTransferCutoff;
        private float effectiveTranslationSpeedCutoff;
        private float translationMult = 1f;
        private float scaleMult = 2.0f;
        private float rotationMult = 1f;

        public float drag;
        private GameObject hologramCenter;
        private Light mainLight;
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

            hologramCenter = GameObject.Find("HologramCenter");
            clippingCenterShaderID = Shader.PropertyToID("hologramCenter");
            hologramScaleShaderID = Shader.PropertyToID("hologramScale");

//            transformCandidate = GameObject.Find("RealWorld");
            transformCandidate = GameObject.Find("VisualizationContainer");            

            mainLight = GameObject.Find("MainLight").GetComponent<Light>();
            originalLightRange = mainLight.range;

            mainSliceContainer = GameObject.Find("DataManager").GetComponent<GlobalContainerHolder>().ServiceSliceContainer;
            downwardConnectionContainer = GameObject.Find("DataManager").GetComponent<GlobalContainerHolder>().DownwardConnectionContainer;
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
            if (collider.gameObject.tag == "GameController")
            {
                if (collider.gameObject.GetComponent<Hand>() != null)
                {
                    if (!touchingControllerList.Contains(collider.gameObject))
                        touchingControllerList.Add(collider.gameObject);
                }
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.tag == "GameController")
            {
                if (collider.gameObject.GetComponent<Hand>() != null)
                {
                    if (touchingControllerList.Contains(collider.gameObject))
                        touchingControllerList.Remove(collider.gameObject);
                }
            }
        }

        private void UpdateTranslation(bool useDrag)
        {
            #region translation constraint
            bool boundaryHit = false;
            //Vector3 currentDifferenceFromCenter = GlobalVar.worldCenter - hologramCenter.transform.position;
            Vector3 currentDifferenceFromCenter = GlobalVar.worldCenter - transformCandidate.transform.position;
            
            if (currentDifferenceFromCenter.magnitude > (GlobalVar.worldRadius + GlobalVar.translationCutoff))
            {
                Debug.Log("currentDifferenceFromCenter.magnitude = " + currentDifferenceFromCenter.magnitude);
                Debug.Log("(GlobalVar.worldRadius + GlobalVar.translationCutoff) = " + (GlobalVar.worldRadius + GlobalVar.translationCutoff));
                Debug.LogError("recenterForce");
                boundaryHit = true;
                Vector3 recenterForce = 100f * (1.0f / GlobalVar.inverseHologramScale) * currentDifferenceFromCenter;
                currentTranslationVelocity.x = -recenterForce.x;
                currentTranslationVelocity.z = -recenterForce.z;
            }
            #endregion

            if (useDrag)
            {
                if (currentTranslationVelocity.magnitude > effectiveTranslationSpeedCutoff || boundaryHit)
                {
                    float x = currentTranslationVelocity.x;
                    float z = currentTranslationVelocity.z;
                    currentTranslationVelocity.x = x - Mathf.Sign(x) * effectiveDrag * Time.deltaTime * x * x;
                    currentTranslationVelocity.z = z - Mathf.Sign(z) * effectiveDrag * Time.deltaTime * z * z;
                    Debug.Log(currentTranslationVelocity);
                    Debug.Log("x = " + x);
                    Debug.Log("Mathf.Sign(x) = " + Mathf.Sign(x));
                    Debug.Log("effectiveDrag = " + effectiveDrag);
                    Debug.Log("Time.deltaTime = " + Time.deltaTime);
                }
                else
                {
                    return;
                }
            }

            transformCandidate.transform.Translate(-currentTranslationVelocity * Time.deltaTime * translationMult, Space.World);
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
                Vector3 controllerVelocity = usingControllerList[0].GetTrackedObjectVelocity();
                currentTranslationVelocity = -controllerVelocity;
                currentTranslationVelocity.y = 0f;
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
                    float scalingFactor = diffNext.magnitude / diffCurrent.magnitude;
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

            //Shader.SetGlobalVector(clippingCenterShaderID, hologramCenter.transform.position);
            //Shader.SetGlobalFloat(hologramScaleShaderID, GlobalVar.inverseHologramScale * 0.8f);

        }

        public void RotateAndScale(Vector3 origin, float amountRot, float amountScale)
        {
            #region scale constraints
            if (GlobalVar.inverseHologramScale > GlobalVar.worldRadius*GlobalVar.maxZoomCutoff && amountScale > 1.0f)
                amountScale = 1.0f;
            else if (GlobalVar.inverseHologramScale < GlobalVar.worldRadius*GlobalVar.minZoomCutoff && amountScale < 1.0f)
                amountScale = 1.0f;
            #endregion

            Vector3 scaleVec = new Vector3(amountScale, amountScale, amountScale);
            Helperfunctions.scaleFromPivot(transformCandidate.transform, origin, scaleVec);
            transformCandidate.transform.RotateAround(origin, Vector3.up, -amountRot);

            #region Update due to scale change
            GlobalVar.inverseHologramScale = transformCandidate.transform.localScale.x;
            mainLight.range = originalLightRange * GlobalVar.inverseHologramScale;
            effectiveDrag = drag * 1.0f / GlobalVar.inverseHologramScale;
            effectiveTranslationSpeedCutoff = translationSpeedCutoff * GlobalVar.inverseHologramScale;
            effectivePivotTransferCutoff = pivotTransferCutoff * GlobalVar.inverseHologramScale;
            #endregion

            #region Correct Y Position of ServiceSlices
            foreach (ServiceSlice slice in serviceSlices)
            {
                Vector3 correctedPosition = slice.transform.position;
                //correctedPosition.y = Mathf.Max(slice.height, slice.height * GlobalVar.inverseHologramScale);
                correctedPosition.y = GlobalVar.hologramTableHeight + (slice.height - GlobalVar.hologramTableHeight) * GlobalVar.inverseHologramScale;
                slice.transform.position = correctedPosition;
            }
            #endregion

            #region Correct Height of downward Connections
            Vector3 oldDClocalScale = downwardConnectionContainer.transform.localScale;
            Vector3 oldDCposition = downwardConnectionContainer.transform.position;
            oldDClocalScale.y = GlobalVar.inverseHologramScale;
            oldDCposition.y = -(GlobalVar.inverseHologramScale * GlobalVar.hologramTableHeight) + GlobalVar.hologramTableHeight;
            downwardConnectionContainer.transform.localScale = oldDClocalScale;
            downwardConnectionContainer.transform.position = oldDCposition;
            #endregion
        }

        public void Reposition(Vector3 newPos)
        {
            transformCandidate.transform.position = newPos;
        }



    }
}