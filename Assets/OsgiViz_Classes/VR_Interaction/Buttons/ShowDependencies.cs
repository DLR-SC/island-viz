using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz
{
    public class ShowDependencies : MonoBehaviour
    {

        private PdaPage parentPage;

        void Awake()
        {
            parentPage = gameObject.GetComponentInParent<PdaPage>();
            if (parentPage == null)
                throw new Exception("Button " + gameObject.name + " must be a child of a PdaPage!");
        }

        // Use this for initialization
        void Start()
        {
        }

        public void Click()
        {
            parentPage.getSceneController().ShowAllDependencies();
        }
    }
}
