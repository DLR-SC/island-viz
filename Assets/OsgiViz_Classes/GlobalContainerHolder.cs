using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsgiViz
{
    public class GlobalContainerHolder : MonoBehaviour
    {

        public GameObject VisualizationContainer;
        public GameObject ServiceSliceContainer;
        public GameObject DownwardConnectionContainer;
        public GameObject DependencyContainer;
        public GameObject RealWorldContainer;

        void Awake()
        {
            if(VisualizationContainer == null)
                throw new Exception("Visualization Container is missing or not properly linked to the GlobalContainerHolder!");
            if (ServiceSliceContainer == null)
                throw new Exception("ServiceSlice Container is missing or not properly linked to the GlobalContainerHolder!");
            if (DownwardConnectionContainer == null)
                throw new Exception("DownwardConnection Container is missing or not properly linked to the GlobalContainerHolder!");
            if (DependencyContainer == null)
                throw new Exception("Dependency Container is missing or not properly linked to the GlobalContainerHolder!");
            if (RealWorldContainer == null)
                throw new Exception("RealWorld Container is missing or not properly linked to the GlobalContainerHolder!");

            VisualizationContainer.SetActive(true);
            ServiceSliceContainer.SetActive(true);
            DownwardConnectionContainer.SetActive(true);
            DependencyContainer.SetActive(true);
            RealWorldContainer.SetActive(true);

        }

    }
}
