using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manages all user interface (UI) elements in the IslandViz application.
/// All UI elements are accessable through this class. 
/// Additionally this class provides some helper functions for UI actions.
/// </summary>
public class IslandVizUI : MonoBehaviour
{
    public static IslandVizUI Instance; // The instance of this class.

    [Header("General UI Components")]
    public Transform TableUI_Parent; // Parent Transform of all UI elements, that are "attached" to the table.
    public Transform StaticUI_Parent; // Parent Transform of all static UI elements.
    public GameObject LoadingScreen; // Parent GameObject of all loading screen UI elements.
    public GameObject ZoomLevel; // Parent GameObject of all zoom level UI elements.
    public GameObject Notification;
    public GameObject BundleNameSelection; // The the Scroll View containing the bundle names.

    [Header("Loading Screen Components")]
    public Text LoadingScreenProgressValue; // Text element containing the loading progress (in %) of the current loading process.
    public Text LoadingScreenNameValue; // Text element containing the name of the current loading process. 

    [Header("Zoom Level Components")]
    public Slider ZoomLevelSlider; // Slider element showing the current zoom level like a progress bar.
    public Text ZoomLevelValue; // Text element containing the current zoom level (in %).

    [Header("Current Visible Islands Components")]
    public Text CurrentVisivleIslandsValue; // Text element containing the current number of visible islands (in %). 

    [Header("Current Notification Components")]
    public Text NotificationValue;

    [Header("Current Selected Components")]
    public Text CurrentSelectedHeader; 
    public Text CurrentSelectedBody;

    [Header("Bundle Name Selection Components")]
    public Transform BundleNameSelectionContent; // The "Content" child of the Scroll View containing the bundle names.
    public GameObject BundleNamePrefab;

    void Awake()
    {
        Instance = this;

        IslandVizBehaviour.Instance.OnConstructionDone += OnConstructionDone; // Subscribe to the OnConstructionDone event of the IslandVizBehaviour.

        StaticUI_Parent.gameObject.SetActive(true); // This is probably disabled because it is annoying in the editor, so we enable it here ;)
        Notification.SetActive(false);

        IslandVizVisualization.Instance.OnTableHeightChanged += TableHeightChanged;

        IslandVizInteraction.Instance.OnIslandSelect += OnIslandSelected;
        IslandVizInteraction.Instance.OnRegionSelect += OnRegionSelected;
        IslandVizInteraction.Instance.OnBuildingSelect += OnBuildingSelected;
    }


    // ################
    // Event Handling
    // ################

    /// <summary>
    /// Call this when the table height was changed to also change the height of all UI elements that are attached to the table.
    /// </summary>
    public void TableHeightChanged (float newHeight)
    {
        TableUI_Parent.position = Vector3.up * newHeight;
    }

    /// <summary>
    /// This method is called when the IslandViz construction is done.
    /// </summary>
    public void OnConstructionDone ()
    {
        StaticUI_Parent.gameObject.SetActive(false); // Disable the loading screen.
    }





    // ################
    // Loading Screen
    // ################

    /// <summary>
    /// Change the process name and progress of the loading screen.
    /// </summary>
    /// <param name="processName">The name of the process that is current loading.</param>
    /// <param name="processProgress">The loading progress of the current process (e.g. "88.76%"). Leave empty when you do not want to show progress.</param>
    public void UpdateLoadingScreenUI (string processName, string processProgress)
    {
        LoadingScreenNameValue.text = processName;
        LoadingScreenProgressValue.text = processProgress;
    }
       



    // ################
    // Zoom Level
    // ################

    /// <summary>
    /// Change the position of the zoom level slider and the text containing the current zoom level in %.
    /// </summary>
    /// <param name="zoomLevelInPercentage">The zoom level in % (e.g. 88.76).</param>
    public void UpdateZoomLevelUI (float zoomLevelInPercentage)
    {
        ZoomLevelSlider.value = 100 - zoomLevelInPercentage; // Invert the percentage value to be represented correctly by the slider.
        ZoomLevelValue.text = zoomLevelInPercentage.ToString("0") + "%";
    }




    // ################
    // Current Visible Islands
    // ################

    /// <summary>
    /// Change the text containting the current percentage of visible islands.
    /// </summary>
    /// <param name="percentage">The number of visible islands in % (e.g. 88.76).</param>
    public void UpdateCurrentVisibleIslandsUI (float percentage)
    {
        CurrentVisivleIslandsValue.text = "Currently visible:\n<b>" + percentage.ToString("0") + "%</b> of all islands";
    }




    // ################
    // Notifications
    // ################

    public void MakeNotification (float duration, string text)
    {
        StartCoroutine(NotificationRoutine(duration, text));
    }

    IEnumerator NotificationRoutine (float duration, string text)
    {
        Notification.SetActive(true);
        NotificationValue.text = text;

        yield return new WaitForSeconds(duration);

        Notification.SetActive(false);
    }


    // ################
    // Bundle Name Selection
    // ################

    public void InitBundleNames ()
    {
        List<OsgiViz.Unity.Island.IslandGO> islandList = IslandVizVisualization.Instance.IslandGOs.OrderBy(o => o.name).ToList();

        for (int i = 0; i < islandList.Count; i++)
        {
            ((GameObject)Instantiate(BundleNamePrefab, BundleNameSelectionContent)).GetComponent<IslandNameSelectionName>().Init(islandList[i]);
        }
    }


    // ################
    // Current Selected
    // ################

    public void OnIslandSelected (IslandGO island, IslandVizInteraction.SelectionType selectionType, bool selected)
    {
        if (selectionType == IslandVizInteraction.SelectionType.Select && selected && island != null)
        {
            UpdateCurrentSelectedInfo("Bundle", island.name, "Packages", island.Regions.Count.ToString());
        }
        else if (island == null)
        {
            UpdateCurrentSelectedInfo();
        }
    }

    public void OnRegionSelected (Region region, IslandVizInteraction.SelectionType selectionType, bool selected)
    {
        if (selectionType == IslandVizInteraction.SelectionType.Select && selected && region != null)
        {
            UpdateCurrentSelectedInfo("Package", region.name, "Classes", region.getBuildings().Count.ToString());
        }
    }

    public void OnBuildingSelected (Building building, IslandVizInteraction.SelectionType selectionType, bool selected)
    {
        if (selectionType == IslandVizInteraction.SelectionType.Select && selected && building != null)
        {
            UpdateCurrentSelectedInfo(building.getCU().GetModifier().ToString() + " " + building.getCU().GetType().ToString(), building.getCU().getName(), "LOC", building.getCU().getLoc() == 0 ? "n.a." : building.getCU().getLoc().ToString());
        }
    }
           
    public void UpdateCurrentSelectedInfo(string type, string name, string detailName, string detail)
    {
        //TODO hier fehler
        CurrentSelectedHeader.text = type;
        CurrentSelectedBody.text = "<b>Name:</b>\n" + name + "\n<b>" + detailName + "</b>:\n" + detail;
    }

    public void UpdateCurrentSelectedInfo()
    {
        CurrentSelectedHeader.text = "Nothing Selected ...";
        CurrentSelectedBody.text = "";
    }
}
