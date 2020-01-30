using OSGI_Datatypes.OrganisationElements;
using OsgiViz;
using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryNavigation : MonoBehaviour
{
    private enum TimeLapsStatus
    {
        forwards, backwards, stop
    }

    public static HistoryNavigation Instance { get { return instance; } }
    private static HistoryNavigation instance; // The instance of this class.

    public float islandspeed;
    public float timelapsInterval = 3;
    public bool showTimeDependentHight;
    public bool showChangeSymbols;
    public bool historyHighlightActive { get; set; }

    private Project project;
    private Commit currentCommitToShow;
    private TimeLapsStatus timeLapsStatus;

    private List<IslandContainerController_Script> islands;
    private List<DependencyDock> docks;

    private void Awake()
    {
        islands = new List<IslandContainerController_Script>();
        docks = new List<DependencyDock>();
    }

    #region Instantiation Methods
    void Start()
    {
        instance = this;
        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();
        timeLapsStatus = TimeLapsStatus.stop;

        historyHighlightActive = showChangeSymbols;
    }

    public void AddDock(DependencyDock d)
    {
        docks.Add(d);
    }

    public void AddIsland(IslandContainerController_Script i)
    {
        islands.Add(i);
    }

    #endregion

    /// <summary>
    /// History Navigation Via KeyBoard Entry:
    /// Left-/Right-Arrow = Step Back, Step Next
    /// Numbers 1-9 Call show Commit add index-1 in commitList if available
    /// </summary>
    void Update()
    {
        Commit newC = null;
        Commit oldCommit = currentCommitToShow;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StepNext();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StepBack();
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
            StartCoroutine(CallCommitEvent(oldCommit, newC, true));
        }

    }

    public Commit GetCurrentCommit()
    {
        return currentCommitToShow;
    }

    public IEnumerator CallCommitEvent(Commit oldCommit, Commit newCommit, bool createDependencies)
    {
        //Generals
        IslandVizInteraction.Instance.OnClearVisForNextCommit();
        CommitTransformInfo(currentCommitToShow, newCommit);
        currentCommitToShow = newCommit;
        GlobalVar.islandNumber = newCommit.GetBundleCount();

        //Notify IslandContainers to change Islands
        int countFinishedIslands = 0;
        foreach(IslandContainerController_Script islandC in islands)
        {
            StartCoroutine(islandC.RenewIsland(newCommit, (returnScript) => { if (returnScript != null) { countFinishedIslands++; } }));
        }
        //Wait for all IslandContainers to finish 
        while(countFinishedIslands < islands.Count)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (createDependencies)
        {
            yield return CreateDependenciesRoutine();
        }
        yield return null;
    }

    public IEnumerator CreateDependenciesRoutine()
    {
        Transform rootTransform = IslandVizVisualization.Instance.VisualizationRoot;
        Vector3 scaleTemp = rootTransform.localScale;
        rootTransform.localScale = Vector3.one;

        int countFinishedDocks = 0;
        foreach (DependencyDock dock in docks)
        {
            StartCoroutine(dock.ConnectionArrowConstructionRoutine((returnScript) => { if (returnScript != null) { countFinishedDocks++; } }));
        }
        while (countFinishedDocks < docks.Count)
        {
            yield return new WaitForSeconds(0.1f);
        }
        rootTransform.localScale = scaleTemp;
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
            //CallCommitEvent(currentCommitToShow, newCommit);
            StartCoroutine(CallCommitEvent(currentCommitToShow, newCommit, true));
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
            //CallCommitEvent(currentCommitToShow, newCommit);
            StartCoroutine(CallCommitEvent(currentCommitToShow, newCommit, true));

        }

    }

    public void TimelapsForwards()
    {
        Debug.Log("TLNextFired");
        if (timeLapsStatus.Equals(TimeLapsStatus.stop))
        {
            timeLapsStatus = TimeLapsStatus.forwards;
            StartCoroutine(TimelapsForwardsRoutine());
        }
    }

    public void TimelapsBackwards()
    {
        Debug.Log("TLBackFired");
        if (timeLapsStatus.Equals(TimeLapsStatus.stop))
        {
            timeLapsStatus = TimeLapsStatus.backwards;
            StartCoroutine(TimelapsBackwardsRoutine());
        }
    }

    public void TimelapsStop()
    {
        Debug.Log("TLStopFired");
        if (!timeLapsStatus.Equals(TimeLapsStatus.stop))
        {
            timeLapsStatus = TimeLapsStatus.stop;
        }
    }

    public IEnumerator TimelapsForwardsRoutine()
    {
        while(timeLapsStatus.Equals(TimeLapsStatus.forwards)&& currentCommitToShow.GetNext(currentCommitToShow.GetBranch()) != null)
        {
            StepNext();
            yield return new WaitForSeconds(timelapsInterval);
        }
        if(currentCommitToShow.GetNext(currentCommitToShow.GetBranch()) == null)
        {
            timeLapsStatus = TimeLapsStatus.stop;
        }
    }

    public IEnumerator TimelapsBackwardsRoutine()
    {
        while (timeLapsStatus.Equals(TimeLapsStatus.backwards) && currentCommitToShow.GetPrevious(currentCommitToShow.GetBranch()) != null)
        {
            StepBack();
            yield return new WaitForSeconds(timelapsInterval);
        }
        if (currentCommitToShow.GetPrevious(currentCommitToShow.GetBranch()) == null)
        {
            timeLapsStatus = TimeLapsStatus.stop;
        }
    }

    public void CommitTransformInfo(Commit oldCommit, Commit newCommit)
    {
        string c1 = "";
        if(oldCommit != null)
        {
            c1 = oldCommit.GetString();
        }
        string c2 = newCommit.GetString();

        string Message = "timestep from \n" + c1 + "\n to \n" + c2;
        IslandVizUI.Instance.MakeNotification(2.5f, Message);
    }

    public void ToggleShowChangeHighlight()
    {
        if(!showChangeSymbols && !historyHighlightActive)
        {
            IslandVizUI.Instance.MakeNotification(2f, "History Highlight not available");
            return;
        }

        if (historyHighlightActive)
        {
            historyHighlightActive = false;
        }
        else
        {
            historyHighlightActive = true;
        }

        IslandVizInteraction.Instance.OnHistoryHighlightChanged(historyHighlightActive);

    }

}
