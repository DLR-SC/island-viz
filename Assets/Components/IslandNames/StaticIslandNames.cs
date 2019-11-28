using OsgiViz.Core;
using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This component adds a name tag to an island, when a island is selected.
/// </summary>
public class StaticIslandNames : AdditionalIslandVizComponent
{

    public GameObject IslandNameParentPrefab;
    public GameObject IslandNamePrefab;

    private Transform StaticIslandNameParent;
    private List<Transform> currentTargets;

    private bool initiated = false;

    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Initialize this input component. 
    /// This method is called by the IslandVizInteraction class.
    /// </summary>
    public override IEnumerator Init()
    {
        // Create parent GameObject of the island names
        StaticIslandNameParent = ((GameObject) Instantiate(IslandNameParentPrefab, Vector3.zero, Quaternion.identity)).transform;
        //IslandNameParent.parent = IslandVizVisualization.Instance.VisualizationRoot;

        currentTargets = new List<Transform>();

        IslandVizInteraction.Instance.OnIslandSelect += UpdateIslandName;
        IslandVizInteraction.Instance.OnRegionSelect += UpdateRegionName;
        IslandVizInteraction.Instance.OnBuildingSelect += UpdateBuildingName;

        yield return null;

        initiated = true;
    }
    #endregion


    private void UpdateIslandName (IslandGO island, bool selected)
    {
        if (selected)
        {
            CreateName(island.transform);
        }
        else
        {
            RemoveIslandName(island.transform);
        }
    }

    private void UpdateRegionName(Region region, bool selected)
    {
        if (selected)
        {
            CreateName(region.transform);
        }
        else
        {
            RemoveIslandName(region.transform);
        }
    }

    private void UpdateBuildingName(Building building, bool selected)
    {
        if (selected)
        {
            CreateName(building.transform);
        }
        else
        {
            RemoveIslandName(building.transform);
        }
    }





    private void CreateName (Transform target)
    {
        if (!currentTargets.Contains(target))
        {
            StartCoroutine(ShowTargetName(target));
        }
    }

    IEnumerator ShowTargetName (Transform target)
    {
        //Debug.Log("Spawning Text");
        currentTargets.Add(target);
        bool isIsland = target.GetComponent<IslandGO>() != null;

        GameObject islandName = (GameObject)Instantiate(IslandNamePrefab);
        islandName.transform.parent = StaticIslandNameParent;
        islandName.transform.position = new Vector3(target.position.x, GlobalVar.hologramTableHeight + 0.075f, target.position.z);
        islandName.GetComponent<StaticIslandName>().ChangeName(target.name);
        islandName.GetComponent<AlwaysLookAtTarget>().Target = Camera.main.transform;

        yield return new WaitForFixedUpdate();

        while (currentTargets.Contains(target))
        {
            if (target.gameObject.activeSelf)
            {
                if (!islandName.activeSelf)
                    islandName.SetActive(true);

                if (IslandVizVisualization.Instance.CurrentZoomLevel != ZoomLevel.Near)
                {
                    islandName.transform.position = new Vector3(target.position.x, GlobalVar.hologramTableHeight + 0.075f + currentTargets.IndexOf(target) * 0.1f, target.position.z);
                }
                else
                {
                    if (isIsland)
                    {
                        islandName.transform.position = new Vector3(target.position.x, GlobalVar.hologramTableHeight + 0.2f + GlobalVar.CurrentZoom * 2f, target.position.z);
                    }
                    else
                    {
                        islandName.transform.position = new Vector3(target.position.x, GlobalVar.hologramTableHeight + 0.2f + GlobalVar.CurrentZoom, target.position.z);
                    }
                }
            }
            else
            {
                if (islandName.activeSelf)
                    islandName.SetActive(false);
            }
            yield return new WaitForFixedUpdate();
        }

        Destroy(islandName);
    }

    /// <summary>
    /// By removing the island from the currentIslands list, we stop the IslandName Coroutine and the island name disappears.
    /// </summary>
    /// <param name="target">The target that was deselected.</param>
    private void RemoveIslandName (Transform target)
    {
        if (currentTargets.Contains(target))
            currentTargets.Remove(target);
    }

}
