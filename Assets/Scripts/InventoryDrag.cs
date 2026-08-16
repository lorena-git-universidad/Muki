using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StarterAssets
{
    public class InventoryDrag : MonoBehaviour
    {
        public static InventoryDrag Instance;

        public Image dragImage;

        private int originIndex = -1;

        private bool dropped;

        private InventoryItem draggedItem;

        private void Awake()
        {
            Instance = this;
            dragImage.enabled = false;
        }

        private void Update()
        {
            if (dragImage.enabled && Mouse.current != null)
            {
                dragImage.transform.position =
                    Mouse.current.position.ReadValue();
            }
        }

        public void BeginDrag(int slotIndex)
        {
            draggedItem = InventoryManager.Instance.GetItem(slotIndex);

            if (draggedItem == null)
                return;

            originIndex = slotIndex;
            dropped = false;

            dragImage.enabled = true;
            dragImage.sprite = draggedItem.item.icon;

            InventoryManager.Instance.SetItem(slotIndex, null);

            Debug.Log("Begin Drag");
            Debug.Log(dragImage.sprite);
        }

        public void DropOnSlot(int slotIndex)
        {
            if (draggedItem == null)
                return;

            InventoryManager.Instance.MoveItem(originIndex, slotIndex);

            dropped = true;
        }

        public void EndDrag()
        {
            if (!dropped && draggedItem != null)
            {
                InventoryManager.Instance.SetItem(originIndex, draggedItem);
            }

            dragImage.enabled = false;

            draggedItem = null;
            originIndex = -1;
            dropped = false;
            Debug.Log("End Drag");
        }
    }
}