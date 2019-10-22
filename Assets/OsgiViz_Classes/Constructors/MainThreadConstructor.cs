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
        [Header("Settings")]
        public int RandomSeed;
        public Graph_Layout Graph_Layout;

        [Header("Components")]
        public AdditionalIslandVizComponent[] InputComponents; 

        [Header("Tranforms")]
        public Transform VisualizationRoot;

        
               
        private IslandGOConstructor islandGOConstructor;
        private ServiceGOConstructor serviceGOConstructor;
        private DockGOConstructor dockGOConstructor;
        private HierarchyConstructor hierarchyConstructor;
        private JsonObjConstructor jConstructor;
        private OsgiProjectConstructor osgiConstructor;
        private IslandStructureConstructor isConstructor;
        private Graph_Layout_Constructor bdConstructor;
        private Neo4jObjConstructor neo4jConstructor;

        private bool waiting = true;

        private System.Random RNG;
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

            RNG = new System.Random(RandomSeed);
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
            // Start the timer to measure total construction time.
            stopwatch.Start();

            // TODO add in future
            //yield return neo4jConstructor.Construct();

            #region Remove in future
            // Read & construct a Json Object.
            jConstructor.Construct(GlobalVar.projectmodelPath, Done);            
            // Wait for jConstructor.Construct.
            while (waiting)
                yield return null;
            #endregion

            // Construct a osgi Object from the Json Object.
            yield return osgiConstructor.Construct(jConstructor.getJsonModel()); // neo4jConstructor.GetNeo4JModel()
            Debug.Log("Project has a total of " + osgiConstructor.getProject().getNumberOfCUs() + " compilation units!");

            //Construct islands from bundles in the osgi Object.
            yield return isConstructor.Construct(osgiConstructor.getProject());

            // Construct the spatial distribution of the islands.
            if (Graph_Layout == Graph_Layout.ForceDirected)
            {
                yield return bdConstructor.ConstructFDLayout(osgiConstructor.getProject(), 0.25f, 70000, RNG);
            }
            else
            {
                Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
                Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
                yield return bdConstructor.ConstructRndLayout(osgiConstructor.getProject().getDependencyGraph(), minBounds, maxBounds, 0.075f, 10000, RNG);
            }

            GlobalVar.islandNumber = osgiConstructor.getProject().getBundles().Count;
            List<CartographicIsland> islandStructures = isConstructor.getIslandStructureList();

            // Construct the island GameObjects.
            yield return islandGOConstructor.Construct(islandStructures, VisualizationRoot.gameObject);
                        
            // Construct the connections between the islands from services in the osgi Object.
            yield return serviceGOConstructor.Construct(osgiConstructor.getProject().getServices(), islandGOConstructor.getIslandGOs());

            // Construct the dock GameObjects.
            yield return dockGOConstructor.Construct(islandGOConstructor.getIslandGOs(), VisualizationRoot.gameObject);

            // Construct the island hierarchy. TODO enable in the future
            //yield return hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs());

            yield return AutoZoom();

            // Init Input Components
            foreach (var item in InputComponents)
            {
                item.Init();
                yield return null;
            }

            stopwatch.Stop();
            Debug.Log("Construction finished after " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds!");

            //yield return AfterConstructionTasks();
        }

        /// <summary>
        /// Called after the construction of the OSGI visualization is done.
        /// </summary>
        //IEnumerator AfterConstructionTasks ()
        //{
            //yield return null;            

            // TODO Redo
            //AddHighlightToAllInteractables();
            
            // TODO wichtig?
            //BroadcastMessage("MainConstructorFinished");
        //}


        // Scales the VisualizationContainer, so all islands are visible on start. The CurrentZoomLevel is saved in the GlobalVar.
        IEnumerator AutoZoom()
        {
            Transform furthestIslandTransform = null; // Transfrom of the island which is furthest away from the center.
            float furthestDistance = 0; // Furthest distance of a island to the center.
            float distance_temp = 0;

            float maxDistance = 0.7f; // TODO move to Settings

            // Search island which is furthest away from the center.
            foreach (var islandGO in islandGOConstructor.getIslandGOs())
            {
                distance_temp = Vector3.Distance(islandGO.transform.position, Vector3.zero);
                if (furthestIslandTransform == null || distance_temp > furthestDistance)
                {
                    furthestDistance = distance_temp;
                    furthestIslandTransform = islandGO.transform; 
                }
            }

            yield return null;

            VisualizationRoot.localScale *= maxDistance / furthestDistance; // Scales the islands to make all of them fit on the table.
            GlobalVar.CurrentZoomLevel = VisualizationRoot.localScale.x; 
            GlobalVar.MinZoomLevel = VisualizationRoot.localScale.x;
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


    public enum Graph_Layout
    {
        ForceDirected,
        Random
    }

}