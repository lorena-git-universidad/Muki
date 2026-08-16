using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class InventoryPanel : MonoBehaviour
    {
        [Header("UI")]
        public GameObject inventoryPanel;

        [Header("Player")]
        public FirstPersonController firstPersonController;
        public PlayerInput playerInput;

        private bool isOpen = false;

        private void Start()
        {
            inventoryPanel.SetActive(false);

            if (firstPersonController == null)
                firstPersonController = FindFirstObjectByType<FirstPersonController>();

            if (playerInput == null)
                playerInput = FindFirstObjectByType<PlayerInput>();
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

            if (firstPersonController != null)
                firstPersonController.enabled = !isOpen;

            Cursor.lockState = isOpen
                ? CursorLockMode.None
                : CursorLockMode.Locked;

            Cursor.visible = isOpen;
        }
    }
}