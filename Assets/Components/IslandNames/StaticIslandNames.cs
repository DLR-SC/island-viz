using OsgiViz.Core;
using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticIslandNames : AdditionalIslandVizComponent
{

    public GameObject IslandNameParentPrefab;
    public GameObject IslandNamePrefab;

    private Transform IslandNameParent;
    private List<IslandGO> currentIslands;
    private List<GameObject> IslandNames;

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
        IslandNameParent = ((GameObject) Instantiate(IslandNameParentPrefab)).transform;
        IslandNameParent.parent = IslandVizVisualization.Instance.VisualizationRoot;
        IslandNameParent.localPosition = Vector3.zero;

        IslandNames = new List<GameObject>();
        currentIslands = new List<IslandGO>();

        IslandVizInteraction.Instance.OnIslandSelected += CrateIslandName;
        IslandVizInteraction.Instance.OnIslandDeselected += RemoveIslandName;

        yield return null;

        initiated = true;
    }
    #endregion

    private void CrateIslandName (IslandGO island)
    {
        if (!currentIslands.Contains(island))
        {
            currentIslands.Add(island);
            StartCoroutine(IslandName(island));
        }
    }

    IEnumerator IslandName (IslandGO island)
    {
        GameObject islandName = (GameObject)Instantiate(IslandNamePrefab);
        islandName.transform.parent = IslandNameParent;
        islandName.transform.localScale = Vector3.one * 0.001f;
        islandName.transform.position = new Vector3(island.transform.position.x, GlobalVar.hologramTableHeight + 0.075f, island.transform.position.z);
        islandName.GetComponent<StaticIslandName>().ChangeName(island.name);
        islandName.GetComponent<AlwaysLookAtTarget>().Target = Camera.main.transform;
        //IslandNames.Add(islandName);

        yield return new WaitForFixedUpdate();

        while (currentIslands.Contains(island))
        {
            if (island.gameObject.activeSelf)
            {

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
    /// <param name="island">The island that was deselected.</param>
    private void RemoveIslandName (IslandGO island)
    {
        if (currentIslands.Contains(island))
            currentIslands.Remove(island);
    }

}
