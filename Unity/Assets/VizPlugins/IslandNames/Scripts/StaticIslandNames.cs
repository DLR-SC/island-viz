using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IslandData.SystemData;
using IslandEvents;
using IslandInterface.Selection;
using IslandLevelOfDetail;
using IslandPlugins;
using Unity.Entities;
using UnityEngine;
using IslandViz.Core;

namespace StaticIslandNamesComponent
{
    /// <summary>
    /// This component reacts to islands being selected and adds a name tag to the selected part of the island.
    /// </summary>
    public class StaticIslandNames : IslandPluginBase
    {
        // ################
        // Public
        // ################

        /// <summary>
        /// Instance of this plugin.
        /// </summary>
        public static StaticIslandNames Instance;

        [Header("Prefabs")]
        public GameObject IslandSelectionNamePrefab; // This prefab contains the StaticIslandName component, the UI Text, and the background UI Images.
        public GameObject IslandHighlightNamePrefab; // This prefab contains the StaticIslandName component, the UI Text, and the background UI Images.

        [Header("Settings")]
        [Tooltip("The minimal distance (in meter) between two StaticIslandNames before they are moved vertically for better readability.")]
        public float MinTextDistance; // Change this, e.g. when you changed the size of the IslandNamePrefab. Default: 0.25f
        [Tooltip("The vectical distance (in meter) between two StaticIslandNames when they are moved for better readability.")]
        public float VerticalTextOffset; // Change this, e.g. when you changed the size of the IslandNamePrefab. Default: 0.08f

        public float yOffset;

        // ################
        // Private
        // ################

        private bool heightIndexesDirty = false;

        public List<IIslandSystemElement> currentPinnedNames; // List with all pinned islands.

        Dictionary<IIslandSystemElement, StaticIslandName> currentNames; // Dictionary connecting the selected island transform with the StaticIslandName tag.
        Dictionary<IIslandSystemElement, StaticIslandName> currentHiddenNames;


        public enum NameType
        {
            Select,
            Highlight
        }

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
            currentNames = new Dictionary<IIslandSystemElement, StaticIslandName>();
            currentHiddenNames = new Dictionary<IIslandSystemElement, StaticIslandName>();
            currentPinnedNames = new List<IIslandSystemElement>();

            // Subscribe to events.
            //IslandInteraction.OnSelect += OnSelect;
            InteractionEvents.OnIslandElementHighlighted += OnHighlight;
            ZoomLevelEvents.OnZoomLevelChanged += OnZoomLevelChanged;
            //IslandVizVisualization.Instance.OnIslandVisible += UnhideStaticName;
            //IslandVizVisualization.Instance.OnIslandInvisible += HideStaticName;

            StartCoroutine(HightIndexRecalculation());

            yield return null;
        }
        #endregion


        // ################
        // Selection Event Handling
        // ################

        #region OnSelectOrHighlight Event Handling

        public void OnHighlight (IIslandSystemElement islandElement, bool selected, int handID)
        {
            if (islandElement == null)
                return;

            if (selected)
                TryCreateStaticName(islandElement, NameType.Highlight, handID);
            else
                TryRemoveStaticName(islandElement, NameType.Highlight, handID);
        }

        public void OnSelect(IIslandSystemElement oSGiElement, bool selected)
        {
            if (selected)
                TryCreateStaticName(oSGiElement, NameType.Select);
            else
                TryRemoveStaticName(oSGiElement, NameType.Select);
        }

