using System.Collections;
using IslandData.SystemData;
using IslandEntities.IslandComponents;
using IslandEvents;
using IslandSettings;
using IslandVis.ECS_Components;
using IslandViz.Core;
using Unity.Entities;

namespace IslandEntities.Construction
{
    /// <summary>
    /// This class creates/removes relationship entites (the yellow/red arrows indicating dependencies between island elements).
    /// </summary>
    public class RelationshipConstructor
    {
        /// <summary>
        /// This contains data (entities, materials, meshes, default values, and rng) used by all constructors.
        /// </summary>
        private static ConstructorData _constructionData;


        public static IEnumerator Initiate(ConstructorData constructionData)
        {
            _constructionData = constructionData;

            InteractionEvents.OnIslandElementHighlighted += ShowDependenciesOnHighlight;
            //VizEvents.OnIslandElementSelected += ShowDependenciesOnHighlight;

            yield return null;
        }

        private static void ShowDependenciesOnHighlight(IIslandSystemElement islandElement, bool active, int handID = 0) // 
        {
            if (!VizSettings.ShowRelationshipsOnHighlight)
                return;

            // TODO
            if (RelationshipToggler.Active)
                return;

            if (islandElement == null)
                return;

            if (active)
            {
                var elementType = islandElement switch
                {
                    Island => typeof(Island),
                    Region => typeof(Region),
                    _ => typeof(Building)
                };

                float scale = 10f; // GetRelationshipSizeByType(islandElement);

                // Import relationships
                foreach (var import in islandElement.Imports)
                {
                    if (import.Target.Entity == Entity.Null)
                        continue;

                    if (import.Target.GetType() == elementType)
                        InstantiateRelationshipEntity(islandElement.Entity, import.Target.Entity, import.Weight * scale);
                }

                // Export relationships
                foreach (var export in islandElement.Exports)
                {
                    if (export.Target.Entity == Entity.Null)
                        continue;

                    if (export.Target.GetType() == elementType)
                    {
                        InstantiateRelationshipEntity(export.Target.Entity, islandElement.Entity, export.Weight * scale);
                    }   
                }
            }
            else
            {
                RemoveAllRelationships(); // TODO
            }
        }


        private static float GetRelationshipSizeByType (IIslandSystemElement element)
        {
            return element switch
            {
                Island => 20f,
                Region => 15f,
                Building => 10f,
                _ => 10f
            };
        }


        //public void HighlightDependencies(IslandDataElement oSGiElement, bool active)
        //{
        //    if (active)
        //        HighlightAllRelationships(oSGiElement);
        //    else
        //    {
        //        RemoveAllRelationships();
        //        DehighlightAllRelationships(oSGiElement);
        //    }
        //}


        public static void SpawnAllRelationships()
        {
            foreach (var element in VizManager.IslandData.AllElements)
            {
                foreach (var import in element.Value.Imports)
                {
                    // if (element is Island && import.Target is Island)
                    // {
                        InstantiateRelationshipEntity(element.Value.Entity, import.Target.Entity, import.Weight * 2f);
                    // }
                }
            }
        }

        /// <summary>
        /// Display all BUNDLE-IMPORTS->BUNDLE relationships.
        /// </summary>
        /// <param name="osgiProject">The current osgi project.</param>
        /// <returns></returns>
        //public static IEnumerator SpawnAllRelationships (Neo4jData neo4JData)
        //{

        //}

        ///// <summary>
        ///// Display all PACKAGE-IMPORTS-BUNDLE relationships.
        ///// </summary>
        ///// <param name="osgiProject">The current osgi project.</param>
        ///// <returns></returns>
        //public static IEnumerator SpawnAllPackageToBundleRelationships(Project osgiProject)
        //{
        //    foreach (Package package in osgiProject.Packages)
        //    {
        //        foreach (Bundle importBundle in package.Imports)
        //        {
        //            if (package.Entities.Count == 0 || importBundle.Entity == Entity.Null)
        //            {
        //                continue;
        //            }
        //            InstantiateRelationshipEntity(package.Entities[0], importBundle.Entity, 0.25f);
        //        }
        //    }
        //    yield return null;
        //}

        ///// <summary>
        ///// Display all PACKAGE-IMPORTS-BUNDLE relationships.
        ///// </summary>
        ///// <param name="osgiProject">The current osgi project.</param>
        ///// <returns></returns>
        //public static void SpawnRelationships(OSGiElement oSGiElement, RelationshipType type)
        //{
        //    if (oSGiElement is Bundle) // TODO
        //    {
        //        Bundle bundle = oSGiElement as Bundle;

