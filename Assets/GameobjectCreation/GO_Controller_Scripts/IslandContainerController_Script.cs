using OSGI_Datatypes.ArchitectureElements;
using OSGI_Datatypes.ComposedTypes;
using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;

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

    // Start is called before the first frame update
    void Start()
    {
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
        yield return island.GetComponent<IslandController_Script>().Initialise();
        island.SetActive(false);

        //Subscribe to Event
        IslandVizInteraction.Instance.OnNewCommit += OnNewCommit;


        //StartCoroutine(Renew());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator Renew()
    {
        while (true)
        {
            if (!transformationRunning && !movingRunning)
            {
                //newCommit = mainController.GetCurrentCommit();
                newCommit = HistoryNavigation.Instance.GetCurrentCommit();
                if (newCommit != null && newCommit != currentCommit)
                {
                    transformationRunning = true;

                    if(bundleMaster.RelationOfCommitToTimeline(newCommit) != TimelineStatus.present)
                    {
                        //bundle not present in new Commit
                        if (island.activeSelf)
                        {
                            //if island still active set inactive
                            island.SetActive(false);
                        }
                        //nothin further to do
                        NotifyIslandTransformationFinished();
                        yield return new WaitForSeconds(1);
                        continue;
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
                    }
                    else
                    {
                        movingStartTime = Time.time;
                        movingRunning = true;
                        island.GetComponent<Rigidbody>().isKinematic = false;
                        StartCoroutine(MoveIsland(newCommit));
                        //TODO island position transformation
                        //Vector2 pos2D = bundleMaster.GetElement(newCommit).GetPosition();
                        //island.transform.localPosition = new Vector3(pos2D.x, 0f, pos2D.y);
                    }
                    //Island Appearance Transformation
                    StartCoroutine(island.GetComponent<IslandController_Script>().UpdateRoutine(newCommit, this));
                }
            }
            yield return new WaitForSeconds(1);
        }
    }


    private void OnNewCommit(Commit oldCommit, Commit newCommit)
    {
        Debug.Log(gameObject.name + "Encounter newCommitCall");


        if (bundleMaster.RelationOfCommitToTimeline(newCommit) != TimelineStatus.present)
        {
            //bundle not present in new Commit
            if (island.activeSelf)
            {
                //if island still active set inactive
                island.SetActive(false);
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
        }
        else
        {
            movingStartTime = Time.time;
            movingRunning = true;
            //island.GetComponent<Rigidbody>().isKinematic = false;
            StartCoroutine(MoveIsland(newCommit));
        }
        //Island Appearance Transformation
        StartCoroutine(island.GetComponent<IslandController_Script>().UpdateRoutine(newCommit, this));
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

             try
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
             }
            yield return new WaitForSeconds(0.1f);
            direction = target - island.transform.position;
        }
        NotivyIslandMovementFinished();

    }

    private IEnumerator MoveIsland(Commit newCommit)
    {
        Vector2 pos2D = bundleMaster.GetElement(newCommit).GetPosition();
        Vector3 target = new Vector3(pos2D.x, 0f, pos2D.y);

        Vector3 direction = target - island.transform.localPosition;

        while (direction.magnitude >= 1.0 /*& Time.time - movingStartTime < 10*/)
        {
            island.transform.Translate(direction.normalized * Time.deltaTime*0.3f);
            yield return null;

            //yield return new WaitForSeconds(0.1f);
            direction = target - island.transform.localPosition;
        }
        //NotivyIslandMovementFinished();

    }


    public void NotifyIslandTransformationFinished()
    {
        transformationRunning = false;
        NotifyAllFinished();

    }

    public void NotivyIslandMovementFinished()
    {
        movingRunning = false;
        island.GetComponent<Rigidbody>().velocity = Vector3.zero;
        island.GetComponent<ConstantForce>().force = Vector3.zero;
        island.GetComponent<Rigidbody>().isKinematic = true;
        NotifyAllFinished();

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
