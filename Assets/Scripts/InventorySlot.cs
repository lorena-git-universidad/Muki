using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarterAssets
{
    public class InventorySlot :
    MonoBehaviour,
    IPointerDownHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
    {
        public Image itemImage;
        public TMP_Text amountText;

        public int slotIndex;

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("CLICK SLOT");
        }
        public void Clear()
        {
            itemImage.enabled = false;
            itemImage.sprite = null;
            amountText.text = "";
        }

        public void SetItem(InventoryItem item)
        {
            if (item == null)
            {
                Clear();
                return;
            }

            itemImage.enabled = true;
            itemImage.sprite = item.item.icon;

            amountText.text = item.amount > 1
                ? item.amount.ToString()
                : "";
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            InventoryDrag.Instance.BeginDrag(slotIndex);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryDrag.Instance.DropOnSlot(slotIndex);
            Debug.Log("Drop");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            InventoryDrag.Instance.EndDrag();
        }
    }
}