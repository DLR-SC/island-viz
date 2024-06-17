using IslandInterface.Selection;
using IslandLevelOfDetail;
using IslandViz.Core;
using Unity.Entities;
using UnityEngine;

public class VizFunctions : MonoBehaviour
{

    // ################
    // Island Highlight
    // ################

    #region Island Highlight

    /// <summary>
    /// Highlights or dehiglights all islands.
    /// </summary>
    /// <param name="highlight"></param>
    public static void HighlightAllIslands(bool highlight)
    {

        if (LODManager.CurrentLOD == IslandLOD.Island)
        {
            foreach (var island in VizManager.IslandData.Islands)
            {
                if (island.Entity == Entity.Null)
                    continue;

                if (highlight)
                    IslandInteraction.Instance.TryHighlight(island);
                else
                    IslandInteraction.Instance.TryDehighlight(island);
            }
        }
        else if (LODManager.CurrentLOD == IslandLOD.Region)
        {
            foreach (var region in VizManager.IslandData.Regions)
            {
                if (region.Entity == Entity.Null)
                    continue;

                if (highlight)
                    IslandInteraction.Instance.TryHighlight(region);
                else
                    IslandInteraction.Instance.TryDehighlight(region);
            }
        }
        else if (LODManager.CurrentLOD == IslandLOD.Building)
        {
            foreach (var building in VizManager.IslandData.Buildings)
            {
                if (building.Entity == Entity.Null)
                    continue;

                if (highlight)
                    IslandInteraction.Instance.TryHighlight(building);
                else
                    IslandInteraction.Instance.TryDehighlight(building);
            }
        }
    }

    #endregion

}
