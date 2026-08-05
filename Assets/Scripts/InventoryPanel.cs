using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class InventoryPanel : MonoBehaviour
    {
        [Header("UI")]
        public GameObject inventoryPanel;

        private bool isOpen = false;

        private void Start()
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                ToggleInventory();
            }
        }

        public void ToggleInventory()
        {
            isOpen = !isOpen;

            inventoryPanel.SetActive(isOpen);

            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }
    }
}
