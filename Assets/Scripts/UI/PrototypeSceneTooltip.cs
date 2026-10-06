using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Muki.UI
{
    public sealed class PrototypeSceneTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private CanvasGroup tooltip;
        [SerializeField] private TMP_Text message;
        [SerializeField] private string text = "Muki guarda una leyenda en cada rincón.";

        private void Awake()
        {
            if (message != null) message.text = text;
            SetVisible(false);
        }

        public void Configure(CanvasGroup group, TMP_Text label, string copy)
        {
            tooltip = group;
            message = label;
            text = copy;
            if (message != null) message.text = text;
        }

        public void OnPointerEnter(PointerEventData eventData) => SetVisible(true);
        public void OnPointerExit(PointerEventData eventData) => SetVisible(false);
        public void OnSelect(BaseEventData eventData) => SetVisible(true);
        public void OnDeselect(BaseEventData eventData) => SetVisible(false);

        private void SetVisible(bool visible)
        {
            if (tooltip == null) return;
            tooltip.alpha = visible ? 1f : 0f;
            tooltip.interactable = visible;
            tooltip.blocksRaycasts = false;
        }
    }
}
