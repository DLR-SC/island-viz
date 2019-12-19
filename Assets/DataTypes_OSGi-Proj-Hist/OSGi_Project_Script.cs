using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;

/// <summary>
/// Only hold project data structure in non-destroyable gameobject
/// </summary>
public class OSGi_Project_Script : MonoBehaviour
{
    private Project project;

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        project = new Project();
    }


    public Project GetProject()
    {
        return project;
    }

}
