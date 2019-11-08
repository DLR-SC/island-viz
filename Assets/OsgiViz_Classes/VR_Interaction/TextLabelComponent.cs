using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.Core;

namespace OsgiViz
{

    [RequireComponent(typeof(Valve.VR.InteractionSystem.Interactable))]
    public class TextLabelComponent : MonoBehaviour
    {

        private LineRenderer lineRenderer;
        private GameObject textLabelPrefab;
        private GameObject textLabel;
        private Transform label;
        private Transform observer;
        private GameObject hologramCenter;
        private Vector3 scaleToKeep;
        TMPro.TMP_Text text;

        //Relative Height of the TextLabel, as seen from the bottom of the holoplane.
        public float relativeHeight = 0.5f;
        public float relativeScale = 1.0f;

        // Use this for initialization
        void Awake()
        {
            //hologramCenter = GameObject.Find("HologramCenter");
            //textLabelPrefab = (GameObject)Resources.Load("Prefabs/TextLabel");
            //GameObject observerGO = GameObject.FindGameObjectWithTag("Observer");
            //if (observerGO == null)
            //    throw new System.Exception("No GameObject has the tag 'Observer'. TextLabels from the TextLabelComponent need to face an observer.");
            //observer = observerGO.transform;

            //scaleToKeep = Vector3.one;
            //initTextLabel();
            //initTextLabelLine();
            //hideTextLabel();
        }

        private void adjustTextLabelSize(Transform label, TMPro.TMP_Text txtObj, float width, float height, float depth)
        {
            label.localScale = new Vector3(width, height, depth);
            txtObj.rectTransform.sizeDelta = new Vector2(width, height);
            txtObj.rectTransform.anchoredPosition3D = new Vector3(width * 0.5f, height * -0.5f, depth * 0.501f);

            txtObj.ForceMeshUpdate();
        }

        private void initTextLabelLine()
        {
            lineRenderer = textLabel.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 1f;
            lineRenderer.endWidth = 1f;
            lineRenderer.material = (Material)Resources.Load("Materials/LabelLine");
        }

        private void initTextLabel()
        {
            textLabel = GameObject.Instantiate(textLabelPrefab, transform);
            textLabel.tag = "TextLabel";
            text = textLabel.GetComponentInChildren<TMPro.TMP_Text>();
            textLabel.transform.position = estimateCenterForLabel();
            label = textLabel.transform.Find("Label");
            #region Override parents localScale
            
            Vector3 parentScale = transform.localScale;
            Vector3 normalizationVec = new Vector3(1.0f / parentScale.x, 1.0f / parentScale.y, 1.0f / parentScale.z);
            scaleToKeep = Vector3.Scale(textLabel.transform.localScale, normalizationVec);
            textLabel.transform.localScale = scaleToKeep;
            
            #endregion

            //Initial setup with 1m distance from observer. Used to determine the right proportions of the textLabel.
            //At runtime, the Labels transform will be scaled instead of the font, to fullfil the required
            //GlobalVar.minTextSizeInRadians & GlobalVar.maxLabelWidthInRadians 
            float maxLabelWidth       = 2f*Mathf.Tan(GlobalVar.maxLabelWidthInRadians * 0.5f);
            float labelMinFontSize = 2f*Mathf.Tan(GlobalVar.minTextSizeInRadians * 0.5f) * 10f;
            text.fontSizeMin = labelMinFontSize;
            text.fontSizeMax = labelMinFontSize;

            #region adjust relative size of Label & RectTransform
            //Initial size
            float finalLabelWidth  = labelMinFontSize * (0.1f * 1.25f);
            float finalLabelHeight = labelMinFontSize * (0.1f * 1.25f );
            adjustTextLabelSize(label, text, finalLabelWidth, finalLabelHeight, GlobalVar.labelDepth);
            text.SetText(gameObject.name);
            //Adjust Label width until maximum is reached
            while (finalLabelWidth < maxLabelWidth)
            {   
                //Fontsize is given in 1/10th of unity units.
                finalLabelWidth += labelMinFontSize * 0.1f;
                adjustTextLabelSize(label, text, finalLabelWidth, finalLabelHeight, GlobalVar.labelDepth);
                if (!text.isTextOverflowing)
                    break;
                
            }
            //Continue to further adjust the height of the label until the whole text is accomodated
            int cc = 0;
            while (text.isTextOverflowing)
            {
                finalLabelHeight += labelMinFontSize * 0.1f;
                adjustTextLabelSize(label, text, finalLabelWidth, finalLabelHeight, GlobalVar.labelDepth);
                cc++;
            }

            #endregion

        }

