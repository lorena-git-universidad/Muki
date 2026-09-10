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
            public GameObject cardRoot;
            public GameObject iconFrame;
            public GameObject quantityContainer;
            public GameObject divider;
            public UnityEngine.UI.Image iconImage;
            public TMP_Text nameText;
            public TMP_Text quantityText;
        }

        public GameObject panel;
        public FirstPersonController firstPersonController;
        public List<MineralEntry> mineralEntries = new();
        public TMP_Text objectiveProgressText;

        private readonly HashSet<ItemData> unlockedMinerals = new();
        private bool isOpen;

        private void Start()
        {
            if (firstPersonController == null)
                firstPersonController = FindFirstObjectByType<FirstPersonController>();

            isOpen = false;
            if (panel != null)
                panel.SetActive(false);

            RefreshUnlocks();
            ApplyState();
        }

        private void Update()
        {
            RefreshUnlocks();

            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
                Toggle();
        }

        private void RefreshUnlocks()
        {
            if (InventoryManager.Instance != null)
            {
                foreach (MineralEntry entry in mineralEntries)
                {
                    if (entry?.itemData == null)
                        continue;

                    if (GetInventoryAmount(entry.itemData) > 0)
                        unlockedMinerals.Add(entry.itemData);
                }
            }

            if (isOpen)
                Refresh();
        }

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

            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }

        public void Refresh()
        {
            int discovered = 0;
            foreach (MineralEntry entry in mineralEntries)
            {
                if (entry?.itemData == null)
                    continue;

                bool unlocked = unlockedMinerals.Contains(entry.itemData);
                if (unlocked)
                    discovered++;

                if (entry.cardRoot != null)
                    entry.cardRoot.SetActive(true);

                if (entry.iconFrame != null)
                    entry.iconFrame.SetActive(unlocked);
                if (entry.nameText != null)
                {
                    entry.nameText.gameObject.SetActive(unlocked);
                    entry.nameText.text = unlocked ? entry.itemData.itemName.ToUpperInvariant() : string.Empty;
                }
                if (entry.divider != null)
                    entry.divider.SetActive(unlocked);
                if (entry.quantityContainer != null)
                    entry.quantityContainer.SetActive(unlocked);

                int amount = GetInventoryAmount(entry.itemData);
                if (entry.quantityText != null)
                    entry.quantityText.text = unlocked ? amount.ToString() : string.Empty;
                if (entry.iconImage != null)
                {
                    entry.iconImage.sprite = entry.itemData.icon;
                    entry.iconImage.enabled = unlocked;
                }
            }

            if (objectiveProgressText != null)
                objectiveProgressText.text = discovered + " / 3 minerales descubiertos";
        }

        private int GetInventoryAmount(ItemData item)
        {
            if (InventoryManager.Instance == null)
                return 0;

            int amount = 0;
            foreach (InventoryItem inventoryItem in InventoryManager.Instance.items)
            {
                if (inventoryItem?.item == item)
                    amount += inventoryItem.amount;
            }

            return amount;
        }
    }
}
