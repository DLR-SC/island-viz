using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This visualization component creates a compass on the table. The compass needle rotates when the visualization is rotated.
/// </summary>
public class Compass : AdditionalIslandVizComponent
{
    public GameObject CompassPrefab; // Prefab of the compass GameObject. It is important, that this prefab contains one child named "Needle"!
    public Vector3 InitialPosition; // The position the compass is initiated. The y-position is set automaticly.
    public float HeightOffset; // The height of the compass is set to the tableheight + this offset.

    private GameObject compass; // GameObject of the instantiated compass.
    private Transform compassNeedle; // Transform of the compass needle, that we are moving.

    private bool initialized = false;


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
        compass = (GameObject)Instantiate(CompassPrefab, InitialPosition, CompassPrefab.transform.rotation);
        compass.name = "VisualizationComponent - Compass";
        compassNeedle = compass.transform.Find("Needle");

        UpdateHeight(GlobalVar.hologramTableHeight);
        IslandVizVisualization.Instance.OnTableHeightChanged += UpdateHeight;

        yield return null;
        initialized = true;
    }
    #endregion


    // ################
    // Needle Rotation
    // ################

    /// <summary>
    /// Called by Unity every fixed time span.
    /// </summary>
    private void FixedUpdate()
    {
        if (!initialized)
            return;

        if (compassNeedle.localRotation.z != IslandVizVisualization.Instance.VisualizationRoot.rotation.y)
        {
            compassNeedle.localRotation = Quaternion.Euler(0, 0, IslandVizVisualization.Instance.VisualizationRoot.rotation.eulerAngles.y);
        }
    }


    // ################
    // Compass Height
    // ################

    /// <summary>
    /// Update the y-position of the Compass. This is called, when the table height was changed.
    /// </summary>
    /// <param name="newHeight"></param>
    public void UpdateHeight (float newHeight)
    {
        compass.transform.position = new Vector3(compass.transform.position.x, newHeight + HeightOffset, compass.transform.position.z);
    }


    public void OnVisualizationRotationChanged () // TODO
    {

    }
}
