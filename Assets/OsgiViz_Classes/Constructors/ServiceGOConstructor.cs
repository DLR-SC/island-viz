// #########################
// This class is deprecated! -> ServiceVisualization.cs
// #########################

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.Core;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Unity.Island;


namespace OsgiViz.Unity.MainThreadConstructors
{
    public class ServiceGOConstructor : MonoBehaviour
    {

        private Status status;
        private ServiceVolume serviceVolume;

        private Material defaultMaterial;
        private GameObject VisualizationContainer;
        private GameObject ServiceSliceContainer;

        private GameObject interfacePrefab;
        private GameObject referencePrefab;
        private GameObject implementationPrefab;

        // Use this for initialization
        void Start()
        {
            status = Status.Idle;
            defaultMaterial = (Material)Resources.Load("Materials/Diffuse_White");
            interfacePrefab = (GameObject)Resources.Load("Prefabs/ServiceInterfaceNode");
            referencePrefab = (GameObject)Resources.Load("Prefabs/ServiceReferenceNode");
            implementationPrefab = (GameObject)Resources.Load("Prefabs/ServiceImplementationNode");

            ServiceSliceContainer = GameObject.Find("ServiceSliceContainer");
            VisualizationContainer = IslandVizVisualization.Instance.IslandContainer.gameObject;
        }

        public IEnumerator Construct(List<Service> services, List<IslandGO> islandGOs)
        {
            status = Status.Working;
            serviceVolume = new ServiceVolume();
            Debug.Log("Started with Service-GameObject construction!");
            yield return StartCoroutine(constructAll(services, islandGOs));
        }


        private Dictionary<ServiceSlice, List<Service>> distributeServicesToSlices(List<Service> services)
        {
            Dictionary<ServiceSlice, List<Service>> result = new Dictionary<ServiceSlice, List<Service>>();

            int sliceNumber = services.Count / GlobalVar.groupsPerSlice;
            
            for(int i = 0; i < sliceNumber; i++)
            {
                GameObject serviceSlice = new GameObject("ServiceSlice");
                float height = GlobalVar.startingHeight + GlobalVar.heightStep * i;
                serviceSlice.transform.position = new Vector3(0f, height, 0f);
                serviceSlice.transform.SetParent(ServiceSliceContainer.transform);
                ServiceSlice sliceComponent = serviceSlice.AddComponent<ServiceSlice>();
                sliceComponent.height = height;

                List<Service> currentServiceList = new List<Service>();
                for (int s = 0; s < GlobalVar.groupsPerSlice; s++)
                {
                    int sIdx = GlobalVar.groupsPerSlice * i + s;
                    if (sIdx < services.Count)
                    {
                        currentServiceList.Add(services[sIdx]);
                    }
                }
                result.Add(sliceComponent, currentServiceList);
            }

            return result;
        }

        IEnumerator constructAll(List<Service> services, List<IslandGO> islandGOs)
        {
            Dictionary<ServiceSlice, List<Service>> serviceSliceMap = distributeServicesToSlices(services);
            foreach (KeyValuePair<ServiceSlice, List<Service>> kvp in serviceSliceMap)
            {
                constructServicesAndComponents(kvp.Value, kvp.Key);
                yield return null;
            }
            
            foreach (IslandGO islandGO in islandGOs)
            {
                if (islandGO == null)
                    continue;
                else
                {
                    if (!islandGO.IsIslandEmpty())
                    {
                        foreach (Region region in islandGO.Regions)
                            foreach (Building b in region.getBuildings())
                            {
                                ServiceLayerGO serviceLayer = b.GetComponent<ServiceLayerGO>();
                                if (serviceLayer != null)
                                {
                                    serviceLayer.CreateDownwardConnections(new GameObject(), new GameObject());

                                    List<ServiceNodeScript> snsList = serviceLayer.getServiceNodes();
                                    foreach (ServiceNodeScript sns in snsList)
                                        sns.constructServiceConnections(new GameObject());
                                }
                            }
                    }
                }
            }
            

            Debug.Log("Finished with Service-GameObject construction!");
            status = Status.Finished;
        }

        private void constructServicesAndComponents(List<Service> services, ServiceSlice serviceSlice )
        {
            foreach (Service service in services)
            {
                CompilationUnit serviceCU = service.getServiceCU();
                if (serviceCU != null && serviceCU.getGameObject() != null)
                {

                    GameObject serviceGO = Instantiate(interfacePrefab, transform.position, Quaternion.identity);
                    serviceGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                    serviceGO.name = service.getName();
                    Vector3 cuPosition = serviceCU.getGameObject().transform.position;
                    cuPosition.y = 0f;
                    serviceGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                    serviceGO.transform.localScale = new Vector3(GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize);
                    serviceGO.transform.SetParent(serviceSlice.transform);
                    ServiceNodeScript sns = serviceGO.AddComponent<ServiceNodeScript>();
                    //serviceGO.AddComponent<TextLabelComponent>();
                    ServiceLayerGO slGO = serviceCU.getGameObject().GetComponent<ServiceLayerGO>();
                    slGO.addServiceNode(sns);
                    #region construct ServiceComponents
                    List<ServiceComponent> implementingComponents = service.getImplementingComponents();
                    List<ServiceComponent> referencingComponents  = service.getReferencingComponents();
                    foreach(ServiceComponent sc in implementingComponents)
                    {
                        CompilationUnit componentCU = sc.getImplementationCU();
                        GameObject scGO = Instantiate(implementationPrefab, transform.position, Quaternion.identity);
                        scGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                        scGO.name = sc.getName();
                        cuPosition = componentCU.getGameObject().transform.position;
                        cuPosition.y = 0f;
                        scGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                        scGO.transform.localScale = new Vector3(GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize);
                        scGO.transform.SetParent(serviceSlice.transform);
                        ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                        //scGO.AddComponent<TextLabelComponent>();
                        scGOcomponent.addConnectedServiceNode(sns);
                        sns.addConnectedServiceNode(scGOcomponent);
                        scGOcomponent.disableServiceNode();
                        ServiceLayerGO sl = componentCU.getGameObject().GetComponent<ServiceLayerGO>();
                        sl.addServiceNode(scGOcomponent);
                    }
                    foreach (ServiceComponent sc in referencingComponents)
                    {
                        CompilationUnit componentCU = sc.getImplementationCU();
                        GameObject scGO = Instantiate(referencePrefab, transform.position, Quaternion.identity);
                        scGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                        scGO.name = sc.getName();
                        cuPosition = componentCU.getGameObject().transform.position;
                        cuPosition.y = 0f;
                        scGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                        scGO.transform.localScale = new Vector3(GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize);
                        scGO.transform.SetParent(serviceSlice.transform);
                        ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                        //scGO.AddComponent<TextLabelComponent>();
                        scGOcomponent.addConnectedServiceNode(sns);
                        sns.addConnectedServiceNode(scGOcomponent);

                        scGOcomponent.disableServiceNode();
                        ServiceLayerGO sl = componentCU.getGameObject().GetComponent<ServiceLayerGO>();
                        sl.addServiceNode(scGOcomponent);
                    }
                    #endregion
                    sns.disableServiceNode();
                }
            }
        }

        public Status getStatus()
        {
            return status;
        }

        public void setStatus(Status newStatus)
        {
            status = newStatus;
        }
    }
}
