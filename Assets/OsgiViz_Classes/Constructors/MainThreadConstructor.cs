using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.SideThreadConstructors;
using OsgiViz.Core;
using OsgiViz.Island;

namespace OsgiViz.Unity.MainThreadConstructors
{

    public class MainThreadConstructor : MonoBehaviour
    {


        private SideThreadConstructor sideConstructor;
        private IslandGOConstructor islandGOConstructor;
        private ServiceGOConstructor serviceGOConstructor;
        private DockGOConstructor dockGOConstructor;
        private HierarchyConstructor hierarchyConstructor;

        private Status status;

        // Use this for initialization
        void Start()
        {
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1f;
            Shader.SetGlobalFloat("hologramOutlineWidth", GlobalVar.hologramOutlineWidth);
            Shader.SetGlobalVector("hologramOutlineColor", GlobalVar.hologramOutlineColor);
            //SideConstructor runs advanceConstruction() after finished
            sideConstructor = new SideThreadConstructor();
            islandGOConstructor = gameObject.AddComponent<IslandGOConstructor>();
            serviceGOConstructor = gameObject.AddComponent<ServiceGOConstructor>();
            dockGOConstructor = gameObject.AddComponent<DockGOConstructor>();
            hierarchyConstructor = gameObject.AddComponent<HierarchyConstructor>();

            status = Status.Working;
            //LayoutTester
            //lt = gameObject.AddComponent<LayoutTester>();
        }

        // Update is called once per frame
        void Update()
        {
            //Wait till SideThreadConstructor is finished
            if (sideConstructor.getStatus() == Status.Finished && status == Status.Working)
            {
                advanceConstruction();
            }

        }

        private void advanceConstruction()
        {
            //Start IslandGO construction
            if (sideConstructor.getStatus() == Status.Finished)
            {
                GlobalVar.islandNumber = sideConstructor.getOsgiProjectConstructor().getProject().getBundles().Count;

                sideConstructor.setStatus(Status.Idle);
                IslandStructureConstructor isConstructor = sideConstructor.getIslandStructureConstructor();
                List<CartographicIsland> islandStructures = isConstructor.getIslandStructureList();
                islandGOConstructor.Construct(islandStructures, advanceConstruction);
            }
            /*
            if (islandGOConstructor.getStatus() == Status.Finished)
            {
                islandGOConstructor.setStatus(Status.Idle);
                //LayoutTester
                OsgiProject project = sideConstructor.getOsgiProjectConstructor().getProject();
                lt.enableLayoutComputation(project.getDependencyGraph());
            }
            */
            
            if (islandGOConstructor.getStatus() == Status.Finished)
            {
                islandGOConstructor.setStatus(Status.Idle);
                OsgiProject project = sideConstructor.getOsgiProjectConstructor().getProject();
                serviceGOConstructor.Construct(project.getServices(), islandGOConstructor.getIslandGOs(), advanceConstruction);
            }
            if(serviceGOConstructor.getStatus() == Status.Finished)
            {
                serviceGOConstructor.setStatus(Status.Idle);
                
                dockGOConstructor.Construct(islandGOConstructor.getIslandGOs(), advanceConstruction);
            }
            if (dockGOConstructor.getStatus() == Status.Finished)
            {
                dockGOConstructor.setStatus(Status.Idle);
                hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs(), advanceConstruction);
            }
            
            if(hierarchyConstructor.getStatus() == Status.Finished)
            {

                hierarchyConstructor.setStatus(Status.Idle);
                status = Status.Finished;
                afterConstructionTasks();
            }
            

        }

        private void afterConstructionTasks()
        {
            /*
            GazeTrigger[] gazeTriggers = Resources.FindObjectsOfTypeAll<GazeTrigger>();
            foreach (GazeTrigger gt in gazeTriggers)
            {
                gt.gameObject.SetActive(true);
                gt.allocateOverlapArray(GlobalVar.islandNumber);
            }
            */
            //MultiTouchController mtController = GameObject.Find("MapNavigationArea").AddComponent<MultiTouchController>();
            InverseMultiTouchController mtController = GameObject.Find("MapNavigationArea").AddComponent<InverseMultiTouchController>();
            mtController.drag = 7.5f;

            AddHighlightToAllInteractables();

            BroadcastMessage("MainConstructorFinished");
        }

        private void AddHighlightToAllInteractables()
        {
            Valve.VR.InteractionSystem.Interactable[] interactablesComponents = GameObject.FindObjectsOfType<Valve.VR.InteractionSystem.Interactable>();
            foreach (Valve.VR.InteractionSystem.Interactable component in interactablesComponents)
            {
                Valve.VR.InteractionSystem.Interactable[] childrenComponents = component.GetComponentsInChildren<Valve.VR.InteractionSystem.Interactable>(true);
                foreach (Valve.VR.InteractionSystem.Interactable childComponent in childrenComponents)
                {
                    if (childComponent.gameObject.GetComponent<MeshFilter>() != null)
                        childComponent.gameObject.AddComponent<Highlightable>();
                }


            }
        }

        public Status getStatus()
        {
            return status;
        }
        public IslandGOConstructor getIslandGOConstructor()
        {
            return islandGOConstructor;
        }
        public DockGOConstructor getDockConstructor()
        {
            return dockGOConstructor;
        }

    }

}