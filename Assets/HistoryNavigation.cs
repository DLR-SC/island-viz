using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryNavigation : MonoBehaviour
{
    public static HistoryNavigation Instance { get { return instance; } }
    private static HistoryNavigation instance; // The instance of this class.

    private Project project;
    private Commit currentCommitToShow;
    private bool forwards;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();
        forwards = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StepNext();
        }
        if (Input.GetKeyDown("1"))
        {
            Debug.Log("Pressed 1");
            Commit newC = project.GetOrderedCommitList()[0];
            if (newC != null)
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

    public void StepNext()
    {
        Debug.Log("StepNextFired");
        /*if (currentCommitToShow == null)
        {
            Commit newC = project.GetOrderedCommitList()[0];
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
        }
        else if (forwards)
        {
            Commit newC = currentCommitToShow.GetNext(currentCommitToShow.GetBranch());
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
            else
            {
                newC = currentCommitToShow.GetPrevious(currentCommitToShow.GetBranch());
                if(newC != null)
                {
                    currentCommitToShow = newC;
                }
                forwards = false;
            }
        }
        else
        {
            Commit newC = currentCommitToShow.GetPrevious(currentCommitToShow.GetBranch());
            if (newC != null)
            {
                currentCommitToShow = newC;
            }
            else
            {
                newC = currentCommitToShow.GetNext(currentCommitToShow.GetBranch());
                if (newC != null)
                {
                    currentCommitToShow = newC;
                }
                forwards = true;
            }
        }*/
    }

    public void StepBack()
    {
        Debug.Log("StepBackFired");

    }

    public void TimelapsForwards()
    {
        Debug.Log("TLNextFired");

    }

    public void TimelapsBackwards()
    {
        Debug.Log("TLBackFired");

    }

    public void TimelapsStop()
    {
        Debug.Log("TLStopFired");

    }

}
