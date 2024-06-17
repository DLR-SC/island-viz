using IslandEvents;
using IslandInterface.Input;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IslandInterface.Selection
{
    /// <summary>
    /// This class handles all entity raycast interaction by shooting raycasts every fixed update and throwing events
    /// when hitting (or stop hitting) entities.
    /// </summary>
    public class EntityRaycaster : MonoBehaviour
    {
        /// <summary>
        /// Wether the EntityInteractionManager is currently shooting raycasts. 
        /// </summary>
        public bool RaycastEnabled { get; private set; }

        /// <summary>
        /// The max distance the raycast will travel. (set in editor)
        /// </summary>
        public float RaycastDistance;

        /// <summary>
        /// The entity that was hit by the raycast last frame. Empty if nothing was hit last frame.
        /// </summary>
        private Entity[] _lastEntityHit;

        /// <summary>
        /// The position that was hit by the raycast last frame. Empty if nothing was hit last frame.
        /// </summary>
        private float3[] _lastEntityHitPosition;

        /// <summary>
        /// Called by Unity on application start (after Awake()).
        /// </summary>
        void Start()
        {
            VizEvents.OnVisualizationInitilized += EnableRaycast;

            _lastEntityHit = new Entity[PlayerManager.Instance.Hands.Length];
            _lastEntityHitPosition = new float3[_lastEntityHit.Length];

            for (int i = 0; i < _lastEntityHit.Length; i++)
            {
                _lastEntityHit[i] = Entity.Null;
                _lastEntityHitPosition[i] = float3.zero;
            }
        }

        /// <summary>
        /// Makes the EntityInteractionManager start shoot raycasts.
        /// </summary>
        private void EnableRaycast()
        {
            RaycastEnabled = true;
        }

        public float3 GetCurrentHitPosition (int handID)
        {
            return _lastEntityHitPosition[handID];
        }

        /// <summary>
        /// Called every fixed update by Unity.
        /// </summary>
        private void FixedUpdate()
        {
            if (RaycastEnabled)
                ShootRaycast();
        }

        /// <summary>
        /// Shoots a raycast for each player hand and throws an event when a raycast hits something new.
        /// </summary>
        private void ShootRaycast()
        {
            for (int i = 0; i < _lastEntityHit.Length; i++)
            {
                var hand = PlayerManager.Instance.Hands[i];

                // We hit something.
                if (EntityRaycast.Raycast(hand.position, hand.position + hand.forward * RaycastDistance, out var newEntityHit, out var hitPosition))
                {                    
                    _lastEntityHitPosition[i] = hitPosition; // Update hit position.

                    // Do nothing else, if lastEntityHit equals newEntityHit.
                    if (_lastEntityHit[i] != Entity.Null && _lastEntityHit[i] == newEntityHit) 
                        return;

                    InteractionEvents.OnEntityHitChanged?.Invoke(newEntityHit, _lastEntityHit[i], i);
                    _lastEntityHit[i] = newEntityHit;
                }
                // We hit nothing, but we did hit something last raycast.
                else if (_lastEntityHit[i] != Entity.Null)
                {
                    InteractionEvents.OnEntityHitChanged?.Invoke(Entity.Null, _lastEntityHit[i], i);
                    _lastEntityHit[i] = Entity.Null;
                }
            }
        }


    }
}
