﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;
using OSGI_Datatypes.ArchitectureElements;
using OSGI_Datatypes.OrganisationElements;
using Assets;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Neo4JDriver;

public class LayoutCreation : MonoBehaviour
{
    public static LayoutCreation Instance { get { return instance; } }
    private static LayoutCreation instance; // The instance of this class.

    private int workingThreads;


    private Project project;

    void Start()
    {
        instance = this;
        Neo4J database = GameObject.Find("DatabaseObject").GetComponent<DatabaseAccess>().GetDatabase();
        Neo4JWriterLayout.SetDatabase(database);

        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();
    }

    public IEnumerator AllBundlesGridCreation()
    {
        int bundlesTotal = project.GetMasterBundles().Count;
        IslandVizUI.Instance.UpdateLoadingScreenUI("Creating Island Layout", ""); // Update UI.
        List<ProcessingStatus> coList = new List<ProcessingStatus>();

        workingThreads = 0;

        //StartCoroutines for each Bundle
        foreach(BundleMaster masterB in project.GetMasterBundles())
        {
            ProcessingStatus stat = new ProcessingStatus();
            stat.working = true;
            coList.Add(stat);
            workingThreads++;
            StartCoroutine(CreateGridForBundle(masterB, stat, true, Constants.useValuesFromDBWherePossible, Constants.writeNewValuesToDB));
            if (workingThreads % 10 == 0)
            {
                yield return null;
            }
        }
        yield return null;

        int finished = 0;
        //Wait Until all Coroutines are finished
        while (coList.Count > 0)
        {
            int i = 0;
            while (i < coList.Count)
            {
                if (!coList[i].working)
                {
                    finished++;
                    IslandVizUI.Instance.UpdateLoadingScreenUI("Creating Island Layout", (finished*100/(float)bundlesTotal).ToString("0.00")+"%"); // Update UI.
                    coList.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            yield return new WaitForSeconds(1);

        }

        if (Constants.writeNewValuesToDB)
        {
            yield return Neo4JWriterLayout.WriteCommitIslandsLayouted(project.GetCommits().Keys.ToList<int>());
        }

        Debug.Log("All finished");
        yield return null;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundle">Bundle = Island to be layouted</param>
    /// <param name="flag">For process management</param>
    /// <param name="layoutingNeeded">true if some (or) all commits are not (completely) layouted</param>
    /// <param name="useAbailableLayoutInfo"> true: all layoutinfo from database will be used, false: a completely new layout is created</param>
    /// <param name="writeLayoutToDB">if true new layoutinfo will be written to db</param>
    /// <returns></returns>
    private IEnumerator CreateGridForBundle(BundleMaster bundle, ProcessingStatus flag, bool layoutingNeeded, bool useAbailableLayoutInfo, bool writeLayoutToDB)
    {
        HexGrid grid = new HexGrid(2);
        bundle.SetGrid(grid);

        if (layoutingNeeded || !useAbailableLayoutInfo)
        {
            LayoutCreator layoutCreator = new LayoutCreator(bundle, useAbailableLayoutInfo, writeLayoutToDB);
            yield return layoutCreator.Create();
        }
        else
        {
            yield return CreateGridForBundleFromData(bundle);
        }
        yield return null;
        flag.working = false;

    }

    
    /// <summary>
    /// If all layoutInformation for Island is available from database and no new layout is requested the grid can be created from data
    /// simply get through all compUnits of bundle and assign them to their specified cell
    /// </summary>
    /// <param name="bundle"></param>
    /// <returns></returns>
    private IEnumerator CreateGridForBundleFromData(BundleMaster bundle)
    {
        HexGrid grid = bundle.GetGrid();
        List<CompUnitMaster> masterCus = bundle.GetContainedMasterCompUnits();
        yield return null;

        foreach(CompUnitMaster cum in masterCus)
        {
            if (cum.HasValidGridInfo())
            {
                Vector2Int gridPos = cum.GetGridPos();
                Cell cell = grid.GetCellByGridCreateCell(gridPos.x, gridPos.y);
                cum.SetCell(cell);
                cell.SetCompUnitMaster(cum);
                //TODO ggf. celle auch mit Region / Package verbinden
            }
            else
            {
                Debug.LogError("Creating Layout for a masterBundle from data where a masterCu has no position");
            }
            yield return null;
        }
        grid.FillOuterAssignedDictionarys(bundle.GetCommitsByTimeline(OSGI_Datatypes.ComposedTypes.SortTypes.byBranch));
        yield return null;
    }

    


}