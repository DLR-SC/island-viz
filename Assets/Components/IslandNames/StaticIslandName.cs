using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OsgiViz.Core;
using OsgiViz.Unity.Island;

public class StaticIslandName : MonoBehaviour
{
    public GameObject NameParent;
    public Text Name;
    public Text Line;

    public AlwaysLookAtTarget AlwaysLookAtTarget;


    private Transform target;
    private float heightIndex = 1f;    

    private bool isIsland;
    private float yPosition;

    private bool initiated = false;



    public void Init (Transform target, string name)
    {
        Debug.Log("StaticIslandName Init");

        AlwaysLookAtTarget.Target = Camera.main.transform;
        Name.text = name;
        this.target = target;

        heightIndex = StaticIslandNames.Instance.GetHeightIndex(this);

        isIsland = target.GetComponent<IslandGO>() != null;

        initiated = true;
    }


    private void FixedUpdate()
    {
        if (!initiated)
            return;

        if (target.gameObject.activeSelf) // && target.GetComponent<Collider>().enabled
        {
            if (IslandVizVisualization.Instance.CurrentZoomLevel != ZoomLevel.Near)
            {
                yPosition = GlobalVar.hologramTableHeight + 0.075f + heightIndex * 0.15f;
                transform.position = new Vector3(target.position.x, yPosition, target.position.z);
            }
            else
            {
                if (isIsland)
                {
                    transform.position = new Vector3(target.position.x, GlobalVar.hologramTableHeight + 0.2f + GlobalVar.CurrentZoom * 2f, target.position.z);
                }
                else
                {
                    yPosition = GlobalVar.hologramTableHeight + 0.2f + GlobalVar.CurrentZoom + heightIndex * 0.15f;
                    transform.position = new Vector3(target.position.x, yPosition, target.position.z);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public void DisableText ()
    {
        NameParent.SetActive(false);
        Line.gameObject.SetActive(false);
    }

    public void EnableText ()
    {
        NameParent.SetActive(true);
        Line.gameObject.SetActive(true);
    }



    public Transform GetTarget ()
    {
        return target;
    }
    public float GetHeightIndex ()
    {
        return heightIndex;
    }
    public void SetHeightIndex (int index)
    {
        heightIndex = index;
    }
}
