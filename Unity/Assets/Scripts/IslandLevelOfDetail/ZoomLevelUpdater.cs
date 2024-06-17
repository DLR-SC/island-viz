using IslandInterface.Input;
using IslandSettings;
using UnityEngine;

namespace IslandLevelOfDetail
{
    /// <summary>
    /// This class updates the current zoom level when the zoom (i.e., the island scale) was changed.
    /// </summary>
    public class ZoomLevelUpdater : MonoBehaviour
    {
        private Transform _playerCamera;

        private void Awake()
        {
            LODManager.SetZoomLevel(IslandLOD.Island);
        }

        /// <summary>
        /// Called by Unity on application start.
        /// </summary>
        private void Start()
        {
            _playerCamera = PlayerManager.Instance.PlayerCamera.transform;
        }

        private void FixedUpdate()
        {
            if (_playerCamera.transform.position.y <= ZoomLevelSettings.BuildingHeight)
            {
                if (LODManager.CurrentLOD != IslandLOD.Building)
                    LODManager.SetZoomLevel(IslandLOD.Building);
            }
            else if (_playerCamera.transform.position.y <= ZoomLevelSettings.RegionHeight)
            {
                if (LODManager.CurrentLOD != IslandLOD.Region)
                    LODManager.SetZoomLevel(IslandLOD.Region);
            }
            else if (_playerCamera.transform.position.y > ZoomLevelSettings.RegionHeight)
            {
                if (LODManager.CurrentLOD != IslandLOD.Island)
                    LODManager.SetZoomLevel(IslandLOD.Island);
            }
        }
    }
}