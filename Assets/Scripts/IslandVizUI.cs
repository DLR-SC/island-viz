using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandVizUI : MonoBehaviour
{
    public static IslandVizUI Instance;

    [Header("General UI Components")]
    public Transform TableUI_Parent;
    public Transform StaticUI_Parent;
    public GameObject LoadingScreen;
    public GameObject ZoomLevel;

    [Header("Loading Screen Components")]
    public Text LoadingScreenProgressValue;
    public Text LoadingScreenNameValue;


    [Header("Zoom Level Components")]
    public Slider ZoomLevelSlider;
    public Text ZoomLevelValue;

    [Header("Current Visible Islands Components")]
    public Text CurrentVisivleIslandsValue;



    void Awake()
    {
        Instance = this;

        IslandVizBehaviour.Instance.OnConstructionDone += ConstructionDone;

        StaticUI_Parent.gameObject.SetActive(true); // This is probably disabled because it is annoying in the editor, so we enable it here ;)
    }

    // ################
    // General
    // ################

    /// <summary>
    /// Call this when the table height was changed to also change the height of all UI elements.
    /// </summary>
    public void OnTableHeightChanged ()
    {
        TableUI_Parent.position = Vector3.up * OsgiViz.Core.GlobalVar.hologramTableHeight;
    }

    public void ConstructionDone ()
    {
        StaticUI_Parent.gameObject.SetActive(false);
    }


    // ################
    // Loading Screen
    // ################

    public void UpdateLoadingScreenUI (string currentName, string progress)
    {
        LoadingScreenNameValue.text = currentName;
        LoadingScreenProgressValue.text = progress;
    }




    // ################
    // Zoom Level
    // ################

    public void UpdateZoomLevelUI (float zoomLevelInPercent)
    {
        ZoomLevelSlider.value = 100 - zoomLevelInPercent;
        ZoomLevelValue.text = zoomLevelInPercent.ToString("0") + "%";
    }


    // ################
    // Current Visible Islands
    // ################
    public void UpdateCurrentVisibleIslandsUI (float percent)
    {
        CurrentVisivleIslandsValue.text = "Currently visible:\n<b>" + percent.ToString("0") + "%</b> of all islands";
    }
}
