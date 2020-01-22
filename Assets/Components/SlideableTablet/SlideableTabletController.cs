using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the slidable tablet, i.e. opening and closing the tablet and handling UI button events.
/// Basic functionality: When a UI button is hightlighted (the tablet also consists of a background "Trigger" GameObject with a UI_Button component) tablet opens.
/// When no UI button of the tablet is highlighted, i.e. the user is not pointing at the tablet, the tablet closes.
/// </summary>
public class SlideableTabletController : MonoBehaviour
{
    public RectTransform Content; // Parent transform of the content, i.e. the UI buttons and the background images.
    public Collider[] ButtonColliders; // List of all button colliders that will disable when the tablet is closed, i.e. the buttons wont be clickable.

    private SlideableTablet slideableTablet; // This contains the tablet settings.
    private List<GameObject> currentHighlightedTabletObjects; // List of all buttons that are currently highlighted.

    private bool tabletOpen = false; 
    private bool tabletAnimationRunning = false;



    /// <summary>
    /// Called by Unity when the tablet prefab (containing this) is initiated.
    /// </summary>
    private void Awake()
    {
        currentHighlightedTabletObjects = new List<GameObject>();
    }




    // ################
    // UI Button OnClick Functionality
    // ################

    #region UI Button OnClick Functionality

    public void Recenter()
    {
        IslandVizVisualization.Instance.RecenterView();
    }

    public void Undo()
    {
        IslandVizBehaviour.Instance.Undo();
    }

    public void AllBundles()
    {
        IslandVizVisualization.Instance.HighlightAllIslands(true);
    }

    public void NoBundles()
    {
        IslandVizVisualization.Instance.HighlightAllIslands(false);
    }

    public void AllDependencies()
    {
        IslandVizVisualization.Instance.HighlightAllDocks(true);
    }

    public void NoDependencies()
    {
        IslandVizVisualization.Instance.HighlightAllDocks(false);
    }

    #endregion



    // ################
    // Tablet Sliding
    // ################

    #region Tablet Sliding

    /// <summary>
    /// When at least one button of the tablet is highlighted (the Trigger GameObject is also counting as a button) the tablet needs to be open.
    /// Called by the UI Buttons OnHighlight, i.e. when the highlight status changed.
    /// </summary>
    /// <param name="button"></param>
    public void OnHighlight(GameObject button) 
    {
        if (!currentHighlightedTabletObjects.Contains(button))
        {
            currentHighlightedTabletObjects.Add(button);
        }
        else
        {
            currentHighlightedTabletObjects.Remove(button);
        }
        StartCoroutine(DelayedToggleCheck());
    }

    /// <summary>
    /// When the RaycastSelection no longer selects e.g. the Recenter button, this button throws a onhighlight(false) event which would cause the tablet to close. 
    /// However when hitting the tablet Trigger within the delay, the tablet will not close.
    /// This also makes shure that the ShowHideTablet Coroutine is not called while still running.
    /// </summary>
    IEnumerator DelayedToggleCheck()
    {
        yield return new WaitForSeconds(0.1f);

        if (tabletAnimationRunning)
        {
            yield break;
        }

        if ((!tabletOpen && currentHighlightedTabletObjects.Count >= 1) || (tabletOpen && currentHighlightedTabletObjects.Count == 0))
        {
            yield return TabletAnimation();
        }
    }


    /// <summary>
    /// Lerps the tablet content position from a open position to a closed position and vice versa.
    /// </summary>
    IEnumerator TabletAnimation()
    {
        tabletAnimationRunning = true;

        float value = 0f;

        Vector3 startPos = tabletOpen ? new Vector3(slideableTablet.maxX, 0f, 0f) : new Vector3(slideableTablet.minX, 0f, 0f);
        Vector3 endPos = tabletOpen ? new Vector3(slideableTablet.minX, 0f, 0f) : new Vector3(slideableTablet.maxX, 0f, 0f);

        while (value <= 1)
        {
            Content.localPosition = Vector3.Lerp(startPos, endPos, value);
            value += slideableTablet.Speed;
            yield return new WaitForFixedUpdate();
        }

        Content.localPosition = endPos;

        tabletOpen = !tabletOpen;

        foreach (var item in ButtonColliders)
        {
            item.enabled = tabletOpen;
        }

        tabletAnimationRunning = false;

        // Fix issue when user deselects the tablet while the tablet slides open.
        if (tabletOpen && currentHighlightedTabletObjects.Count == 0) 
        {
            StartCoroutine(DelayedToggleCheck());
        }
    }

    #endregion




    public void SetSlideableTablet (SlideableTablet slideableTablet)
    {
        this.slideableTablet = slideableTablet;
    }


}