        //        if (bundle.Entity == Entity.Null)
        //        {
        //            return;
        //        }

        //        RemoveAllRelationships(); // TODO 

        //        if (type == RelationshipType.Import || type == RelationshipType.Both)
        //        {
        //            SpawnBundleImportRelationships(bundle);
        //        }
        //        if (type == RelationshipType.Export || type == RelationshipType.Both)
        //        {
        //            SpawnBundleExportRelationships(bundle);
        //        }
        //    }
        //}

        //private static void SpawnBundleImportRelationships(Bundle bundle)
        //{
        //    foreach (var importBundle in bundle.Imports)
        //    {
        //        if (importBundle.Key.Entity == Entity.Null)
        //        {
        //            continue;
        //        }
        //        InstantiateRelationshipEntity(bundle.Entity, importBundle.Key.Entity, importBundle.Value);
        //        //OSGiInteraction.Instance.Highlight(importBundle.Key);
        //    }
        //}

        //private static void SpawnBundleExportRelationships(Bundle bundle)
        //{
        //    foreach (var exportBundle in bundle.Exports)
        //    {
        //        if (exportBundle.Entity == Entity.Null)
        //        {
        //            continue;
        //        }
        //        InstantiateRelationshipEntity(bundle.Entity, exportBundle.Entity, 0.25f);
        //        //OSGiInteraction.Instance.Highlight(exportBundle);
        //    }
        //}


        /// <summary>
        /// Display a relationships between two entities.
        /// </summary>
        /// <param name="start">Start-node entity of the relationship.</param>
        /// <param name="end">End-node entity of the relationship.</param>
        //public static void SpawnRelationship(Entity start, Entity end, float width = 0.5f)
        //{
        //    if (start == Entity.Null || end == Entity.Null)
        //        return;

        //    InstantiateRelationshipEntity(start, end, width);
        //}



        #region ECS

        /// <summary>
        /// Instantiate a new relationship entity. Note: the transformation of the relationship is updated by the RelationshipTransformUpdateSystem.cs.
        /// </summary>
        /// <param name="start">Start-node entity of the relationship.</param>
        /// <param name="end">End-node entity of the relationship.</param>
        /// <param name="width"></param>
        /// <returns>Returns the instantiated entity.</returns>
        private static void InstantiateRelationshipEntity(Entity start, Entity end, float width)
        {
            var entity = _constructionData.EntityManager.Instantiate(_constructionData.EntityRelationshipPrefab);
            _constructionData.EntityManager.AddComponent<RelationshipTag>(entity);
            _constructionData.EntityManager.AddComponentData(entity, new RelationshipData
            {
                Start = start,
                End = end,
                Width = width
            });
        }

        /// <summary>
        /// Adds a DestroyTag to all existing entitites with a RelationshipTag, i.e. destroys all relationships.
        /// </summary>
        public static void RemoveAllRelationships ()
        {
            var entityQuery = _constructionData.EntityManager.CreateEntityQuery(typeof(RelationshipTag));
            _constructionData.EntityManager.AddComponent<DestroyTag>(entityQuery);
        }

        //public static void DehighlightAllRelationships (OSGiElement oSGiElement)
        //{
        //    if (oSGiElement is Bundle bundle)
        //    {
        //        //foreach (var exportBundle in bundle.Exports)
        //        //{
        //        //    if (exportBundle.Entity == Entity.Null)
        //        //        continue;
        //        //    OSGiInteraction.Instance.Dehighlight(exportBundle);
        //        //}
        //        foreach (var importBundle in bundle.Imports)
        //        {
        //            if (importBundle.Key.Entity == Entity.Null)
        //                continue;
        //            IslandInteraction.Instance.Dehighlight(importBundle.Key);
        //        }
        //    }
        //}

        //public static void HighlightAllRelationships(OSGiElement oSGiElement)
        //{
        //    if (oSGiElement is Bundle bundle)
        //    {
        //        //foreach (var exportBundle in bundle.Exports)
        //        //{
        //        //    if (exportBundle.Entity == Entity.Null)
        //        //        continue;
        //        //    OSGiInteraction.Instance.Highlight(exportBundle);
        //        //}
        //        foreach (var importBundle in bundle.Imports)
        //        {
        //            if (importBundle.Key.Entity == Entity.Null)
        //                continue;
        //            IslandInteraction.Instance.Highlight(importBundle.Key);
        //        }
        //    }
        //}

        #endregion
    }
}
