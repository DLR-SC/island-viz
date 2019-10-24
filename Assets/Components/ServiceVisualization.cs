using OsgiViz.Unity.MainThreadConstructors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.Core;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Unity.Island;
using OsgiViz;

public class ServiceVisualization : AdditionalIslandVizComponent
{

    public Material DefaultMaterial; // (Material)Resources.Load("Materials/Diffuse_White")

    public GameObject InterfacePrefab; // (GameObject)Resources.Load("Prefabs/ServiceInterfaceNode")
    public GameObject ReferencePrefab; // (GameObject)Resources.Load("Prefabs/ServiceReferenceNode")
    public GameObject ImplementationPrefab; // (GameObject)Resources.Load("Prefabs/ServiceImplementationNode")

    
    private GameObject ServiceSliceContainer;

    private ServiceVolume serviceVolume;





    public override void Init()
    {
        ServiceSliceContainer = new GameObject("ServiceSliceContainer");
        ServiceSliceContainer.transform.SetParent(IslandVizVisualization.Instance.VisualizationRoot, false);

        serviceVolume = new ServiceVolume();

        StartCoroutine(ConstructAll());
    }


    // Taken from ServiceGOConstructor.cs
    IEnumerator ConstructAll()
    {
        Debug.Log("Started with Service-GameObject construction!");

        Dictionary<ServiceSlice, List<Service>> serviceSliceMap = distributeServicesToSlices(IslandVizData.Instance.OsgiProject.getServices());
        foreach (KeyValuePair<ServiceSlice, List<Service>> kvp in serviceSliceMap)
        {
            yield return constructServicesAndComponents(kvp.Value, kvp.Key);
        }

        foreach (IslandGO islandGO in IslandVizVisualization.Instance.IslandGameObjects)
        {
            if (islandGO == null || islandGO.isIslandEmpty())
            {
                continue;
            }

            foreach (Region region in islandGO.getRegions())
            {
                foreach (Building building in region.getBuildings())
                {
                    if (building.GetComponent<ServiceLayerGO>() != null)
                    {
                        ServiceLayerGO serviceLayer = building.GetComponent<ServiceLayerGO>();
                        serviceLayer.createDownwardConnections();
                        foreach (ServiceNodeScript sns in serviceLayer.getServiceNodes())
                        {
                            sns.constructServiceConnections();
                        }
                    }
                }
            }                        
                        
        }
        Debug.Log("Finished with Service-GameObject construction!");
    }


    private Dictionary<ServiceSlice, List<Service>> distributeServicesToSlices(List<Service> services)
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

    private IEnumerator constructServicesAndComponents(List<Service> services, ServiceSlice serviceSlice)
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
            cuPosition.y = 0f;

            Debug.Log(cuPosition);
            Debug.Log(serviceCU.getGameObject().name);
            Debug.Log(serviceGO.name);

            serviceGO.transform.SetParent(serviceSlice.transform);
            serviceGO.transform.localScale = Vector3.one * GlobalVar.serviceNodeSize;
            serviceGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);

            ServiceNodeScript sns = serviceGO.AddComponent<ServiceNodeScript>();
            yield return null; // This is very important since otherwise the Start() method of the ServiceNodeScript is called to late!
            serviceGO.AddComponent<TextLabelComponent>();
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
                cuPosition.y = 0f;
                scGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                scGO.transform.localScale = new Vector3(GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize);
                scGO.transform.SetParent(serviceSlice.transform, false);
                ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                yield return null; // This is very important since otherwise the Start() method of the ServiceNodeScript is called to late!
                scGO.AddComponent<TextLabelComponent>();
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
                cuPosition.y = 0f;
                scGO.transform.position = cuPosition + new Vector3(0f, serviceSlice.height, 0f);
                scGO.transform.localScale = new Vector3(GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize, GlobalVar.serviceNodeSize);
                scGO.transform.SetParent(serviceSlice.transform, false);
                ServiceNodeScript scGOcomponent = scGO.AddComponent<ServiceNodeScript>();
                yield return null; // This is very important since otherwise the Start() method of the ServiceNodeScript is called to late!
                scGO.AddComponent<TextLabelComponent>();
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
