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
    public static StaticIslandNames Instance;

    public GameObject IslandNameParentPrefab;
    public GameObject IslandNamePrefab;

    private Transform StaticIslandNameParent;

    //private List<Transform> currentTargets;
    //private List<StaticIslandName> currentStaticIslandNames;

    Dictionary<Transform, StaticIslandName> currentNames;

    private float minTextDistance = 0.5f;





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
        Instance = this;

        // Create parent GameObject of the island names
        StaticIslandNameParent = ((GameObject) Instantiate(IslandNameParentPrefab, Vector3.zero, Quaternion.identity)).transform;
        //IslandNameParent.parent = IslandVizVisualization.Instance.VisualizationRoot;

        currentNames = new Dictionary<Transform, StaticIslandName>();

        IslandVizInteraction.Instance.OnIslandSelect += OnIslandSelectionUpdate;
        IslandVizInteraction.Instance.OnRegionSelect += OnRegionSelectionUpdate;
        IslandVizInteraction.Instance.OnBuildingSelect += OnBuildingSelectionUpdate;

        IslandVizVisualization.Instance.OnVisualizationScaleChanged += UpdateHeightIndexes;

        yield return null;
    }
    #endregion


    private void OnIslandSelectionUpdate (IslandGO island, bool selected)
    {
        if (selected)
        {
            CreateStaticName(island.transform);
        }
        else
        {
            RemoveStaticName(island.transform);
        }
    }

    private void OnRegionSelectionUpdate(Region region, bool selected)
    {
        if (selected)
        {
            CreateStaticName(region.transform);
        }
        else
        {
            RemoveStaticName(region.transform);
        }
    }

    private void OnBuildingSelectionUpdate(Building building, bool selected)
    {
        if (selected)
        {
            CreateStaticName(building.transform);
        }
        else
        {
            RemoveStaticName(building.transform);
        }
    }




    private void CreateStaticName (Transform target)
    {
        if (!currentNames.ContainsKey(target))
        {
            // Instantiate & initiate StaticIslandName
            GameObject islandName = (GameObject)Instantiate(IslandNamePrefab);
            islandName.transform.parent = StaticIslandNameParent;
            islandName.transform.position = new Vector3(target.position.x, GlobalVar.hologramTableHeight + 0.075f, target.position.z);
            StaticIslandName staticIslandName = islandName.GetComponent<StaticIslandName>();

            currentNames.Add(target, staticIslandName);

            staticIslandName.Init(target, target.name);
        }
    }
    

    /// <summary>
    /// By removing the island from the currentIslands list, we stop the IslandName Coroutine and the island name disappears.
    /// </summary>
    /// <param name="target">The target that was deselected.</param>
    private void RemoveStaticName (Transform target)
    {
        if (currentNames.ContainsKey(target))
        {
            Destroy(currentNames[target].gameObject);
            currentNames.Remove(target);
        }
    }




    // TODO Check performance
    private void UpdateHeightIndexes ()
    {
        foreach (var item in currentNames)
        {
            item.Value.SetHeightIndex(GetHeightIndex(item.Value));
        }
    }






    public int GetHeightIndex(StaticIslandName staticIslandName)
    {
        List<KeyValuePair<Transform, StaticIslandName>> allEntriesInRadius = GetAllEntriesInRadius(minTextDistance, staticIslandName);

        if (allEntriesInRadius == null || allEntriesInRadius.Count == 0)
        {
            return 1;
        }
        else
        {
            for (int heightIndex = 1; heightIndex <= allEntriesInRadius.Count; heightIndex++) // Go through all possible numbers to see if there is an empty spot.
            {
                if (!EntriesContainHeightIndex(allEntriesInRadius, heightIndex))
                {
                    return heightIndex;
                }
            }
            return allEntriesInRadius.Count + 1; // Just add on top;
        }
    }

    private List<KeyValuePair<Transform, StaticIslandName>> GetAllEntriesInRadius(float radius, StaticIslandName origin)
    {
        List<KeyValuePair<Transform, StaticIslandName>> result = new List<KeyValuePair<Transform, StaticIslandName>>();

        foreach (var item in currentNames)
        {
            if (item.Value != origin && Vector3.Distance(item.Key.position, origin.GetTarget().position) <= radius)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private bool EntriesContainHeightIndex(List<KeyValuePair<Transform, StaticIslandName>> entries, int heightIndex)
    {
        foreach (var item in entries)
        {
            if (item.Value.GetHeightIndex() == heightIndex)
            {
                return true;
            }
        }
        return false;
    }




}
