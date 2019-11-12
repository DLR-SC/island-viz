using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandVizUI : MonoBehaviour
{
    public static IslandVizUI Instance;


    [Header("Zoom Level Components")]
    public Text ZoomLevelValue;
    public RectTransform ZoomLevelLine;
    public Vector2 ZoomLevelLineMinMax; // x = min; y = max




    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }





    public void UpdateZoomLevelUI (float zoomLevelInPercent)
    {
        ZoomLevelLine.localPosition = Vector3.Lerp(new Vector3(ZoomLevelLine.localPosition.x, ZoomLevelLineMinMax.x, ZoomLevelLine.localPosition.z), new Vector3(ZoomLevelLine.localPosition.x, ZoomLevelLineMinMax.y, ZoomLevelLine.localPosition.z), (zoomLevelInPercent / 100f));
    }
}
