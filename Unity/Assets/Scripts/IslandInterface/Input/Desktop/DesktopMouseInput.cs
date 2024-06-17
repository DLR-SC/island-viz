using IslandEvents;
using UnityEngine;

namespace IslandInterface.Input.Desktop
{
    /// <summary>
    /// This class handles all mouse inputs and throws a OnControllerButtonEvents.
    /// </summary>
    public class DesktopMouseInput : MonoBehaviour
    {
        /// <summary>
        /// Called by Unity every frame.
        /// </summary>
        void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                InteractionEvents.OnButtonEvent?.Invoke(Button.Trigger, PressType.PressDown, PlayerManager.Instance.Hands[0]);
            }
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                InteractionEvents.OnButtonEvent?.Invoke(Button.Trigger, PressType.PressUp, PlayerManager.Instance.Hands[0]);
            }
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                InteractionEvents.OnButtonEvent?.Invoke(Button.Menu, PressType.PressDown, PlayerManager.Instance.Hands[0]);
            }
            if (UnityEngine.Input.GetMouseButtonUp(1))
            {
                InteractionEvents.OnButtonEvent?.Invoke(Button.Menu, PressType.PressUp, PlayerManager.Instance.Hands[0]);
            }
            //if (Input.mouseScrollDelta.y != 0 && !Input.GetKey(KeyCode.LeftAlt))
            //{
            //    IslandTransformationSystem.Scale(Input.mouseScrollDelta.y * 0.04f);
            //}
        }
    }

}
