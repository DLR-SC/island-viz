using OSGI_Datatypes.ArchitectureElements;
using OSGI_Datatypes.ComposedTypes;
using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using OsgiViz.Unity.Island;

public class IslandContainerController_Script : MonoBehaviour
{
    GameObject island;
    BundleMaster bundleMaster;

    private IslandObjectContainer_Script mainController;
    private Commit newCommit;
    private Commit currentCommit;
    private bool transformationRunning;
    private bool movingRunning;
    private float movingStartTime;

    private float speed;

    // Start is called before the first frame update
    void Start()
    {
    }
    private void Awake()
    {
        HistoryNavigation.Instance.AddIsland(this);
    }

    public IEnumerator Initialise(BundleMaster bm, int i)
    {
        mainController = GameObject.Find("IslandObjectContainer").GetComponent<IslandObjectContainer_Script>();
        currentCommit = null;
        transformationRunning = false;
        movingRunning = false;

        island = gameObject.transform.Find("IslandObject").gameObject;
        island.transform.localPosition = Vector3.zero;
        island.name = "Island_" + i;

        bundleMaster = bm;
        island.GetComponent<IslandController_Script>().SetBunldeMaster(bm);
        bm.islandController = island.GetComponent<IslandController_Script>();
        yield return island.GetComponent<IslandController_Script>().Initialise();
        island.SetActive(false);

        //Subscribe to Event
        //IslandVizInteraction.Instance.OnNewCommit += OnNewCommit;

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnNewCommit(Commit oldCommit, Commit newCommit)
    {
        if (bundleMaster.RelationOfCommitToTimeline(newCommit) != TimelineStatus.present)
        {
            //bundle not present in new Commit
            if (island.activeSelf)
            {
                //if island still active set inactive
                island.SetActive(false);
                //IslandVizVisualization.Instance.VisibleIslandGOs.Remove(island.transform.GetChild(0).GetComponent<IslandGO>());
            }
            //nothin further to do
            return;
        }

        bool justAktivated = false;
        if (!island.activeSelf && bundleMaster.RelationOfCommitToTimeline(newCommit) == TimelineStatus.present)
        {
            justAktivated = true;
            island.SetActive(true);
        }
        if (justAktivated)
        {
            Vector2 pos2D = bundleMaster.GetElement(newCommit).GetPosition();
            island.transform.localPosition = new Vector3(pos2D.x, 0f, pos2D.y);
            //IslandVizVisualization.Instance.VisibleIslandGOs.Add(island.transform.GetChild(0).GetComponent<IslandGO>());
        }
        else
        {
            movingStartTime = Time.time;
            movingRunning = true;
            //island.GetComponent<Rigidbody>().isKinematic = false;
            //StartCoroutine(MoveIsland(newCommit));
        }
        //Island Appearance Transformation
        StartCoroutine(island.GetComponent<IslandController_Script>().UpdateRoutine(newCommit, this, justAktivated, null));
    }

    public IEnumerator RenewIsland(Commit newCommit, System.Action<IslandContainerController_Script> callback)
    {
        if (bundleMaster.RelationOfCommitToTimeline(newCommit) != TimelineStatus.present)
        {
            //bundle not present in new Commit
            if (island.activeSelf)
            {
                //if island still active set inactive
                island.SetActive(false);
                //IslandVizVisualization.Instance.VisibleIslandGOs.Remove(island.transform.GetChild(0).GetComponent<IslandGO>());
            }
            //nothin further to do
            callback(this);
        }
        else
        {
            bool docksSetFlag = false;
            bool movingFinishedFlag = false;

            bool justAktivated = false;
            if (!island.activeSelf && bundleMaster.RelationOfCommitToTimeline(newCommit) == TimelineStatus.present)
            {
                justAktivated = true;
                island.SetActive(true);
            }
            if (justAktivated)
            {
                Vector2 pos2D = bundleMaster.GetElement(newCommit).GetPosition();
                island.transform.localPosition = new Vector3(pos2D.x, 0f, pos2D.y);
                movingFinishedFlag = true;
                //IslandVizVisualization.Instance.VisibleIslandGOs.Add(island.transform.GetChild(0).GetComponent<IslandGO>());
            }
            else
            {
                movingStartTime = Time.time;
                movingRunning = true;
                //island.GetComponent<Rigidbody>().isKinematic = false;
                StartCoroutine(MoveIsland(newCommit, (returnScript) => { if (returnScript != null) { movingFinishedFlag = true; } }));
            }
            //Island Appearance Transformation
            StartCoroutine(island.GetComponent<IslandController_Script>().UpdateRoutine(newCommit, this, justAktivated, (returnScript) => { if (returnScript != null) { docksSetFlag = true; } }));
            yield return null;

            while (!(docksSetFlag && movingFinishedFlag))
            {
                yield return new WaitForSeconds(0.1f);
            }
            callback(this);
        }
    }


    private IEnumerator MoveIsland_old(Commit newCommit)
    {
        Vector2 pos2D = bundleMaster.GetElement(newCommit).GetPosition();
        Vector3 target = new Vector3(pos2D.x, 0f, pos2D.y);

        Vector3 direction = target - island.transform.position;

        while(direction.magnitude >= 1.0 & Time.time - movingStartTime < 10)
        {
           /* if (direction.magnitude >= 1.0)
            {
                Vector3 oldPos = island.transform.position;
                island.transform.position = oldPos + 0.2f * direction.normalized;
            }
            else
            {
                Vector3 oldPos = island.transform.position;
                island.transform.position = oldPos + 0.1f * direction.normalized;
            }*/

           /*  try
             {
                 if (direction.magnitude >= 3.0)
                 {
                     island.GetComponent<Rigidbody>().velocity = 5*Mathf.Log(direction.magnitude)* direction.normalized;
                 }
                 else
                 {
                     island.GetComponent<Rigidbody>().velocity = direction.normalized;
                 }
             }
             catch
             {
                 Debug.Log("Exception");
             }*/
            yield return new WaitForSeconds(0.1f);
            direction = target - island.transform.position;
        }
        NotivyIslandMovementFinished();

    }

    private IEnumerator MoveIsland(Commit newCommit, System.Action<IslandContainerController_Script> callback)
    {
        yield return null;
        /*speed = HistoryNavigation.Instance.islandspeed;

        Vector2 pos2D = bundleMaster.GetElement(newCommit).GetPosition();
        Vector3 target = new Vector3(pos2D.x, 0f, pos2D.y);

        Vector3 direction = target - island.transform.localPosition;

        while (direction.magnitude >= 0.1 /*& Time.time - movingStartTime < 10*/ //)
       /* {
            Vector3 newPos = island.transform.localPosition + direction.normalized * 0.1f * speed;
            island.transform.localPosition = newPos;
            //island.transform.Translate(direction.normalized * 0.1f*speed);
            //yield return null;

            yield return new WaitForSeconds(0.1f);
            direction = target - island.transform.localPosition;
        }*/
        //NotivyIslandMovementFinished();
        callback(this);

    }


    public void NotifyIslandTransformationFinished()
    {
        transformationRunning = false;
        NotifyAllFinished();

    }

    public void NotivyIslandMovementFinished()
    {
        /*movingRunning = false;
        island.GetComponent<Rigidbody>().velocity = Vector3.zero;
        island.GetComponent<ConstantForce>().force = Vector3.zero;
        island.GetComponent<Rigidbody>().isKinematic = true;
        NotifyAllFinished();*/

    }

    private void NotifyAllFinished()
    {
        if (!transformationRunning && !movingRunning)
        {
            currentCommit = newCommit;
            newCommit = null;
        }

    }
}
