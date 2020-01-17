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
        Commit newC = null;
        Commit oldCommit = currentCommitToShow;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StepNext();
        }
        if (Input.GetKeyDown("1"))
        {
            Debug.Log("Pressed 1");
            newC = project.GetOrderedCommitList()[0];
        }
        else if (Input.GetKeyDown("2"))
        {
            Debug.Log("Pressed 2");
            newC = project.GetOrderedCommitList()[1];
        }
        else if (Input.GetKeyDown("3"))
        {
            Debug.Log("Pressed 3");
            newC = project.GetOrderedCommitList()[2];
        }
        else if (Input.GetKeyDown("4"))
        {
            Debug.Log("Pressed 4");
            newC = project.GetOrderedCommitList()[3];
        }
        else if (Input.GetKeyDown("5"))
        {
            Debug.Log("Pressed 5");
            newC = project.GetOrderedCommitList()[4];
        }
        else if (Input.GetKeyDown("6"))
        {
            Debug.Log("Pressed 6");
            newC = project.GetOrderedCommitList()[5];
        }
        else if (Input.GetKeyDown("7"))
        {
            Debug.Log("Pressed 7");
            newC = project.GetOrderedCommitList()[6];
        }
        else if (Input.GetKeyDown("8"))
        {
            Debug.Log("Pressed 8");
            newC = project.GetOrderedCommitList()[7];
        }
        else if (Input.GetKeyDown("9"))
        {
            Debug.Log("Pressed 9");
            newC = project.GetOrderedCommitList()[8];
        }
        if (newC != null&&newC !=oldCommit)
        {
            IslandVizInteraction.Instance.OnNewCommit(oldCommit, newC);
            currentCommitToShow = newC;
        }

    }

    public Commit GetCurrentCommit()
    {
        return currentCommitToShow;
    }

    public void StepNext()
    {
        Commit oldCommit = currentCommitToShow;
        Commit newCommit = null;

        if (currentCommitToShow == null)
        {
            newCommit = project.GetOrderedCommitList()[0];
        }
        else
        {
            newCommit = currentCommitToShow.GetNext(currentCommitToShow.GetBranch());
        }
        if (newCommit != null&&newCommit!=oldCommit)
        {
            IslandVizInteraction.Instance.OnNewCommit(oldCommit, newCommit);
            currentCommitToShow = newCommit;
        }

        
    }

    public void StepBack()
    {
        Commit oldCommit = currentCommitToShow;
        Commit newCommit = null;

        if (currentCommitToShow == null)
        {
            newCommit = project.GetOrderedCommitList()[0];
        }
        else
        {
            newCommit = currentCommitToShow.GetPrevious(currentCommitToShow.GetBranch());
        }
        if (newCommit != null && newCommit != oldCommit)
        {
            IslandVizInteraction.Instance.OnNewCommit(oldCommit, newCommit);
            currentCommitToShow = newCommit;
        }

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
