using System.Collections;
using IslandPlugins;
using UnityEngine;

/// <summary>
/// This plugin creates a slidable tablet on the left controller. 
/// All tablet behaviors are manged in the SlidableTabletController.cs.
/// </summary>
public class SlideableTablet : IslandPluginBase
{
    // public GameObject TabletPrefab; // Slidable tablet prefab.

    // public float Speed = 0.1f; // Sliding animation speed.

    // public float minX; // Most left position of the tablet conent (closed).
    // public float maxX; // Most right position of the tablet content (open).

    // private SlideableTabletController tabletController;
    // private Transform tabletParent; // Parent of the slidable tablet, i.e. one of the Hands.
    

    // // ################
    // // Initiation
    // // ################

    // #region Initiation

    // /// <summary>
    // /// Initialize this input component. 
    // /// This method is called by the IslandVizInteraction class.
    // /// </summary>
    public override IEnumerator Init()
    {
    //     tabletParent = Player.instance.leftHand.transform;

    //     // Init tablet GameObject.
    //     tabletController = Instantiate(TabletPrefab, tabletParent).GetComponent<SlideableTabletController>();
    //     tabletController.transform.localPosition = Vector3.zero;
    //     tabletController.transform.localRotation = Quaternion.identity;

    //     tabletController.SetSlideableTablet(this);

         yield return null;
    }

    // #endregion


}
