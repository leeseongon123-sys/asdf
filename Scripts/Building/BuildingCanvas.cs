using UnityEngine;

namespace SculptGame.Building
{
    public class BuildingCanvas : MonoBehaviour
    {
        public static BuildingCanvas Instance { get; private set; }

        [Header("Canvas Bounds")]
        public Vector3 canvasSize = new Vector3(10f, 10f, 10f);
        public Transform canvasCenterTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public bool IsPositionInCanvas(Vector3 pos)
        {
            Vector3 center = canvasCenterTransform != null ? canvasCenterTransform.position : transform.position;
            Vector3 halfSize = canvasSize * 0.5f;

            return (pos.x >= center.x - halfSize.x && pos.x <= center.x + halfSize.x) &&
                   (pos.y >= center.y - halfSize.y && pos.y <= center.y + halfSize.y) &&
                   (pos.z >= center.z - halfSize.z && pos.z <= center.z + halfSize.z);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = canvasCenterTransform != null ? canvasCenterTransform.position : transform.position;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
            Gizmos.DrawCube(center, canvasSize);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
            Gizmos.DrawWireCube(center, canvasSize);
        }
    }
}
