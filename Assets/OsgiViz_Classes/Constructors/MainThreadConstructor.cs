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
        private IslandGOConstructor islandGOConstructor;
        private ServiceGOConstructor serviceGOConstructor;
        private DockGOConstructor dockGOConstructor;
        private HierarchyConstructor hierarchyConstructor;

        public string projectModelFile = GlobalVar.projectmodelPath;
        private JsonObjConstructor jConstructor;
        private OsgiProjectConstructor osgiConstructor;
        private IslandStructureConstructor isConstructor;
        private Graph_Layout_Constructor bdConstructor;

        private Status status;
        private bool waiting = true;

        private System.Diagnostics.Stopwatch stopwatch;


        // This Method is called by Unity when the application is started.
        void Start()
        {
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1f;
            Shader.SetGlobalFloat("hologramOutlineWidth", GlobalVar.hologramOutlineWidth);
            Shader.SetGlobalVector("hologramOutlineColor", GlobalVar.hologramOutlineColor);
            islandGOConstructor = gameObject.AddComponent<IslandGOConstructor>();
            serviceGOConstructor = gameObject.AddComponent<ServiceGOConstructor>();
            dockGOConstructor = gameObject.AddComponent<DockGOConstructor>();
            hierarchyConstructor = gameObject.AddComponent<HierarchyConstructor>();

            jConstructor = new JsonObjConstructor();
            osgiConstructor = new OsgiProjectConstructor();
            isConstructor = new IslandStructureConstructor(1, 2, 8);
            bdConstructor = new Graph_Layout_Constructor();

            stopwatch = new System.Diagnostics.Stopwatch();

            //LayoutTester
            //lt = gameObject.AddComponent<LayoutTester>();

            StartCoroutine(Construction());                       
        }


        IEnumerator Construction ()
        {
            status = Status.Working;
            stopwatch.Start();

            // Read & construct a Json Object.
            jConstructor.Construct(projectModelFile, Done);            
            // Wait for jConstructor.Construct.
            while (waiting)
                yield return null;

            // Construct a osgi Object from the Json Object.
            yield return osgiConstructor.Construct(jConstructor.getJsonModel());

            //Debug.Log("Project has a total of " + osgiConstructor.getProject().getNumberOfCUs() + " compilation units!");

            //Construct islands from bundles in the osgi Object.
            yield return isConstructor.Construct(osgiConstructor.getProject());

            //Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
            //Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
            //bdConstructor.ConstructRndLayout(osgiConstructor.getProject().getDependencyGraph(), Done, minBounds, maxBounds, 0.075f, 10000);

            waiting = true;
            // Construct the spatial distribution of the islands.
            bdConstructor.ConstructFDLayout(osgiConstructor.getProject(), Done, 0.25f, 70000);
            // Wait for bdConstructor.ConstructFDLayout.
            while (waiting)
                yield return null;

            GlobalVar.islandNumber = osgiConstructor.getProject().getBundles().Count;
            List<CartographicIsland> islandStructures = isConstructor.getIslandStructureList();
            // Construct the island GameObjects.
            yield return islandGOConstructor.Construct(islandStructures);
                        
            OsgiProject project = osgiConstructor.getProject();
            // Construct the connections between the islands from services in the osgi Object.
            yield return serviceGOConstructor.Construct(project.getServices(), islandGOConstructor.getIslandGOs());

            // Construct the ports between the islands from services in the osgi Object.
            yield return dockGOConstructor.Construct(islandGOConstructor.getIslandGOs());
            
            yield return hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs());

                                    
            status = Status.Finished;
            stopwatch.Stop();
            Debug.Log("Construction finished after " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds!");

            yield return afterConstructionTasks();
        }


        IEnumerator afterConstructionTasks ()
        {
            yield return null;

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

        // Helper Functions
        public void Done ()
        {
            waiting = false;
        }

    }

}