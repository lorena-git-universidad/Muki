using UnityEngine;
using UnityEngine.EventSystems;

namespace Muki.UI
{
    /// <summary>
    /// Se añade a un botón de leyenda: al pasar el cursor o seleccionarlo muestra su mapa
    /// y hace aparecer (con fundido) los textos de esa leyenda junto a su zona del mapa.
    /// </summary>
    public sealed class LevelMapHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private LevelMapHover map;
        [SerializeField] private Texture highlightedMap;
        [SerializeField] private CanvasGroup[] captions;
        [SerializeField] private float captionFadeDuration = 0.3f;

        private bool hovered;

        private void Awake() => SetCaptionsAlpha(0f);

        private void Update()
        {
            if (captions == null) return;

            float step = Time.unscaledDeltaTime / Mathf.Max(0.01f, captionFadeDuration);
            foreach (CanvasGroup caption in captions)
            {
                if (caption == null) continue;
                caption.alpha = Mathf.MoveTowards(caption.alpha, hovered ? 1f : 0f, step);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => Show();
        public void OnPointerExit(PointerEventData eventData) => Hide();
        public void OnSelect(BaseEventData eventData) => Show();
        public void OnDeselect(BaseEventData eventData) => Hide();

        private void OnDisable()
        {
            Hide();
            SetCaptionsAlpha(0f);
        }

        private void Show()
        {
            hovered = true;
            if (map != null) map.Hover(highlightedMap);
        }

        private void Hide()
        {
            hovered = false;
            if (map != null) map.Unhover(highlightedMap);
        }

        private void SetCaptionsAlpha(float alpha)
        {
            if (captions == null) return;
            foreach (CanvasGroup caption in captions)
                if (caption != null) caption.alpha = alpha;
        }
    }
}
