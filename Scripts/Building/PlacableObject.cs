using UnityEngine;

namespace SculptGame.Building
{
    public class PlacableObject : MonoBehaviour
    {
        public string objectId;
        public string objectName;

        [Header("Visual Feedback")]
        private MeshRenderer[] renderers;
        private Color[] originalColors;

        private void Awake()
        {
            renderers = GetComponentsInChildren<MeshRenderer>();
            if (renderers != null && renderers.Length > 0)
            {
                originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].material != null && renderers[i].material.HasProperty("_Color"))
                    {
                        originalColors[i] = renderers[i].material.color;
                    }
                }
            }
        }

        public void SetHighlight(bool highlight, Color highlightColor)
        {
            if (renderers == null) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    if (highlight)
                    {
                        renderers[i].material.color = highlightColor;
                    }
                    else if (originalColors != null && i < originalColors.Length)
                    {
                        renderers[i].material.color = originalColors[i];
                    }
                }
            }
        }
    }
}
