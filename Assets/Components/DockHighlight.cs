using OsgiViz;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockHighlight : AdditionalIslandVizComponent
{
    public Material HighlightMaterial;

    private List<DependencyDock> Highlights; 

    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Initialize this visualization component. 
    /// This method is called by the IslandVizVisualization class.
    /// </summary>
    public override IEnumerator Init()
    {
        IslandVizInteraction.Instance.OnDockSelect += HighlightDock;

        Highlights = new List<DependencyDock>();

        yield return null;
    }

    #endregion




    private void HighlightDock (DependencyDock dock, IslandVizInteraction.SelectionType selectionType, bool selected)
    {
        if (selectionType == IslandVizInteraction.SelectionType.Highlight)
        {
            if (selected && !Highlights.Contains(dock))
            {
                GameObject highlight = new GameObject("Highlight");
                highlight.AddComponent<MeshRenderer>().material = HighlightMaterial;
                highlight.AddComponent<MeshFilter>().mesh = dock.GetComponent<MeshFilter>().mesh;
                highlight.transform.parent = dock.transform;
                highlight.transform.localScale = Vector3.one * 1.01f;
                highlight.transform.position = dock.transform.position;
                highlight.transform.rotation = dock.transform.rotation;
                Highlights.Add(dock);
            }
            else if (!selected && Highlights.Contains(dock))
            {
                Transform h = dock.transform.Find("Highlight");
                if (h != null)
                    Destroy(h.gameObject);
                Highlights.Remove(dock);
            }
        }

        if (!dock.Selected && selectionType == IslandVizInteraction.SelectionType.Select)
        {

        }
    }





}