        private Vector3 estimateCenterForLabel()
        {
            Vector3 result = new Vector3(0, 0, 0);
            Collider col = gameObject.GetComponent<Collider>();
            if (col != null)
            {
                result = col.bounds.center;
            }
            else
            {
                result = transform.position;
            }

            return result;
        }

        public void setText(string txt)
        {
            text.SetText(txt);
        }

        private void OnHandHoverBegin(Hand hand)
        {

            textLabel.transform.position = estimateCenterForLabel();

            Vector3 temp = textLabel.transform.position;
            Hand.HandType handType = hand.GuessCurrentHandType();
            if (handType == Hand.HandType.Left)
            {
                temp += hand.transform.right * GlobalVar.CurrentZoom * GlobalVar.labelOffset;
                temp += observer.transform.up * GlobalVar.CurrentZoom * GlobalVar.labelOffset;
            }
            else if (handType == Hand.HandType.Right)
            {
                temp -= hand.transform.right * GlobalVar.CurrentZoom * GlobalVar.labelOffset;
                temp += observer.transform.up * GlobalVar.CurrentZoom * GlobalVar.labelOffset;
            }
            //If no sure which hand, use offset for Right.
            else if (handType == Hand.HandType.Any)
            {
                temp -= hand.transform.right * GlobalVar.CurrentZoom * GlobalVar.labelOffset;
                temp += observer.transform.up * GlobalVar.CurrentZoom * GlobalVar.labelOffset;
            }

            textLabel.transform.position = temp;

            adjustToObserver();
            showTextLabel();

        }

        private void OnHandHoverEnd(Hand hand)
        {
            hideTextLabel();
        }

        private void adjustToObserver()
        {
            Vector3 temp = textLabel.transform.position;
            float distanceToLabel = Vector3.Distance(temp, observer.position);
            float scalingFactor = distanceToLabel;
            textLabel.transform.localScale = new Vector3(scalingFactor * scaleToKeep.x, scalingFactor * scaleToKeep.y, scalingFactor * scaleToKeep.z);
            temp.y = gameObject.GetComponent<Collider>().bounds.max.y;

            #region orientate label
            textLabel.transform.position = temp;
            textLabel.transform.LookAt(observer.position);
            #endregion

            float lineWidth = 0.01f;
            lineRenderer.startWidth = scalingFactor * lineWidth;
            lineRenderer.endWidth = scalingFactor * lineWidth;
            Vector3[] lineVertices = { estimateCenterForLabel(), lineRenderer.transform.position };
            lineRenderer.SetPositions(lineVertices);
        }

        /*
        private void HandHoverUpdate(Hand hand)
        {
            adjustToObserver();
        }
        */
        
        void Start()
        {

        }
        /*
        void OnEnable()
        {
            showTextLabel();
        }
        */
        
        //void OnDisable()
        //{
        //    hideTextLabel();
        //}
        
         
        public void hideTextLabel()
        {
            textLabel.SetActive(false);
            lineRenderer.enabled = false;
        }

        public void showTextLabel()
        {
            textLabel.SetActive(true);
            lineRenderer.enabled = true;
        }
        
        public GameObject getTextLabel()
        {
            return textLabel;
        }
        
    }
}
