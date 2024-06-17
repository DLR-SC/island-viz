using System;
using System.Collections.Generic;
using IslandData.SystemData;
using IslandEntities.IslandComponents;
using IslandEvents;
using IslandInterface.Input;
using IslandLevelOfDetail;
using IslandViz.Core;
using Unity.Entities;
using UnityEngine;

namespace IslandInterface.Selection
{
    /// <summary>
    /// This class listens to entity raycast & selection events, finds the corresponding island system element and
    /// throws a island highlight or selection event.
    /// It can also be called from other classes to highlight/select an island element.
    /// </summary>
    public class IslandInteraction : MonoBehaviour
    {
        /// <summary>
        /// Instance of this class.
        /// </summary>
        public static IslandInteraction Instance { private set; get; }

        /// <summary>
        /// Unity ECS controller used to handle component requests.
        /// </summary>
        private EntityManager _entityManager;

        /// <summary>
        /// The island elements that are currently highlighted by hand (one per hand).
        /// </summary>
        private IIslandSystemElement[] _highlightedByHands;

        /// <summary>
        /// The island elements that are currently highlighted by the system.
        /// </summary>
        private List<IIslandSystemElement> _highlightedBySystem;



        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _highlightedByHands = new IIslandSystemElement[PlayerManager.Instance.Hands.Length];
            _highlightedBySystem = new List<IIslandSystemElement>();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            InteractionEvents.OnEntityHitChanged += HandleEntityRaycastEvent;
            InteractionEvents.OnEntitySelected += HandleEntitySelectionEvent;
        }


        /// <summary>
        /// Throw a new highlight event for this <paramref name="element"/> if not alredy highlighted.
        /// </summary>
        public void TryHighlight(IIslandSystemElement element, int handID = -1)
        {
            if (element is null)
                return;

            // Get the relevant element based on the current zoom level.
            var realElement = GetZoomLevelIslandElement(element);

            if (handID < 0)
                TrySystemHighlight(realElement);
            else
                TryHandHighlight(realElement, handID);
        }

        private void TryHandHighlight (IIslandSystemElement element, int handID)
        {
            // No new highlight event needed
            if (Array.Exists(_highlightedByHands, x => x == element) || _highlightedBySystem.Contains(element))
            {
                // The hand highlights something else.
                if (_highlightedByHands[handID] != element)
                {
                    // Dehighlight element that was highlighted before.
                    if (_highlightedByHands[handID] != null)
                        InteractionEvents.OnIslandElementHighlighted?.Invoke(_highlightedByHands[handID], false, handID);

                    _highlightedByHands[handID] = element;
                }

                return;
            }
            else
            {
                // Dehighlight element that was highlighted before.
                if (_highlightedByHands[handID] != null)
                    InteractionEvents.OnIslandElementHighlighted?.Invoke(_highlightedByHands[handID], false, handID);

                // Store new element.
                _highlightedByHands[handID] = element;

                // Throw event.
                InteractionEvents.OnIslandElementHighlighted?.Invoke(element, true, handID);
            }
        }

        private void TrySystemHighlight (IIslandSystemElement element)
        {
            // Add element.
            if (!_highlightedBySystem.Contains(element))
                _highlightedBySystem.Add(element);

            // Do not throw event if the element is already highlightd by hand.
            if (Array.Exists(_highlightedByHands, x => x == element))      
                return;

            // Throw event.
            InteractionEvents.OnIslandElementHighlighted?.Invoke(element, true, -1);
        }



        /// <summary>
        /// Dehighlights an entity.
        /// </summary>
        public void TryDehighlight(IIslandSystemElement element, int handID = -1)
        {
            if (element is null)
                return;

            var realElement = GetZoomLevelIslandElement(element);
                        
            if (handID < 0 && _highlightedBySystem.Contains(element))
            {
                handID = -1;
                _highlightedBySystem.Remove(element);
            }                
            else if (handID >= 0 && _highlightedByHands[handID] == realElement)
            {
                _highlightedByHands[handID] = null;

                // Do not dehighlight the element if the system wants to highlight the element.
                if (_highlightedBySystem.Contains(realElement))
                    return;
            }
            else
            {
                return;
            }

            InteractionEvents.OnIslandElementHighlighted?.Invoke(realElement, false, handID);
        }


        /// <summary>
        /// Selects an entity.
        /// </summary>
        public void Select(IIslandSystemElement element)
        {
            if (element is null)
                return;

            IIslandSystemElement relevantElement = GetZoomLevelIslandElement(element);
            InteractionEvents.OnIslandElementSelected?.Invoke(relevantElement, true);
        }
        
        /// <summary>
        /// Deselects an entity.
        /// </summary>
        public void Deselect(IIslandSystemElement element)
        {
            if (element is null)
                return;

            IIslandSystemElement relevantElement = GetZoomLevelIslandElement(element);
            InteractionEvents.OnIslandElementSelected?.Invoke(relevantElement, false);
        }

        /// <summary>
        /// Handles entity selection events.
        /// </summary>
        private void HandleEntitySelectionEvent (Entity entity, bool select)
        {
            if (select)
                Select(EntityAsIslandElement(entity));
            else
                Deselect(EntityAsIslandElement(entity));
        }

        /// <summary>
        /// Handles entity raycast hit events.
        /// </summary>
        private void HandleEntityRaycastEvent(Entity newHit, Entity oldHit, int handID)
        {
            if (newHit != Entity.Null)
            {
                var newElement = EntityAsIslandElement(newHit);
                TryHighlight(newElement, handID);
            }
            else
            {
                var oldElement = EntityAsIslandElement(oldHit);
                TryDehighlight(oldElement, handID);
            }
        }


        #region Helper Functions

        /// <summary>
        /// Returns the islandElement of an entity.
        /// Note that island entities do not have colliders, hence, we can not raycast them directly.
        /// </summary>
        private IIslandSystemElement EntityAsIslandElement(Entity entity)
        {   
            if (entity == Entity.Null)
                return null;

            if (_entityManager.HasComponent<RegionTileData>(entity))
            {
                var tileData = _entityManager.GetComponentData<RegionTileData>(entity);

                var region = VizManager.IslandData.Regions.Find(x => x.UID == tileData.RegionUID);
                var building = (region as Region).GetBuilding(tileData.BuildingUID);
                return building;
                //return Visualization.IslandData.Regions.Find(x => x.Entities.Contains(entity));
            }
            else if (_entityManager.HasComponent<BuildingData>(entity))
            {
                return VizManager.IslandData.Buildings.Find(x => x.Entity == entity);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Applies the current zoom level to an islandElement and returns the currently relevant islandElement.
        /// </summary>
        private IIslandSystemElement GetZoomLevelIslandElement (IIslandSystemElement islandElement)
        {
            if (islandElement is Building building)
            {
                return LODManager.CurrentLOD switch
                {
                    IslandLOD.Building => building,
                    IslandLOD.Region => building.Parent,
                    IslandLOD.Island => building.Parent.Parent,
                    _ => null,
                };
            }
            else if (islandElement is Region region)
            {
                return LODManager.CurrentLOD switch
                {
                    IslandLOD.Building => region,
                    IslandLOD.Region => region,
                    IslandLOD.Island => region.Parent,
                    _ => null,
                };
            }
            else
            {
                return islandElement;
            }
        }

        #endregion

    }
}
