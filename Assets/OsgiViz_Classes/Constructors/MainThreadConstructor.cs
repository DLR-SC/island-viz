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


        // Use this for initialization
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

            //LayoutTester
            //lt = gameObject.AddComponent<LayoutTester>();

            StartCoroutine(Construction());                       
        }


        IEnumerator Construction ()
        {
            status = Status.Working;

            // Read & generate the Json Object 
            jConstructor.Construct(projectModelFile, Done);
            
            // Wait for jConstructor.Construct
            while (waiting)
                yield return null;

            // 
            yield return osgiConstructor.Construct(jConstructor.getJsonModel());

            Debug.Log("Project has a total of " + osgiConstructor.getProject().getNumberOfCUs() + " compilation units!");
            
            //Start IslandStructure construction
            yield return isConstructor.Construct(osgiConstructor.getProject());
            
            //Start the spatial distribution of islands    
            //Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
            //Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
            //bdConstructor.ConstructRndLayout(osgiConstructor.getProject().getDependencyGraph(), Done, minBounds, maxBounds, 0.075f, 10000);
            waiting = true;
            bdConstructor.ConstructFDLayout(osgiConstructor.getProject(), Done, 0.25f, 70000);

            // Wait for spatial distribution of islands
            while (waiting)
                yield return null;

            //Start IslandGO construction
            GlobalVar.islandNumber = osgiConstructor.getProject().getBundles().Count;
            List<CartographicIsland> islandStructures = isConstructor.getIslandStructureList();
            waiting = true;
            islandGOConstructor.Construct(islandStructures, Done);

            // Wait for islandGOConstructor.Construct
            while (waiting)
                yield return null;
                        
            OsgiProject project = osgiConstructor.getProject();
            waiting = true;
            serviceGOConstructor.Construct(project.getServices(), islandGOConstructor.getIslandGOs(), Done);

            // Wait for serviceGOConstructor.Construct
            while (waiting)
                yield return null;

            waiting = true;
            dockGOConstructor.Construct(islandGOConstructor.getIslandGOs(), Done);

            // Wait for dockGOConstructor.Construct
            while (waiting)
                yield return null;

            waiting = true;
            hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs(), Done);

            // Wait for hierarchyConstructor.Construct
            while (waiting)
                yield return null;
                        
            status = Status.Finished;
            afterConstructionTasks();

            yield return null;
        }




        private void afterConstructionTasks()
        {
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