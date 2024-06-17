using UnityEngine;

namespace IslandInterface.Input.Desktop
{
    /// <summary>
    /// This class updates the position of a GameObject representing the player hand (i.e. mouse pointer) in the scene.
    /// This GameObject is the origin of all ray casts.
    /// </summary>
    public class DesktopHandPositionUpdater : MonoBehaviour
    {
        /// <summary>
        /// Called by Unity every frame.
        /// </summary>
        void Update()
        {
            Vector3 mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            transform.position = Camera.main.ScreenToWorldPoint(mousePos);
            transform.forward = Vector3.Normalize(transform.position - Camera.main.transform.position);
        }
    }
}
