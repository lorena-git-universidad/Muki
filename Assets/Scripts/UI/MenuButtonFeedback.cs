using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Muki.UI
{
    [RequireComponent(typeof(Selectable))]
    public sealed class MenuButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private float hoverScale = 1.06f;
        [SerializeField] private Color normalBorder = new Color(0.443f, 0.286f, 0.122f, 1f);
        [SerializeField] private Color hoverBorder = new Color(0.925f, 0.733f, 0.282f, 1f);
        [SerializeField] private float transitionSpeed = 14f;

        private RectTransform rectTransform;
        private Outline outline;
        private Vector3 normalScale;
        private bool isHighlighted;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            normalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
            outline = GetComponent<Outline>();
            SetVisualState(false, true);
        }

        private void Update()
        {
            if (rectTransform == null) return;

            Vector3 targetScale = normalScale * (isHighlighted ? hoverScale : 1f);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData) => SetVisualState(true, false);
        public void OnPointerExit(PointerEventData eventData) => SetVisualState(false, false);
        public void OnSelect(BaseEventData eventData) => SetVisualState(true, false);
        public void OnDeselect(BaseEventData eventData) => SetVisualState(false, false);

        private void SetVisualState(bool highlighted, bool immediate)
        {
            isHighlighted = highlighted;
            if (outline != null) outline.effectColor = highlighted ? hoverBorder : normalBorder;

            if (immediate && rectTransform != null)
                rectTransform.localScale = normalScale;
        }
    }
}
