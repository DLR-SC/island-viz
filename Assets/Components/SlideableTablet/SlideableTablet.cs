using System.Collections;
using UnityEngine;

/// <summary>
/// This additional component creates a slidable tablet on one controller. 
/// All tablet behaviors are manged in the SlidableTabletController.cs.
/// </summary>
public class SlideableTablet : AdditionalIslandVizComponent
{
    public GameObject TabletPrefab; // Prefab of the slidable tablet.
    public Transform TabletParent; // Parent of the slidable tablet, i.e. one of the Hands.

    public float Speed = 0.1f; // Sliding animation speed.

    public float minX; // Most left position of the tablet conent.
    public float maxX; // Most right position of the tablet content.

    private SlideableTabletController tabletController; 


    private void Start(){} // This makes this component toggable in the unity editor.
    

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
        IslandVizUI.Instance.UpdateLoadingScreenUI("InverseMultiTouchInput Construction", "");

        // Init GameObject
        tabletController = Instantiate(TabletPrefab, TabletParent).GetComponent<SlideableTabletController>();
        tabletController.transform.localPosition = Vector3.zero;
        tabletController.transform.localRotation = Quaternion.identity;

        tabletController.SetSlideableTablet(this);

        yield return null;
    }

    #endregion


}
