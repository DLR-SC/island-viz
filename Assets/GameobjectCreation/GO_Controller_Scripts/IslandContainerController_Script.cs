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
    public GameObject island { get; set; }
    BundleMaster bundleMaster;

    private Commit newCommit;
    private Commit currentCommit;
    private bool transformationRunning;
    private bool movingRunning;
    private float movingStartTime;

    private float speed;

    private void Awake()
    {
        HistoryNavigation.Instance.AddIsland(this);
    }

    public IEnumerator Initialise(BundleMaster bm, int i)
    {
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


    private IEnumerator MoveIsland(Commit newCommit, System.Action<IslandContainerController_Script> callback)
    {
        Vector3 lastDirektion = Vector3.zero;
        Vector2 pos2D = bundleMaster.GetElement(newCommit).GetPosition();
        Vector3 target = new Vector3(pos2D.x, 0f, pos2D.y);

        Vector3 direction = target - island.transform.localPosition;

        speed = direction.magnitude / HistoryNavigation.Instance.timelapsInterval;
        if(speed > HistoryNavigation.Instance.islandMaxSpeed)
        {
            speed = HistoryNavigation.Instance.islandMaxSpeed;
        }
        if(speed < HistoryNavigation.Instance.islandMinSpeed)
        {
            speed = HistoryNavigation.Instance.islandMinSpeed;
        }
        float ankuftzeit = Time.time + direction.magnitude / speed + 1.5f;

        while (direction.magnitude >= 0.5f & Time.time < ankuftzeit )
        {
            Vector3 newPos = island.transform.localPosition + direction.normalized * 0.1f * speed;
            island.transform.localPosition = newPos;

            yield return new WaitForSeconds(0.1f);
            direction = target - island.transform.localPosition;
        }
        //NotivyIslandMovementFinished();
        callback(this);

    }

}
