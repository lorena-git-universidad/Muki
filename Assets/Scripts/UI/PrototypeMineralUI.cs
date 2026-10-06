using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PrototypeMineralUI : MonoBehaviour
    {
        [Serializable]
        public class MineralEntry
        {
            public ItemData itemData;

            [Header("Card")]
            public GameObject cardRoot;

            [Header("Visual")]
            public GameObject iconFrame;
            public GameObject quantityContainer;
            public GameObject divider;

            [Header("UI")]
            public UnityEngine.UI.Image iconImage;
            public TMP_Text nameText;
            public TMP_Text quantityText;
        }

        [Header("Panel")]
        public GameObject panel;

        public FirstPersonController firstPersonController;

        [Header("Mineral Grid")]
        public List<MineralEntry> mineralEntries = new();

        [Header("Objective")]
        public TMP_Text objectiveProgressText;

        private bool isOpen;

        // =========================================================
        // START
        // =========================================================

        private void Start()
        {
            if (firstPersonController == null)
            {
                firstPersonController =
                    FindFirstObjectByType<FirstPersonController>();
            }

            isOpen = false;

            if (panel != null)
                panel.SetActive(false);

            Refresh();

            ApplyState();
        }

        // =========================================================
        // ENABLE / DISABLE
        // =========================================================

        private void OnEnable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= Refresh;
            }
        }

        // =========================================================
        // INPUT
        // =========================================================

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.tabKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        // =========================================================
        // TOGGLE PANEL
        // =========================================================

        public void Toggle()
        {
            isOpen = !isOpen;

            ApplyState();

            if (isOpen)
                Refresh();
        }

        private void ApplyState()
        {
            if (panel != null)
                panel.SetActive(isOpen);

            if (firstPersonController != null)
                firstPersonController.enabled = !isOpen;

            Cursor.lockState = isOpen
                ? CursorLockMode.None
                : CursorLockMode.Locked;

            Cursor.visible = isOpen;
        }

        // =========================================================
        // REFRESH UI
        // =========================================================

        public void Refresh()
        {
            int discovered = 0;

            foreach (MineralEntry entry in mineralEntries)
            {
                if (entry == null)
                    continue;

                if (entry.itemData == null)
                    continue;

                int amount = GetInventoryAmount(entry.itemData);

                bool unlocked = amount > 0;

                if (unlocked)
                    discovered++;

                // -------------------------------------------------
                // CARD
                // -------------------------------------------------

                if (entry.cardRoot != null)
                    entry.cardRoot.SetActive(true);

                // -------------------------------------------------
                // ICON FRAME
                // -------------------------------------------------

                if (entry.iconFrame != null)
                    entry.iconFrame.SetActive(unlocked);

                // -------------------------------------------------
                // NAME
                // -------------------------------------------------

                if (entry.nameText != null)
                {
                    entry.nameText.gameObject.SetActive(unlocked);

                    entry.nameText.text = unlocked
                        ? entry.itemData.itemName.ToUpperInvariant()
                        : string.Empty;
                }

                // -------------------------------------------------
                // DIVIDER
                // -------------------------------------------------

                if (entry.divider != null)
                    entry.divider.SetActive(unlocked);

                // -------------------------------------------------
                // QUANTITY CONTAINER
                // -------------------------------------------------

                if (entry.quantityContainer != null)
                    entry.quantityContainer.SetActive(unlocked);

                // -------------------------------------------------
                // QUANTITY
                // -------------------------------------------------

                if (entry.quantityText != null)
                {
                    entry.quantityText.text = unlocked
                        ? amount.ToString()
                        : string.Empty;
                }

                // -------------------------------------------------
                // ICON
                // -------------------------------------------------

                if (entry.iconImage != null)
                {
                    entry.iconImage.sprite = entry.itemData.icon;
                    entry.iconImage.enabled = unlocked;
                }
            }

            // -----------------------------------------------------
            // OBJECTIVE
            // -----------------------------------------------------

            if (objectiveProgressText != null)
            {
                objectiveProgressText.text =
                    discovered + " / 3 minerales descubiertos";
            }
        }

        // =========================================================
        // GET INVENTORY AMOUNT
        // =========================================================

        private int GetInventoryAmount(ItemData item)
        {
            if (InventoryManager.Instance == null)
                return 0;

            return InventoryManager.Instance.GetItemAmount(item);
        }
    }
}
