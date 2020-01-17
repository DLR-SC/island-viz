using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProvider_Script : MonoBehaviour
{
    [SerializeField]
    private GameObject building0_prefab;
    [SerializeField]
    private GameObject building1_prefab;
    [SerializeField]
    private GameObject building2_prefab;
    [SerializeField]
    private GameObject building3_prefab;
    [SerializeField]
    private GameObject building4_prefab;
    [SerializeField]
    private GameObject building5_prefab;
    [SerializeField]
    private GameObject building6_prefab;
    [SerializeField]
    private GameObject building7_prefab;

    private int nrOfLevels = 8;
    private float levelRange;


    public void Initialise(long maxLoc)
    {
        levelRange = Mathf.Sqrt(maxLoc) / nrOfLevels;
    }

    public List<object> GetBuildingPrefabForLoc(long loc)
    {
        int level = Mathf.Min(Mathf.FloorToInt(Mathf.Sqrt(loc) / levelRange), nrOfLevels - 1);
        return new List<object> { GetPrefabOf(level), level };
    }

    private GameObject GetPrefabOf(int index)
    {
        switch (index)
        {
            case 0:
                return building0_prefab;
            case 1:
                return building1_prefab;
            case 2:
                return building2_prefab;
            case 3:
                return building3_prefab;
            case 4:
                return building4_prefab;
            case 5:
                return building5_prefab;
            case 6:
                return building6_prefab;
            case 7:
                return building7_prefab;
            default:
                return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
       //not needed 
    }

    // Update is called once per frame
    void Update()
    {
        //not needed
    }
}
