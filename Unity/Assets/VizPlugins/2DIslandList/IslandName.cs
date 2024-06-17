using System.Collections;
using System.Collections.Generic;
using IslandData.SystemData;
using UnityEngine;
using UnityEngine.UI;

public class IslandName : MonoBehaviour
{
    public Text Name;

    private Island _island;
    private IslandList2D _parentList;

    public void SetValues (Island island, IslandList2D parentList)
    {
        Name.text = island.Name;
        _island = island;
        _parentList = parentList;
    }

    public void IslandNameHighlighted (bool active)
    {
        //_parentList.OnIslandNameHighlighted(_island, active);
    }

    public void IslandNameSelected()
    {
        _parentList.OnIslandNameSelected(_island);
    }
}
