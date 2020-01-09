using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandNameSelectionName : MonoBehaviour
{
    public Text Name;
    private IslandGO island;


    public void Init (IslandGO island)
    {
        Name.text = island.name;
    }
}
