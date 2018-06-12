using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace OsgiViz
{
    public class MultiTouchController : MonoBehaviour
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
        private float translationMult = 1f;
        private float scaleMult = 2.0f;
        private float rotationMult = 1f;

        public float drag;

        void Awake()
        {
            touchingControllerList = new List<GameObject>();
            usingControllerList = new List<Hand>();
            previousControllerPositions = new List<Vector3>();
            serviceSlices = new List<ServiceSlice>();
        }

        // Use this for initialization
        void Start()
        {
            transformCandidate = GameObject.Find("DataManager").GetComponent<GlobalContainerHolder>().VisualizationContainer;
            
            mainSliceContainer = GameObject.Find("DataManager").GetComponent<GlobalContainerHolder>().ServiceSliceContainer;
            downwardConnectionContainer = GameObject.Find("DataManager").GetComponent<GlobalContainerHolder>().DownwardConnectionContainer;
            foreach (Transform child in mainSliceContainer.transform)
            {
                serviceSlices.Add(child.GetComponent<ServiceSlice>());
            }

            if (drag == null)
                drag = 5f;
        }



        void OnTriggerEnter(Collider collider)
        {
            //Must be the Hand/Controller
            if (collider.gameObject.GetComponent<Hand>() != null)
            {
                if (!touchingControllerList.Contains(collider.gameObject))
                    touchingControllerList.Add(collider.gameObject);
            }
        }

        void OnTriggerExit(Collider collider)
        {
            //Must be the Hand/Controller
            if (collider.gameObject.GetComponent<Hand>() != null)
            {
                if (touchingControllerList.Contains(collider.gameObject))
                    touchingControllerList.Remove(collider.gameObject);
            }
        }

        private void updateTranslation(bool useDrag)
        {
            if (useDrag)
            {
                if (currentTranslationVelocity.magnitude > translationSpeedCutoff)
                {
                    float x = currentTranslationVelocity.x;
                    float z = currentTranslationVelocity.z;
                    currentTranslationVelocity.x = x - Mathf.Sign(x) * drag * Time.deltaTime * x * x;
                    currentTranslationVelocity.z = z - Mathf.Sign(z) * drag * Time.deltaTime * z * z;
                }
                else
                {
                    return;
                }
            }

            transformCandidate.transform.Translate(currentTranslationVelocity * Time.deltaTime * translationMult, Space.World);
            mainSliceContainer.transform.Translate(currentTranslationVelocity * Time.deltaTime * translationMult, Space.World);
            downwardConnectionContainer.transform.Translate(currentTranslationVelocity * Time.deltaTime * translationMult, Space.World);

        }

        // Update is called once per frame
        void Update()
        {

            //Identify which controllers are using the object from the touching list
            usingControllerList.Clear();
            if (touchingControllerList.Count > 0)
            {
                foreach (GameObject go in touchingControllerList)
                {
                    Hand h = go.GetComponent<Hand>();
                    if (h != null)
                        if (h.GetStandardInteractionButton())
                        {
                            usingControllerList.Add(h);
                        }
                }
            }

            //Handle Movement
            if (usingControllerList.Count == 1)
            {
                Vector3 controllerVelocity = usingControllerList[0].GetTrackedObjectVelocity();
                currentTranslationVelocity = controllerVelocity;
                currentTranslationVelocity.y = 0f;
                updateTranslation(false);
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
                if (Vector3.Distance(currentPivot, nextPivot) < pivotTransferCutoff)
                {
                    //Scale
                    Vector3 diffCurrent = origin1 - origin2;
                    Vector3 diffNext = nextOrigin1 - nextOrigin2;
                    float scalingFactor = diffNext.magnitude / diffCurrent.magnitude;

                    //scalePivot = currentPivot except, scalingPivot should not vary with the y component
                    Vector3 scalePivot = new Vector3(currentPivot.x, transformCandidate.transform.position.y, currentPivot.z);
                    Vector3 scaleVec = new Vector3(scalingFactor, scalingFactor, scalingFactor);


                    Helperfunctions.scaleFromPivot(transformCandidate.transform, scalePivot, scaleVec);
                    Helperfunctions.scaleFromPivot(mainSliceContainer.transform, scalePivot, scaleVec);
                    scaleVec.y = 1f;
                    Helperfunctions.scaleFromPivot(downwardConnectionContainer.transform, scalePivot, scaleVec);

                    
                    //Rotation
                    //float norm = (Mathf.PI / 4.0f);
                    //float radCurrent = Helperfunctions.DiamondAngle(diffCurrent.x, diffCurrent.z) * norm;
                    //float radNext = Helperfunctions.DiamondAngle(diffNext.x, diffNext.z) * norm;
                    float radCurrent = Mathf.Atan2(diffCurrent.x, diffCurrent.z);
                    float radNext = Mathf.Atan2(diffNext.x, diffNext.z);
                    float rotationAngle = Mathf.Rad2Deg * (radNext - radCurrent);
                    transformCandidate.transform.RotateAround(currentPivot, Vector3.up, rotationAngle * rotationMult);
                    mainSliceContainer.transform.RotateAround(currentPivot, Vector3.up, rotationAngle * rotationMult);
                    downwardConnectionContainer.transform.RotateAround(currentPivot, Vector3.up, rotationAngle * rotationMult);
                    
                     
                    //Correct Y Position of ServiceSlices
                    foreach (ServiceSlice slice in serviceSlices)
                    {
                        Vector3 correctedPosition = slice.transform.position;
                        correctedPosition.y = slice.height;
                        slice.transform.position = correctedPosition;
                    }
                }

            }
            else
            {
                updateTranslation(true);
            }

        }


        

    }
}