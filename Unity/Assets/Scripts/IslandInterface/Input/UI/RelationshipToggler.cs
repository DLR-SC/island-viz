using IslandEntities.Construction;
using UnityEngine;

public class RelationshipToggler : MonoBehaviour
{
    public static bool Active;

    public void Toggle()
    {
        Active = !Active;

        if (Active)
        {
            RelationshipConstructor.SpawnAllRelationships();
        }
        if (!Active)
        {
            RelationshipConstructor.RemoveAllRelationships();
        }
    }



}
