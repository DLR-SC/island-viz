using OsgiViz;
using OsgiViz.Core;
using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StaticIslandNamesComponent
{
    /// <summary>
    /// This component reacts to islands being selected and adds a name tag to the selected part of the island.
    /// </summary>
    public class StaticIslandNames : AdditionalIslandVizComponent
    {
        // ################
        // Public
        // ################

        public static StaticIslandNames Instance; // Instance of this class.

        [Header("Prefabs")]
        public GameObject IslandNameParentPrefab; // This prefab contains the UI canvas, in which the IslandNamePrefabs are initiated.
        public GameObject IslandNamePrefab; // This prefab contains the StaticIslandName component, the UI Text, and the background UI Images.

        [Header("Settings")]
        [Tooltip("The minimal distance (in meter) between two StaticIslandNames before they are moved vertically for better readability.")]
        public float MinTextDistance = 0.4f; // Change this, e.g. when you changed the size of the IslandNamePrefab.
        [Tooltip("The vectical distance (in meter) between two StaticIslandNames when they are moved for better readability.")]
        public float VerticalTextOffset = 0.12f; // Change this, e.g. when you changed the size of the IslandNamePrefab.

        // ################
        // Private
        // ################

        private Transform StaticNameParent; // The transform of the initiated IslandNameParentPrefab. This will be the parent of every island name.

        Dictionary<Transform, StaticIslandName> currentNames; // Dictionary connecting the selected island transform with the StaticIslandName tag.
        Dictionary<Transform, StaticIslandName> currentHiddenNames;




        // ################
        // Initiation
        // ################

        #region Initiation

        /// <summary>
        /// Initialize this input component. 
        /// This method is called by the IslandVizInteraction class.
        /// </summary>
        public override IEnumerator Init()
        {
            Instance = this;
            StaticNameParent = ((GameObject)Instantiate(IslandNameParentPrefab, Vector3.zero, Quaternion.identity)).transform;
            currentNames = new Dictionary<Transform, StaticIslandName>();
            currentHiddenNames = new Dictionary<Transform, StaticIslandName>();

            // Subscribe to events.
            IslandVizInteraction.Instance.OnIslandSelect += OnIslandSelection;
            IslandVizInteraction.Instance.OnRegionSelect += OnRegionSelection;
            IslandVizInteraction.Instance.OnBuildingSelect += OnBuildingSelection;
            IslandVizVisualization.Instance.OnVisualizationScaleChanged += RecalculateAllHeightIndexes;
            IslandVizVisualization.Instance.OnIslandVisible += UnhideStaticName;
            IslandVizVisualization.Instance.OnIslandInvisible += HideStaticName;

            yield return null;
        }
        #endregion


        // ################
        // Selection Event Handling
        // ################

        #region Selection Event Handling

        /// <summary>
        /// Called by the OnIslandSelect event when an island is either selected or deselected and 
        /// either creates or removes the name if this island.
        /// </summary>
        /// <param name="island">The island that was selected.</param>
        /// <param name="selected">Wether the island was selected or deselected.</param>
        private void OnIslandSelection(IslandGO island, IslandVizInteraction.SelectionType selectionType, bool selected)
        {
            if (selectionType == IslandVizInteraction.SelectionType.Select)
            {
                if (selected)
                {
                    CreateStaticName(island.transform, selectionType);
                }
                else
                {
                    RemoveStaticName(island.transform);
                }
            }
            else if (selectionType == IslandVizInteraction.SelectionType.Highlight && !island.Selected)
            {
                if (selected)
                {
                    CreateStaticName(island.transform, selectionType);
                }
                else
                {
                    RemoveStaticName(island.transform);
                }
            }
        }

        /// <summary>
        /// Called by the OnRegionSelect event when a region is either selected or deselected and 
        /// either creates or removes the name if this region.
        /// </summary>
        /// <param name="region">The region that was selected.</param>
        /// <param name="selected">Wether the region was selected or deselected.</param>
        private void OnRegionSelection(Region region, IslandVizInteraction.SelectionType selectionType, bool selected)
        {
            if (selected)
            {
                CreateStaticName(region.transform, selectionType);
            }
            else
            {
                RemoveStaticName(region.transform);
            }
        }

        /// <summary>
        /// Called by the OnBuildingSelect event when a building is either selected or deselected and 
        /// either creates or removes the name if this building.
        /// </summary>
        /// <param name="building">The building that was selected.</param>
        /// <param name="selected">Wether the building was selected or deselected.</param>
        private void OnBuildingSelection(Building building, IslandVizInteraction.SelectionType selectionType, bool selected)
        {
            if (selected)
            {
                //CreateStaticName(building.transform.parent, selectionType); // Also show the name of the Region.
                IslandVizInteraction.Instance.OnRegionSelect(building.transform.parent.GetComponent<Region>(), IslandVizInteraction.SelectionType.Highlight, true);
                CreateStaticName(building.transform, selectionType);
            }
            else
            {
                //RemoveStaticName(building.transform.parent); // Also show the name of the Region.
                IslandVizInteraction.Instance.OnRegionSelect(building.transform.parent.GetComponent<Region>(), IslandVizInteraction.SelectionType.Highlight, false);
                RemoveStaticName(building.transform);
            }
        }

        #endregion


        // ################
        // Name Creation and Removal
        // ################

        #region Name Creation and Removal

        /// <summary>
        /// Creates a name by instantiating an IslandNamePrefab and initiating the StaticIslandName component.
        /// Called when an island (or a region or a building) was selected. 
        /// </summary>
        /// <param name="target">The target Transform the name should be attached to.</param>
        private void CreateStaticName(Transform target, IslandVizInteraction.SelectionType selectionType)
        {
            if (!currentNames.ContainsKey(target) && !currentNames.ContainsKey(target)) 
            {
                GameObject islandName = (GameObject)Instantiate(IslandNamePrefab);
                islandName.transform.parent = StaticNameParent;
                islandName.transform.position = new Vector3(target.position.x, 0f, target.position.z);
                StaticIslandName staticIslandName = islandName.GetComponent<StaticIslandName>();

                currentNames.Add(target, staticIslandName);

                staticIslandName.Init(target, target.name, selectionType);
            }
        }
        
        /// <summary>
        /// By destroying the StaticIslandName GameObject, the island name disappears.
        /// </summary>
        /// <param name="target">The target that was deselected.</param>
        private void RemoveStaticName(Transform target)
        {
            if (currentNames.ContainsKey(target))
            {
                Destroy(currentNames[target].gameObject);
                currentNames.Remove(target);
            }
            else if (currentHiddenNames.ContainsKey(target))
            {
                Destroy(currentNames[target].gameObject);
                currentNames.Remove(target);
            }
        }

        private void HideStaticName (IslandGO island)
        {
            StaticIslandName islandName;
            if (currentNames.TryGetValue(island.transform, out islandName))
            {
                currentHiddenNames.Add(island.transform, islandName);
                currentNames.Remove(island.transform);

                islandName.gameObject.SetActive(false);

                RecalculateAllHeightIndexes();
            }
        }

        private void UnhideStaticName (IslandGO island)
        {
            StaticIslandName islandName;
            if (currentHiddenNames.TryGetValue(island.transform, out islandName))
            {
                currentNames.Add(island.transform, islandName);
                currentHiddenNames.Remove(island.transform);

                islandName.gameObject.SetActive(true);

                RecalculateAllHeightIndexes();
            }
        }

        #endregion


        // ################
        // Height Index
        // ################

        #region Height Index

        /// <summary>
        /// Called by the OnVisualizationScaleChanged event and recalculates the height indexes of all current island names.
        /// </summary>
        private void RecalculateAllHeightIndexes() // TODO Performance wise this seems to be fine, but check again in the future.
        {
            foreach (var item in currentNames)
            {
                item.Value.SetHeightIndex(GetHeightIndex(item.Value));
            }
        }

        /// <summary>
        /// Returns the lowest possible height index of an island name by compairing the height indexes of all island names closer than the MinTextDistance.  
        /// </summary>
        /// <param name="staticIslandName">The lowest possible height index for this StaticIslandName (This island name is exluded in the comparisons).</param>
        /// <returns>Returns a height index.</returns>
        public int GetHeightIndex(StaticIslandName staticIslandName)
        {
            List<KeyValuePair<Transform, StaticIslandName>> allEntriesInRadius = GetAllEntriesInRadius(MinTextDistance, staticIslandName);

            if (allEntriesInRadius == null || allEntriesInRadius.Count == 0) // No island names closer than the MinTextDistance.
            {
                return 1;
            }
            else
            {
                for (int heightIndex = 1; heightIndex <= allEntriesInRadius.Count; heightIndex++) // Go through all possible numbers to see if there is an empty spot.
                {
                    if (!EntriesContainHeightIndex(allEntriesInRadius, heightIndex))
                    {
                        return heightIndex;
                    }
                }
                return allEntriesInRadius.Count + 1; // Just add on top of the other names.
            }
        }

        #endregion


        // ################
        // Helper Functions
        // ################

        #region Helper Functions

        /// <summary>
        /// Returns a list of all current dictionary entries (island names + targets) that are in a certain radius around an origin.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="origin">The island name in the center.</param>
        /// <returns>Returns a list of dictionary entries.</returns>
        private List<KeyValuePair<Transform, StaticIslandName>> GetAllEntriesInRadius(float radius, StaticIslandName origin)
        {
            List<KeyValuePair<Transform, StaticIslandName>> result = new List<KeyValuePair<Transform, StaticIslandName>>();

            foreach (var item in currentNames)
            {
                if (item.Value != origin && Vector3.Distance(item.Key.position, origin.GetTarget().position) <= radius)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Goes through a list of dictionary entries (island names + targets) and checks if one of the StaticIslandName has this height index.
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="heightIndex"></param>
        /// <returns>Return true when the entries contain this height index.</returns>
        private bool EntriesContainHeightIndex(List<KeyValuePair<Transform, StaticIslandName>> entries, int heightIndex)
        {
            foreach (var item in entries)
            {
                if (item.Value.GetHeightIndex() == heightIndex)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}

