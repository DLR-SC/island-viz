using UnityEngine;

namespace IslandEntities.Construction
{
    /// <summary>
    /// This <see cref="ScriptableObject"/> contains settings (prefabs, color palletts, and base materials) used for visualization construction.
    /// </summary>
    [CreateAssetMenu(fileName = "ConstructorSettings", menuName = "ScriptableObjects/Construction/ConstructorSettings", order = 1)]
    public class ScriptableConstructorData : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject HexPrefab; // Region mesh prefab.
        public GameObject[] BuildingPrefabs; // Array containing building prefabs. (One model for each storey atm.) This array must be sorted: [lowest building, ..., highest building]
        public GameObject RelationshipPrefab; // Relationship arrow prefab.
        public GameObject RegionBorderPrefab;

        [Header("Colors")]
        public Color[] RegionColorPalette; // For each color, a region material is created.
        public Color[] BuildingColorPalette; // For each color, a building material is created.

        [Header("Materials")]
        public Material RegionBaseMaterial; // Base material used for material generation.
        public Material BuildingBaseMaterial; // Base material used for material generation.
        public Material BorderBaseMaterial; // Base material used for material generation.
        public Material HighlightMaterial; // TODO Move

        [Header("Procerual Generation")]
        public int Seed; // Seed used for procedural generation (island position, layout, colors).

        [Header("FDL Debug")]
        public bool DrawSprings; // If enabled, the spring forces a rendered using the SpringForceDebugPrefab.
        public GameObject SpringForceDebugPrefab;
    }
}