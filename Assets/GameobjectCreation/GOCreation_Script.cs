using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using OSGI_Datatypes.OrganisationElements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OSGI_Datatypes.ArchitectureElements;

public class GOCreation_Script : MonoBehaviour
{
    public static GOCreation_Script Instance { get { return instance; } }
    private static GOCreation_Script instance; // The instance of this class.

    public GameObject islandPrefab;

    Project project;
    GameObject goContainer;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //Find global project data structure
        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();
        //Find Container for IslandGameObjects
        goContainer = GameObject.Find("IslandObjectContainer");
    }


    public IEnumerator GOCreationMain()
    {
        //initialse building hight provider
        goContainer.GetComponent<BuildingProvider_Script>().Initialise(project.GetMaxLocInProject());

        int bundlesTotal = project.GetMasterBundles().Count;
        IslandVizUI.Instance.UpdateLoadingScreenUI("Creating Island GameObjects", "");

        int i = 0;

        foreach(BundleMaster bundleM in project.GetMasterBundles())
        {
            GameObject island = Instantiate(islandPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            island.name = "IslandContainer_" + i;
            island.transform.parent = goContainer.transform;

            island.transform.localPosition = new Vector3(0f, 0, 0f);


            yield return island.GetComponent<IslandContainerController_Script>().Initialise(bundleM, i);

            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating Island GameObjects", (i*100/(float)project.GetMasterBundles().Count).ToString("0.00")+"%");

            i++;

            yield return null;

        }

        Debug.Log("Finished Island GameObject Creation");
        yield return null;
    }
}
