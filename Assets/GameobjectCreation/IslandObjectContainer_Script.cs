using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandObjectContainer_Script : MonoBehaviour
{
    private Project project;
    private Commit currentCommitToShow;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();

    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown("1")&&Input.GetKeyDown("0"))
        {
            Debug.Log("Pressed 10");
            Commit newC = project.GetOrderedCommitList()[9];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("1"))
        {
            Debug.Log("Pressed 11");
            Commit newC = project.GetOrderedCommitList()[10];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("2"))
        {
            Debug.Log("Pressed 12");
            Commit newC = project.GetOrderedCommitList()[11];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("3"))
        {
            Debug.Log("Pressed 13");
            Commit newC = project.GetOrderedCommitList()[12];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("4"))
        {
            Debug.Log("Pressed 14");
            Commit newC = project.GetOrderedCommitList()[13];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("5"))
        {
            Debug.Log("Pressed 15");
            Commit newC = project.GetOrderedCommitList()[14];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("6"))
        {
            Debug.Log("Pressed 16");
            Commit newC = project.GetOrderedCommitList()[15];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("7"))
        {
            Debug.Log("Pressed 17");
            Commit newC = project.GetOrderedCommitList()[16];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("8"))
        {
            Debug.Log("Pressed 18");
            Commit newC = project.GetOrderedCommitList()[17];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("1") && Input.GetKeyDown("9"))
        {
            Debug.Log("Pressed 19");
            Commit newC = project.GetOrderedCommitList()[18];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("2") && Input.GetKeyDown("0"))
        {
            Debug.Log("Pressed 20");
            Commit newC = project.GetOrderedCommitList()[19];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("2") && Input.GetKeyDown("1"))
        {
            Debug.Log("Pressed 21");
            Commit newC = project.GetOrderedCommitList()[20];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }*/
        if (Input.GetKeyDown("1"))
        {
            Debug.Log("Pressed 1");
            Commit newC = project.GetOrderedCommitList()[0];
            if(newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("2"))
        {
            Debug.Log("Pressed 2");
            Commit newC = project.GetOrderedCommitList()[1];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("3"))
        {
            Debug.Log("Pressed 3");
            Commit newC = project.GetOrderedCommitList()[2];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("4"))
        {
            Debug.Log("Pressed 4");
            Commit newC = project.GetOrderedCommitList()[3];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("5"))
        {
            Debug.Log("Pressed 5");
            Commit newC = project.GetOrderedCommitList()[4];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("6"))
        {
            Debug.Log("Pressed 6");
            Commit newC = project.GetOrderedCommitList()[5];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("7"))
        {
            Debug.Log("Pressed 7");
            Commit newC = project.GetOrderedCommitList()[6];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("8"))
        {
            Debug.Log("Pressed 8");
            Commit newC = project.GetOrderedCommitList()[7];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (Input.GetKeyDown("9"))
        {
            Debug.Log("Pressed 9");
            Commit newC = project.GetOrderedCommitList()[8];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
       
    }

    public Commit GetCurrentCommit()
    {
        return currentCommitToShow;
    }
}
