using IslandData.SystemData;
using Unity.Entities;
using UnityEngine;

namespace IslandEntities.Construction
{
    /// <summary>
    /// This class conatins all objects and settings that are used by the other constructors.
    /// </summary>
    public struct ConstructorData
    {
        public readonly EntityManager EntityManager { get; }

        public readonly Entity EntityHexPrefab { get; }
        public readonly Entity[] EntityBuildingPrefabs { get; }
        public readonly Entity EntityRegionBorderPrefab { get; }
        public readonly Entity EntityRelationshipPrefab { get; }
        public readonly Entity EntitySpringForceDebugPrefab { get; }
                
        public readonly Mesh RegionMesh { get; }
        public readonly Mesh RegionBorderMesh { get; }
        public readonly Mesh[] BuildingMeshes { get; }
                        
        public readonly int BuildingHeightMin { get; }
        public readonly int BuildingHeightMax { get; }
        public readonly int WorldSize { get; }
        public readonly bool DrawSprings { get; }

        public readonly System.Random RNG { get; }


        private readonly Material[] _regionMaterials;
        private readonly Material[] _regionBorderMaterials;
        private readonly Material[] _buildingMaterials;

        private readonly BlobAssetStore _blob; // We need a local ref of this blob asses (used for GameObject to Entity conversion), because we must dispose it on application end.


        /// <summary>
        /// Prepare all data needed for construction, i.e., generate materials based on the color palettes and convert prefabs to entities.
        /// </summary>
        public ConstructorData(ScriptableConstructorData cSettings, IslandSystem iData)
        {
            RNG = new System.Random(cSettings.Seed);

            // Generate materials.
            _regionMaterials = GenerateMaterialColorVariants(cSettings.RegionBaseMaterial, cSettings.RegionColorPalette);
            _regionBorderMaterials = GenerateMaterialColorVariants(cSettings.BorderBaseMaterial, cSettings.RegionColorPalette, 0.5f);
            _buildingMaterials = GenerateMaterialColorVariants(cSettings.BuildingBaseMaterial, cSettings.BuildingColorPalette);

            // Meshes.
            RegionMesh = cSettings.HexPrefab.GetComponent<MeshFilter>().sharedMesh;
            RegionBorderMesh = cSettings.RegionBorderPrefab.GetComponent<MeshFilter>().sharedMesh;
            BuildingMeshes = new Mesh[cSettings.BuildingPrefabs.Length];
            for (int i = 0; i < cSettings.BuildingPrefabs.Length; i++)
            {
                BuildingMeshes[i] = cSettings.BuildingPrefabs[i].GetComponent<MeshFilter>().sharedMesh;
            }

            // Project metrics.
            (BuildingHeightMin, BuildingHeightMax) = iData.GetBuildingHeightRange();
            WorldSize = iData.Islands.Count * 5; // TODO

            // Debug
            DrawSprings = cSettings.DrawSprings;

            // DOTS stuff.
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Convert GameObject Prefabs to Entity Prefabs.
            _blob = new BlobAssetStore();
            GameObjectConversionSettings conversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blob);
            EntityHexPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cSettings.HexPrefab, conversionSettings);
            EntityRegionBorderPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cSettings.RegionBorderPrefab, conversionSettings);
            EntityRelationshipPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cSettings.RelationshipPrefab, conversionSettings);
            EntitySpringForceDebugPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cSettings.SpringForceDebugPrefab, conversionSettings);
            EntityBuildingPrefabs = new Entity[cSettings.BuildingPrefabs.Length];
            for (int i = 0; i < cSettings.BuildingPrefabs.Length; i++)
            {
                EntityBuildingPrefabs[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(cSettings.BuildingPrefabs[i], conversionSettings);
            }
        }

        #region Getter

        public int GetRandomRegionMaterialID()
        {
            return RNG.Next(0, _regionMaterials.Length);
        }

        public Material GetRegionMaterial(int colorID)
        {
            return _regionMaterials[colorID];
        }

        public Material GetBuildingMaterial(int buildingType)
        {
            return _buildingMaterials[Mathf.Clamp(buildingType, 0, _buildingMaterials.Length - 1)];
        }

        public Material GetBorderMaterial(int colorID)
        {
            return _regionBorderMaterials[colorID];
        }


        public int GetBuildingHeightLevelsCount()
        {
            return BuildingMeshes.Length;
        }

        #endregion

        #region Helper_Functions

        private static Material[] GenerateMaterialColorVariants(Material baseMaterial, Color[] colorPalette, float darkenFactor = 0f)
        {
            Material[] materials = new Material[colorPalette.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(baseMaterial);
                materials[i].color = Color.Lerp(colorPalette[i], Color.black, darkenFactor);
            }

            return materials;
        }

        #endregion

        public void Dispose ()
        {
            if (_blob != null)
                _blob.Dispose();
        }
    }
}