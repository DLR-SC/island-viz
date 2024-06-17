using IslandEntities.IslandComponents;
using IslandEvents;
using IslandInterface.Input;
using Unity.Entities;
using UnityEngine;

namespace IslandInterface.Selection
{
    /// <summary>
    /// This class combines button input events and entity raycast events to possible selection events.
    /// </summary>
    public class EntitySelector : MonoBehaviour
    {
        
        /// <summary>
        /// The current highlighted entity.
        /// </summary>
        private Entity? _currentHighlightedEntity;

        /// <summary>
        /// The current highlighted entity.
        /// </summary>
        private Entity? _currentSelectedEntity;

        /// <summary>
        /// Unity ECS controller used to handle component requests.
        /// </summary>
        private EntityManager _entityManager;


        /// <summary>
        /// Called by Unity on application start (after Awake()).
        /// </summary>
        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _currentHighlightedEntity = null;

            InteractionEvents.OnButtonEvent += PossibleSelectionOnButtonDown;
            InteractionEvents.OnEntityHitChanged += HighlightEntity;
        }


        /// <summary>
        /// Highlights an entity.
        /// </summary>
        private void HighlightEntity(Entity newHit, Entity oldHit, int hand)
        {
            if (newHit != Entity.Null && !_entityManager.HasComponent<HighlightTag>(newHit))
            {
                _entityManager.AddComponent<HighlightTag>(newHit);
            }

            _currentHighlightedEntity = newHit;
        }


        /// <summary>
        /// If the trigger button was pressed and there is a currently selected entity, this entity gets selected.
        /// </summary>
        private void PossibleSelectionOnButtonDown(Button button, PressType type, Transform hand)
        {
            if (button != Button.Trigger || type != PressType.PressDown)
                return;

            if (_currentHighlightedEntity == null || _currentSelectedEntity == _currentHighlightedEntity)
                return;

            if (_currentSelectedEntity != null)
            {
                InteractionEvents.OnEntitySelected?.Invoke(_currentSelectedEntity.Value, false);
                _currentSelectedEntity = null;
            }

            InteractionEvents.OnEntitySelected?.Invoke(_currentHighlightedEntity.Value, true);
        }
    }
}