        public void OnZoomLevelChanged (IslandLOD newLevel, IslandLOD prevLevel)
        {
            foreach (var item in currentNames)
            {

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
        private void TryCreateStaticName(IIslandSystemElement oSGiElement, NameType selectionType, int handID = -1)
        {
            // The name with the selection type already exists.
            if (currentNames.ContainsKey(oSGiElement) && currentNames[oSGiElement]._selectionType == selectionType)
            {
                return;
            }

            // Name was highlighted and is now selected -> Remove Highlight-Text.
            if (currentNames.ContainsKey(oSGiElement) && selectionType == StaticIslandNames.NameType.Select && currentNames[oSGiElement]._selectionType == StaticIslandNames.NameType.Highlight)
            {
                RemoveStaticName(oSGiElement);
            }

            if (!currentNames.ContainsKey(oSGiElement))
            {
                GameObject islandName;
                if (selectionType == StaticIslandNames.NameType.Highlight)
                {
                    islandName = Instantiate(IslandHighlightNamePrefab, transform);
                }
                else
                {
                    islandName = Instantiate(IslandSelectionNamePrefab, transform);
                }
                StaticIslandName staticIslandName = islandName.GetComponent<StaticIslandName>();
                currentNames.Add(oSGiElement, staticIslandName);
                staticIslandName.Init(oSGiElement, selectionType, handID, currentPinnedNames.Contains(oSGiElement));

                RecalculateAllHeightIndexes();

                //Debug.Log(currentNames.Count);
            }
        }
        
        public void TryRemoveStaticName(IIslandSystemElement oSGiElement, NameType nameType, int handID = -1)
        {
            // No static name with this oSGiElement or nameType exists.
            if (!currentNames.ContainsKey(oSGiElement) || nameType != currentNames[oSGiElement]._selectionType)
            {
                return;
            }
            // The oSGiElement has already a Selection-Text -> No action on De-highlight.
            if (nameType == NameType.Highlight && currentNames[oSGiElement]._selectionType == NameType.Select)
            {
                return;
            }
            // The islandName is pinned -> We can not de-heighlight it.
            if (nameType == NameType.Highlight && currentNames[oSGiElement].Pin)
            {
                return;
            }
            // The islandName is selected and pinned -> we unselect it und highligt it.
            if (nameType == NameType.Select && currentNames[oSGiElement]._selectionType == NameType.Select && currentNames[oSGiElement].Pin)
            {
                RemoveStaticName(oSGiElement);
                IslandInteraction.Instance.TryHighlight(oSGiElement);
                return;
            }

            // We can remove the name.
            RemoveStaticName(oSGiElement);
        }

        /// <summary>
        /// By destroying the StaticIslandName GameObject, the island name disappears.
        /// </summary>
        /// <param name="target">The target that was deselected.</param>
        public void RemoveStaticName(IIslandSystemElement oSGiElement)
        {
            if (!currentNames.ContainsKey(oSGiElement))
            {
                return;
            }
            RemoveStaticName(currentNames[oSGiElement]);
        }

        public void RemoveStaticName(StaticIslandName islandName)
        {
            currentNames.Remove(islandName._islandElement);
            Destroy(islandName.gameObject);

            //else if (currentHiddenNames.ContainsKey(oSGiElement))
            //{
            //    Destroy(currentHiddenNames[oSGiElement].gameObject);
            //    currentHiddenNames.Remove(oSGiElement);
            //}

            RecalculateAllHeightIndexes();
        }

        private void HideStaticName (Entity island)
        {
            //StaticIslandName islandName;
            //if (currentNames.TryGetValue(island.transform, out islandName))
            //{
            //    currentHiddenNames.Add(island.transform, islandName);
            //    currentNames.Remove(island.transform);

            //    islandName.gameObject.SetActive(false);

            //    RecalculateAllHeightIndexes();
            //}
        }

        private void UnhideStaticName (Entity island)
        {
            //StaticIslandName islandName;
            //if (currentHiddenNames.TryGetValue(island.transform, out islandName))
            //{
            //    currentNames.Add(island.transform, islandName);
            //    currentHiddenNames.Remove(island.transform);

            //    islandName.gameObject.SetActive(true);

            //    RecalculateAllHeightIndexes();
            //}
        }

        #endregion


        // ################
        // Height Index
        // ################

        #region Height Index

        /// <summary>
        /// Called by the OnVisualizationScaleChanged event and recalculates the height indexes of all current island names.
        /// </summary>
        private void RecalculateAllHeightIndexes(float deltasize = 0f) // TODO Performance wise this seems to be fine, but check again in the future.
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
            var children = GetComponentsInChildren<Transform>(true).ToList();
            children.Remove(transform);
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
            List<StaticIslandName> allEntriesInRadius = GetAllEntriesInRadius(MinTextDistance, staticIslandName);

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




        public void PinName (IIslandSystemElement oSGiElement)
        {
            if (!currentPinnedNames.Contains(oSGiElement))
            {
                currentPinnedNames.Add(oSGiElement);
            }
        }

        public void UnPinName (IIslandSystemElement oSGiElement)
        {
            if (currentPinnedNames.Contains(oSGiElement))
            {
                currentPinnedNames.Remove(oSGiElement);
            }
        }



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
        private List<StaticIslandName> GetAllEntriesInRadius(float radius, StaticIslandName origin)
        {
            List<StaticIslandName> result = new List<StaticIslandName>();

            foreach (var item in currentNames)
            {
                if (item.Value != origin && Vector3.Distance(item.Value.transform.position, origin.transform.position) <= radius)
                {
                    result.Add(item.Value);
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
        private bool EntriesContainHeightIndex(List<StaticIslandName> entries, int heightIndex)
        {
            foreach (var item in entries)
            {
                if (item.GetHeightIndex() == heightIndex)
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
        #endregion
    }
}

