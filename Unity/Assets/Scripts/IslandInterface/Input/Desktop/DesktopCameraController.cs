using System.Collections;
using IslandEvents;
using UnityEngine;

namespace IslandInterface.Input.Desktop
{
    public class DesktopCameraController : MonoBehaviour
    {
        public float MinHeight;
        public float MaxHeight;

        public float ZoomRate;
        public AnimationCurve ZoomCurve;

        public enum CameraMode
        {
            Mode2D,
            Mode3D
        }

        private bool _active;
        private CameraMode _currentMode;


        // Start is called before the first frame update
        void Start()
        {
            transform.localRotation = Quaternion.identity;
            _currentMode = CameraMode.Mode3D;
            _active = false;

            VizEvents.OnVisualizationInitilized += ActivateCameraController;
        }

        private void ActivateCameraController()
        {
            StartCoroutine(DelayedStartCameraToggle());
        }

        IEnumerator DelayedStartCameraToggle ()
        {
            yield return new WaitForSeconds(1);
            _active = true;
            yield return CameraModeChanging(CameraMode.Mode3D);

            yield return null;

            GetComponent<SimpleMaxCamera>().enabled = true;
        }

        public void ToggleCameraMode()
        {
            if (!_active)
                return;

            ToggleCameraMode(_currentMode == CameraMode.Mode2D ? CameraMode.Mode3D : CameraMode.Mode2D);
        }

        public void ToggleCameraMode (CameraMode mode)
        {
            if (!_active || mode == _currentMode)
                return;

            StartCoroutine(CameraModeChanging(mode));

            _currentMode = mode;
        }

        IEnumerator CameraModeChanging (CameraMode mode)
        {
            float value = 0;

            var startRotation = transform.localRotation;
            var targetRotation = Quaternion.Euler(mode == CameraMode.Mode2D ? 90f : 45f, 0f, 0f);

            var startPosition = transform.localPosition;
            var targetPosition = transform.localPosition + (mode == CameraMode.Mode2D ? 1f : -1f) * transform.localPosition.y * Vector3.forward;

            while (value <= 1f)
            {
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, value);
                transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, value);
                value += 3f * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            transform.localRotation = targetRotation;
            transform.localPosition = targetPosition;
        }

        void LateUpdate()
        {
            if (!_active)
                return;

            if (UnityEngine.Input.mouseScrollDelta.y != 0) // Input.GetKey(KeyCode.LeftAlt) 
            {
                // Affect the desired Zoom distance if we roll the scrollwheel
                transform.localPosition += UnityEngine.Input.mouseScrollDelta.y * Time.deltaTime * ZoomRate * transform.forward * ZoomCurve.Evaluate(transform.position.y / MaxHeight);
            }
        }
    }

}
