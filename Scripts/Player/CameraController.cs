using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SculptGame.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Top-Down Target Follow")]
        public Transform target;
        public Vector3 followOffset = new Vector3(0f, 14f, -10f);
        public Vector3 cameraAngle = new Vector3(50f, 0f, 0f);
        public float followSpeed = 8.0f;

        [Header("Zoom Settings")]
        public float minZoom = 6.0f;
        public float maxZoom = 25.0f;
        public float zoomSpeed = 4.0f;

        private float currentZoomMultiplier = 1.0f;

        private void Start()
        {
            // In Top-Down mode, keep cursor visible for UI and clicking placement
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            transform.rotation = Quaternion.Euler(cameraAngle);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Handle Scroll Zoom
            float scrollDelta = GetScrollDelta();
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                currentZoomMultiplier -= scrollDelta * zoomSpeed * Time.deltaTime;
                currentZoomMultiplier = Mathf.Clamp(currentZoomMultiplier, 0.4f, 1.8f);
            }

            Vector3 desiredOffset = followOffset * currentZoomMultiplier;
            Vector3 targetPosition = target.position + desiredOffset;

            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(cameraAngle);
        }

        private float GetScrollDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.scroll.ReadValue().y * 0.01f;
            }
            return 0f;
#else
            return Input.GetAxis("Mouse ScrollWheel");
#endif
        }
    }
}
