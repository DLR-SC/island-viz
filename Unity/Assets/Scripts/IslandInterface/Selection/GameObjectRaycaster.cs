using IslandEvents;
using IslandInterface.Input;
using UnityEngine;

namespace IslandInterface.Selection
{
    /// <summary>
    /// This class handles all GameObject raycast interaction by shooting raycasts every fixed update
    /// and throwing events when hitting (or stop hitting) Colliders of e.g. UI elements.
    /// </summary>
    public class GameObjectRaycaster : MonoBehaviour
    {
        /// <summary>
        /// The instance of this class.
        /// </summary>
        public static GameObjectRaycaster Instance;
        

        /// <summary>
        /// A definition of layers that will be considered and ignored by the raycast.
        /// </summary>
        public LayerMask RaycastLayerMask;
        /// <summary>
        /// Wether the EntityInteractionManager is currently shooting raycasts. 
        /// </summary>
        public bool RaycastEnabled;

        /// <summary>
        /// The max distance the raycast will travel.
        /// </summary>
        public float RaycastDistance;

        

        /// <summary>
        /// The collider that is current hit by the raycast.
        /// </summary>
        private Collider[] _lastHit;

        /// <summary>
        /// Called by Unity on application start (after Awake()).
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Called by Unity on application start (after Awake()).
        /// </summary>
        void Start()
        {
            _lastHit = new Collider[PlayerManager.Instance.Hands.Length];
            VizEvents.OnVisualizationInitilized += EnableRaycast;
        }

        /// <summary>
        /// Makes the EntityInteractionManager start shoot raycasts.
        /// </summary>
        public void EnableRaycast()
        {
            RaycastEnabled = true;
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
        /// Does a raycast for each hand and highlights / dehighlight depending on what was hit.
        /// </summary>
        private void ShootRaycast()
        {
            for (int i = 0; i < _lastHit.Length; i++)
            {
                var hand = PlayerManager.Instance.Hands[i];

                // Hit something.
                if (Raycast(hand, out RaycastHit hit))
                {
                    if (_lastHit[i] != null && _lastHit[i] == hit.collider) // Do nothing, if currentEntityHit equals entityHit.
                    {
                        return;
                    }

                    if (_lastHit[i] != null) // Throw Hit-End-Event for last hitted GameObject.
                    {
                        InteractionEvents.OnGameObjectHitEnd?.Invoke(_lastHit[i], hand);
                    }

                    InteractionEvents.OnGameObjectHitStart?.Invoke(hit.collider, hand);
                    _lastHit[i] = hit.collider;
                }
                else if (_lastHit[i] != null) // If nothing was hit, but we hit something last raycast.
                {
                    InteractionEvents.OnGameObjectHitEnd?.Invoke(_lastHit[i], hand);
                    _lastHit[i] = null;
                }
            }

        }

        public bool Raycast(Transform origin, out RaycastHit hit)
        {
            Debug.DrawLine(origin.position, origin.position + origin.forward * RaycastDistance, Color.red, Time.fixedDeltaTime);
            return Physics.Raycast(origin.position, origin.forward, out hit, RaycastDistance, RaycastLayerMask);
        }
    }
}