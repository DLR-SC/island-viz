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
        private string projectModelFile = GlobalVar.projectmodelPath;
               
        private IslandGOConstructor islandGOConstructor;
        private ServiceGOConstructor serviceGOConstructor;
        private DockGOConstructor dockGOConstructor;
        private HierarchyConstructor hierarchyConstructor;
        private JsonObjConstructor jConstructor;
        private OsgiProjectConstructor osgiConstructor;
        private IslandStructureConstructor isConstructor;
        private Graph_Layout_Constructor bdConstructor;
        private Neo4jObjConstructor neo4jConstructor;


        private Status status;
        private bool waiting = true;

        private System.Diagnostics.Stopwatch stopwatch;


        /// <summary>
        /// This Method is called when the application is started.
        /// </summary>
        void Start()
        {
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1f;
            Shader.SetGlobalFloat("hologramOutlineWidth", GlobalVar.hologramOutlineWidth);
            Shader.SetGlobalVector("hologramOutlineColor", GlobalVar.hologramOutlineColor);

            islandGOConstructor = gameObject.AddComponent<IslandGOConstructor>();
            serviceGOConstructor = gameObject.AddComponent<ServiceGOConstructor>();
            dockGOConstructor = gameObject.AddComponent<DockGOConstructor>();
            hierarchyConstructor = gameObject.AddComponent<HierarchyConstructor>();
            neo4jConstructor = gameObject.AddComponent<Neo4jObjConstructor>();

            jConstructor = new JsonObjConstructor();
            osgiConstructor = new OsgiProjectConstructor();
            isConstructor = new IslandStructureConstructor(1, 2, 8);
            bdConstructor = new Graph_Layout_Constructor();

            stopwatch = new System.Diagnostics.Stopwatch();

            //LayoutTester
            //lt = gameObject.AddComponent<LayoutTester>();

            StartCoroutine(Construction());                       
        }

        /// <summary>
        /// This Coroutine creates the OSGI visualization from a JSON file located at projectModelFile.
        /// </summary>
        IEnumerator Construction ()
        {
            status = Status.Working;
            // Start the timer to measure total construction time.
            stopwatch.Start();

            // TODO
            //yield return neo4jConstructor.Construct();

            #region Remove in future
            // Read & construct a Json Object.
            jConstructor.Construct(projectModelFile, Done);            
            // Wait for jConstructor.Construct.
            while (waiting)
                yield return null;
            #endregion

            // Construct a osgi Object from the Json Object.
            yield return osgiConstructor.Construct(jConstructor.getJsonModel()); // neo4jConstructor.GetNeo4JModel()

            Debug.Log("Project has a total of " + osgiConstructor.getProject().getNumberOfCUs() + " compilation units!");

            //Construct islands from bundles in the osgi Object.
            yield return isConstructor.Construct(osgiConstructor.getProject());

            #region alternative layout
            //Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
            //Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
            //bdConstructor.ConstructRndLayout(osgiConstructor.getProject().getDependencyGraph(), Done, minBounds, maxBounds, 0.075f, 10000);
            #endregion

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
                        
            // Construct the connections between the islands from services in the osgi Object.
            yield return serviceGOConstructor.Construct(osgiConstructor.getProject().getServices(), islandGOConstructor.getIslandGOs());

            // Construct the dock GameObjects.
            yield return dockGOConstructor.Construct(islandGOConstructor.getIslandGOs());

            // Construct the island hierarchy. TODO enable in the future
            //yield return hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs());

            
            status = Status.Finished;
            stopwatch.Stop();
            Debug.Log("Construction finished after " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds!");

            yield return AfterConstructionTasks();
        }

        /// <summary>
        /// Called after the construction of the OSGI visualization is done.
        /// </summary>
        IEnumerator AfterConstructionTasks ()
        {
            yield return null;

            InverseMultiTouchController mtController = GameObject.Find("MapNavigationArea").AddComponent<InverseMultiTouchController>();
            mtController.drag = 7.5f;

            // TODO
            // AddHighlightToAllInteractables();
            
            //BroadcastMessage("MainConstructorFinished");
        }

        /// <summary>
        /// Adds a Highlightable component to all GameObjects containing a Interactable component.
        /// </summary>
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

        // get & set
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

        // Helper function
        public void Done ()
        {
            waiting = false;
        }

    }

}