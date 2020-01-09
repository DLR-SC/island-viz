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
    //Fields of canvas for user information
    //[SerializeField]
    //private Text taskTextfield;
    //[SerializeField]
    //private Text statusTextfield;
    //[SerializeField]
    //private Text loadingDotsTextfield;

    public GameObject islandPrefab;



    Project project;
    GameObject goContainer;

    // Start is called before the first frame update
    void Start()
    {
        //At this point no database conection is needed anymore
        //DisposeDatabase will also destroy gameobject;
        GameObject database = GameObject.Find("DatabaseObject");
        database.GetComponent<DatabaseAccess>().DisposeDatabase();

        

        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();
        goContainer = GameObject.Find("IslandObjectContainer");

        //initialse building hight provider
        goContainer.GetComponent<BuildingProvider_Script>().Initialise(project.GetMaxLocInProject());

        StartCoroutine(GOCreationMain());
    }

    // Update is called once per frame
    void Update()
    {
       // loadingDotsTextfield.color = new Color(loadingDotsTextfield.color.r, loadingDotsTextfield.color.g, loadingDotsTextfield.color.b, Mathf.PingPong(Time.time, 1));
    }

    private IEnumerator GOCreationMain()
    {
        int bundlesTotal = project.GetMasterBundles().Count;
       // taskTextfield.text = "Creating Gameobjects for " + bundlesTotal + " Islands";
       // statusTextfield.text = "Waiting for Islands to be completed";
        int i = 0;

        foreach(BundleMaster bundleM in project.GetMasterBundles())
        {
            GameObject island = Instantiate(islandPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            island.name = "IslandContainer_" + i;
            island.transform.parent = goContainer.transform;

            island.transform.localPosition = new Vector3(0f, 0, 0f);


            yield return island.GetComponent<IslandContainerController_Script>().Initialise(bundleM, i);
            //yield return island.GetComponent<IslandController_Script>().Initialise();

          //  statusTextfield.text = "Finished " + (i+1) + " islands of " + bundlesTotal;

            i++;

            yield return null;

        }

        Debug.Log("All finished");
        yield return null;
        SceneManager.LoadScene(5);
    }
}
