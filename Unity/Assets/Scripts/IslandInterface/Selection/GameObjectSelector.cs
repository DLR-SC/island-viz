using IslandEvents;
using IslandInterface.Input;
using UnityEngine;

namespace IslandInterface.Selection
{
    /// <summary>
    /// This class throws GameObject selection events by listening to player input events and GameObject raycast hit events.
    /// </summary>
    public class GameObjectSelector : MonoBehaviour
    {
        

        

        /// <summary>
        /// The collider that is current hit by the raycast.
        /// </summary>
        private Collider _lastHit;


        /// <summary>
        /// Called by Unity on application start.
        /// </summary>
        private void Start()
        {
            InteractionEvents.OnButtonEvent += PossibleSelectionOnButtonDown;
            InteractionEvents.OnGameObjectHitStart += Highlight;
            InteractionEvents.OnGameObjectHitEnd += DeHighlight;
        }

        /// <summary>
        /// Stores the last hit collider.
        /// </summary>
        private void Highlight(Collider collider, Transform hand)
        {
            _lastHit = collider;
        }

        /// <summary>
        /// Removes the last hit collider.
        /// </summary>
        private void DeHighlight(Collider collider, Transform hand)
        {
            _lastHit = null;
        }

        /// <summary>
        /// This is called when a button was pressed. 
        /// If the trigger button was pressed and there is a currently selected collider, this collider gets selected.
        /// </summary>
        private void PossibleSelectionOnButtonDown(Button button, PressType type, Transform hand)
        {
            if (button != Button.Trigger || type != PressType.PressDown)
                return;

            if (_lastHit == null)
            {
                return;
            }

            InteractionEvents.OnGameObjectSelected?.Invoke(_lastHit.gameObject);
        }
    }
}