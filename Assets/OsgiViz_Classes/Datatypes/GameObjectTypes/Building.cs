using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.SoftwareArtifact;


namespace OsgiViz.Unity.Island
{
    public class Building : MonoBehaviour
    {

        private CompilationUnit cUnit;

        void Awake()
        {
            #region clickable
            InteractableViaClickTouch ict = gameObject.GetComponent<InteractableViaClickTouch>();
            if (ict == null)
                ict = gameObject.AddComponent<InteractableViaClickTouch>();

            ict.handleActivationDeactivation.Add(handleActivationDeactivation);
            #endregion

            #region PdaInspectable
            PdaInspectable pi = gameObject.GetComponent<PdaInspectable>();
            if (pi == null)
                pi = gameObject.AddComponent<PdaInspectable>();
            #endregion
        }

        void Start()
        {   
        }

        private void handleActivationDeactivation(Hand hand)
        {
            string contentText = "";
            contentText += "<b><color=green>Name</b></color>";
            contentText += "\n";
            contentText += cUnit.getName();
            contentText += "\n";
            contentText += "<b><color=green>#LOC</b></color>";
            contentText += "\n";
            contentText += cUnit.getLoc();


            gameObject.GetComponent<PdaInspectable>().sendContentToPda(contentText);
        }

        public CompilationUnit getCU()
        {
            return cUnit;
        }

        public void setCU(CompilationUnit cu)
        {
            cUnit = cu;
        }



    }
}
