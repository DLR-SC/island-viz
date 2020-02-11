using OsgiViz;
using OsgiViz.Core;
using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public float MinTextDistance = 0.25f; // Change this, e.g. when you changed the size of the IslandNamePrefab.
        [Tooltip("The vectical distance (in meter) between two StaticIslandNames when they are moved for better readability.")]
        public float VerticalTextOffset = 0.08f; // Change this, e.g. when you changed the size of the IslandNamePrefab.

        // ################
        // Private
        // ################

        private bool heightIndexesDirty = false;

        private Transform StaticNameParent; // The transform of the initiated IslandNameParentPrefab. This will be the parent of every island name.

        Dictionary<Transform, StaticIslandName> currentNames; // Dictionary connecting the selected island transform with the StaticIslandName tag.
        Dictionary<Transform, StaticIslandName> currentHiddenNames;



        private void Start() { } // When this has no Start method, you will not be able to disable this in the editor.


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

            StartCoroutine(HightIndexRecalculation());

            yield return null;
        }
        #endregion

        public IEnumerator UpdateStaticNames(List<Transform> newIslands, List<Transform> deletedIslands)
        {
            foreach(Transform t in deletedIslands)
            {
                RemoveStaticName(t);
            }
            yield return null;
            if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far) && currentNames.Count > 1)
            {
                //TODO new static names shall be shown, if other static names are shown - this part doesn't word
                foreach(Transform t in newIslands)
                {
                    CreateStaticName(t, IslandVizInteraction.SelectionType.Highlight);
                }
            }
        }


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
            if (island == null)
            {
                return;
            }
            if (!island.gameObject.activeSelf)
            {
                return;
            }
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
                //IslandVizInteraction.Instance.OnRegionSelect(building.transform.parent.GetComponent<Region>(), IslandVizInteraction.SelectionType.Highlight, true);
                CreateStaticName(building.transform, selectionType);
            }
            else
            {
                //RemoveStaticName(building.transform.parent); // Also show the name of the Region.
                //IslandVizInteraction.Instance.OnRegionSelect(building.transform.parent.GetComponent<Region>(), IslandVizInteraction.SelectionType.Highlight, false);
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
                islandName.transform.SetParent(StaticNameParent);
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
                Destroy(currentHiddenNames[target].gameObject);
                currentHiddenNames.Remove(target);
            }

            RecalculateAllHeightIndexes();
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
            heightIndexesDirty = true;
        }

        IEnumerator HightIndexRecalculation ()
        {
            while (true)
            {
                if (heightIndexesDirty)
                {
                    heightIndexesDirty = false;

                    for (int i = 0; i < currentNames.Count; i++)
                    {
                        if (i < currentNames.Count && currentNames.ElementAt(i).Value != null)
                        {
                            currentNames.ElementAt(i).Value.SetHeightIndex(GetHeightIndex(currentNames.ElementAt(i).Value));
                            if (i != 0 && i % 10 == 0)
                            {
                                yield return null;
                            }
                        }
                    }
                    yield return null;
                    yield return ReorderChildren();
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        IEnumerator ReorderChildren ()
        {
            // Realligne the children so the ui elements are culled in the right order.
            var children = StaticNameParent.GetComponentsInChildren<Transform>(true).ToList();
            children.Remove(StaticNameParent.transform);
            children.Sort(Compare);
            for (int i = 0; i < children.Count; i++)
            {
                if (i < children.Count && children[i] != null)
                {
                    children[i].SetSiblingIndex(i);
                    if (i != 0 && i % 50 == 0)
                    {
                        yield return null;
                    }
                }
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


        private static int Compare(Transform lhs, Transform rhs)
        {
            if (lhs == rhs) return 0;
            var test = rhs.gameObject.activeInHierarchy.CompareTo(lhs.gameObject.activeInHierarchy);
            if (test != 0) return test;
            if (Vector3.Distance(lhs.position, Camera.main.transform.position) < Vector3.Distance(rhs.position, Camera.main.transform.position)) return 1;
            if (Vector3.Distance(lhs.position, Camera.main.transform.position) > Vector3.Distance(rhs.position, Camera.main.transform.position)) return -1;
            return 0;
        }
       
        public int GetCurrentNameCount()
        {
            return currentNames.Count;
        }
        public Transform GetFirstCurrentNameTransform()
        {
            return currentNames.Keys.ToList<Transform>()[0];
        }

        #endregion
    }
}

