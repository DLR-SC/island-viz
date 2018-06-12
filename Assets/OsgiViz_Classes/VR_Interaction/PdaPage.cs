using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.Core;

namespace OsgiViz
{
    public class PdaPage : MonoBehaviour
    {
        public PdaPageType pageType;
        private SceneController sceneController;

        void Awake()
        {
            if( pageType == null)
                pageType = PdaPageType.Default;

            sceneController = FindObjectOfType<SceneController>();
            if (sceneController == null)
                throw new Exception("No SceneController found. Pda page probably will not work, as it cannot access its functionality!");
        }

        void Start()
        {
        }

        public SceneController getSceneController()
        {
            return sceneController;
        }
    }
}
