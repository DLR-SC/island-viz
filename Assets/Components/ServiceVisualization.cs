using OsgiViz.Unity.MainThreadConstructors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.Core;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Unity.Island;
using OsgiViz;

/// <summary>
/// This component is work in progress ...
/// </summary>
public class ServiceVisualization : AdditionalIslandVizComponent
{
    public Material DefaultMaterial; // (Material)Resources.Load("Materials/Diffuse_White")

    public GameObject InterfacePrefab; // (GameObject)Resources.Load("Prefabs/ServiceInterfaceNode")
    public GameObject ReferencePrefab; // (GameObject)Resources.Load("Prefabs/ServiceReferenceNode")
    public GameObject ImplementationPrefab; // (GameObject)Resources.Load("Prefabs/ServiceImplementationNode")
    public GameObject ServiceConnection; // (GameObject)Resources.Load("Prefabs/ServiceConnection");

    private GameObject ServiceSliceContainer;
    private GameObject DownwardConnectionContainer;

    private ServiceVolume serviceVolume;





    public override IEnumerator Init()
    {
        ServiceSliceContainer = new GameObject("ServiceSliceContainer");
        ServiceSliceContainer.transform.SetParent(IslandVizVisualization.Instance.VisualizationRoot, false);

        DownwardConnectionContainer = new GameObject("DownwardConnectionContainer");
        DownwardConnectionContainer.transform.SetParent(IslandVizVisualization.Instance.VisualizationRoot, false);

        serviceVolume = new ServiceVolume();
        
        Debug.Log("Starting with Service-GameObject construction!");

        Dictionary<ServiceSlice, List<Service>> serviceSliceMap = DistributeServicesToSlices(IslandVizData.Instance.OsgiProject.getServices());
        foreach (KeyValuePair<ServiceSlice, List<Service>> kvp in serviceSliceMap)
        {
            yield return ConstructServicesAndComponents(kvp.Value, kvp.Key);
        }

        foreach (IslandGO islandGO in IslandVizVisualization.Instance.IslandGOs)
        {
            if (islandGO == null || islandGO.IsIslandEmpty())
            {
                continue;
            }

            foreach (Region region in islandGO.Regions)
            {
                foreach (Building building in region.getBuildings())
                {
                    if (building.GetComponent<ServiceLayerGO>() != null)
                    {
                        ServiceLayerGO serviceLayer = building.GetComponent<ServiceLayerGO>();
                        serviceLayer.CreateDownwardConnections(DownwardConnectionContainer, ServiceConnection);
                        foreach (ServiceNodeScript sns in serviceLayer.getServiceNodes())
                        {
                            sns.constructServiceConnections(ServiceConnection);
                        }
                    }
                }
            }                        
                        
        }
        Debug.Log("Finished with Service-GameObject construction!");
    }


    private Dictionary<ServiceSlice, List<Service>> DistributeServicesToSlices(List<Service> services)
    {
        Dictionary<ServiceSlice, List<Service>> result = new Dictionary<ServiceSlice, List<Service>>();

        int sliceNumber = services.Count / GlobalVar.groupsPerSlice;

        for (int i = 0; i < sliceNumber; i++)
        {
            GameObject serviceSlice = new GameObject("ServiceSlice");
            float height = GlobalVar.startingHeight + GlobalVar.heightStep * i;
            serviceSlice.transform.position = new Vector3(0f, height, 0f);
            serviceSlice.transform.SetParent(ServiceSliceContainer.transform, false);
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

    private IEnumerator ConstructServicesAndComponents(List<Service> services, ServiceSlice serviceSlice)
    {
        foreach (Service service in services)
        {
            CompilationUnit serviceCU = service.getServiceCU();
            if (serviceCU == null || serviceCU.getGameObject() == null)
                continue;

            GameObject serviceGO = Instantiate(InterfacePrefab, transform.position, Quaternion.identity);
            serviceGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
            serviceGO.name = service.getName();
            Vector3 cuPosition = serviceCU.getGameObject().transform.position;
            Vector3 cuScale = serviceCU.getGameObject().transform.localScale;
            serviceGO.transform.SetParent(serviceSlice.transform);
            serviceGO.transform.localScale = Vector3.one * GlobalVar.serviceNodeSize;
            serviceGO.transform.position = new Vector3(cuPosition.x, GlobalVar.hologramTableHeight + serviceSlice.height, cuPosition.z);

            ServiceNodeScript sns = serviceGO.AddComponent<ServiceNodeScript>();
            yield return null; // This is very important since otherwise the Start() method of the ServiceNodeScript is called to late!
            //serviceGO.AddComponent<TextLabelComponent>();
            ServiceLayerGO slGO = serviceCU.getGameObject().GetComponent<ServiceLayerGO>();
            slGO.addServiceNode(sns);
            
            #region construct ServiceComponents
            List<ServiceComponent> implementingComponents = service.getImplementingComponents();
            List<ServiceComponent> referencingComponents = service.getReferencingComponents();
            foreach (ServiceComponent sc in implementingComponents)
            {
                CompilationUnit componentCU = sc.getImplementationCU();
                GameObject scGO = Instantiate(ImplementationPrefab, transform.position, Quaternion.identity);
                scGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                scGO.name = sc.getName();

                cuPosition = componentCU.getGameObject().transform.position;
                scGO.transform.SetParent(serviceSlice.transform);
                scGO.transform.localScale = Vector3.one * GlobalVar.serviceNodeSize;
                scGO.transform.position = new Vector3(cuPosition.x, GlobalVar.hologramTableHeight + serviceSlice.height * 2, cuPosition.z);

                ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                yield return null; // This is very important since otherwise the Start() method of the ServiceNodeScript is called to late!
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
                GameObject scGO = Instantiate(ReferencePrefab, transform.position, Quaternion.identity);
                scGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                scGO.name = sc.getName();

                cuPosition = componentCU.getGameObject().transform.position;
                scGO.transform.SetParent(serviceSlice.transform);
                scGO.transform.localScale = Vector3.one * GlobalVar.serviceNodeSize;
                scGO.transform.position = new Vector3(cuPosition.x, GlobalVar.hologramTableHeight + serviceSlice.height, cuPosition.z);

                ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                yield return null; // This is very important since otherwise the Start() method of the ServiceNodeScript is called to late!
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
