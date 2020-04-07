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
        
        void Start() { }


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
