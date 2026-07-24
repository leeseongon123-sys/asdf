using SculptGame.Player;
using UnityEngine;

namespace SculptGame.Building
{
    public class ResourceObject : MonoBehaviour
    {
        public BuildableObjectData objectData;
        public float interactionDistance = 2.5f;

        [Header("Hover Visual")]
        public Color highlightColor = new Color(1f, 0.9f, 0.3f);
        private Renderer[] renderers;
        private Color[] originalColors;
        private bool isHovered = false;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
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

        public void SetHovered(bool hovered)
        {
            if (isHovered == hovered) return;
            isHovered = hovered;

            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    if (isHovered)
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
