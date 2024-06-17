using System.Collections;
using System.Collections.Generic;
using IslandData.SystemData;
using IslandEvents;
using IslandPlugins;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This plugin displays all informations of a highlighted <see cref="IslandSystemElement"/> on an UI panel.
/// This script listens to the OnIslandElementHighlighted event.
/// </summary>
public class IslandElementInfoShower2D : IslandPluginBase
{
    public GameObject Panel;
    public Text Label;
    public Text Name;
    public Text Detail;

    public override IEnumerator Init()
    {
        InteractionEvents.OnIslandElementHighlighted += UpdateInfo;
        Panel.SetActive(false);
        yield return null;
    }

    /// <summary>
    /// Enable/disable UI panel and update UI text.
    /// </summary>
    private void UpdateInfo(IIslandSystemElement islandElement, bool highlighted, int handID)
    {
        if (highlighted)
        {
            if (!Panel.activeSelf)
                Panel.SetActive(true);

            if (islandElement is null)
                return;

            Label.text = GenerateLabelText(islandElement.GetLabels());
            Name.text = islandElement.Name;
            Detail.text = GenerateDetailText(islandElement);
        }
        else
        {
            if (Panel.activeSelf)
                Panel.SetActive(false);
        }
    }

    /// <summary>
    /// Generate a string that contains special information based on whether a <see cref="IslandSystemElement"/> 
    /// is an <see cref="Island"/>, a <see cref="Region"/>, or a <see cref="Building"/>.
    /// </summary>
    private string GenerateDetailText (IIslandSystemElement islandElement)
    {
        string detailText = "";

        detailText += "Import Relationships: <b>" + islandElement.Imports.Count + "</b>";
        detailText += "\n";
        detailText += "Export Relationships: <b>" + islandElement.Exports.Count + "</b>";
        detailText += "\n";

        if (islandElement is Island island)
        {
            detailText += "Child Regions: <b>" + island.Regions.Count + "</b>";
        }
        else if (islandElement is Region region)
        {
            detailText += "Child Tiles: <b>" + region.Buildings.Count + "</b>";
        }
        else if (islandElement is Building building)
        {
            detailText += "Building Type: <b>" + building.Type + "</b>";
        }

        return detailText;
    }


    private string GenerateLabelText(IReadOnlyList<string> labels)
    {
        string final = "";

        for (int i = 0; i < labels.Count; i++)
        {
            final += labels[i];

            if (i < labels.Count - 1)
                final += ", ";
        }
        
        return final;
    }
}
