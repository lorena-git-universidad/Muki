
using UnityEngine;

namespace StarterAssets
{
    [ExecuteAlways]
    public class ScreamerSpawnPreview : MonoBehaviour
    {
        [Header("Vista previa")]
        public Sprite previewSprite;

        [Min(0.01f)]
        public float previewScale = 1f;

        public Color previewTint = Color.white;

        private SpriteRenderer previewRenderer;

        private void OnEnable()
        {
            UpdatePreview();
        }

        private void OnValidate()
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (previewRenderer == null)
            {
                previewRenderer =
                    GetComponent<SpriteRenderer>();

                if (previewRenderer == null)
                {
                    previewRenderer =
                        gameObject.AddComponent<SpriteRenderer>();
                }
            }

            if (previewRenderer == null)
                return;

            previewRenderer.sprite = previewSprite;
            previewRenderer.color = previewTint;
            previewRenderer.drawMode = SpriteDrawMode.Simple;
            previewRenderer.sortingOrder = 100;
            transform.localScale = Vector3.one * previewScale;

            previewRenderer.enabled = previewSprite != null;
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                if (previewRenderer == null)
                    previewRenderer = GetComponent<SpriteRenderer>();

                if (previewRenderer != null)
                    previewRenderer.enabled = false;
            }
        }
    }
}
