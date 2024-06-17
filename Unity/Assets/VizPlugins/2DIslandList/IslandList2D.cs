using IslandViz.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using IslandData.SystemData;
using IslandEvents;
using IslandLevelOfDetail;
using UnityEngine;
using UnityEngine.UI;

public class IslandList2D : MonoBehaviour
{
    public GameObject IslandNameElementPrefab;

    public Transform IslandNameParent;

    public Text ShowIslandsTooltipps;


    private void Start()
    {
        VizEvents.OnPluginsLoaded += CreateIslandNameElements;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnShowIslandsClicked();
        }
    }

    private void CreateIslandNameElements()
    {
        List<Island> existingIslands = VizManager.IslandData.Islands.OrderBy(x => x.Name).ToList();

        foreach (var island in existingIslands)
        {
            var islandName = Instantiate(IslandNameElementPrefab, IslandNameParent).GetComponent<IslandName>();
            islandName.SetValues(island, this);
        }

        IslandNameParent.gameObject.SetActive(false);
        IslandNameParent.gameObject.SetActive(false);
    }

    public void OnIslandNameHighlighted(Island island, bool active)
    {
        InteractionEvents.OnIslandElementHighlighted(island, active, 0);
    }

    public void OnIslandNameSelected(Island island)
    {
        OnShowIslandsClicked(); // Hide island names.
        IslandTransformation.Instance.CenterIslands(island.Entity, IslandLOD.Island);
    }

    public void OnShowIslandsClicked()
    {
        IslandNameParent.gameObject.SetActive(!IslandNameParent.gameObject.activeSelf);
        ShowIslandsTooltipps.text = (IslandNameParent.gameObject.activeSelf ? "Hide" : "Show") + " Island List";
    }
}
