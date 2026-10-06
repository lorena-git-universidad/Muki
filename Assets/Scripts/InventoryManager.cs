using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;

        [Header("Inventory")]
        public List<InventoryItem> items = new();

        // Evento que avisa a la UI cuando cambia el inventario
        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // =========================================================
        // ADD ITEM
        // =========================================================

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null)
            {
                Debug.LogError("InventoryManager: se intentó añadir un ItemData NULL.");
                return false;
            }

            if (amount <= 0)
            {
                Debug.LogWarning(
                    $"InventoryManager: cantidad inválida para {item.itemName}: {amount}"
                );

                return false;
            }

            // -----------------------------------------------------
            // Buscar stack existente
            // -----------------------------------------------------

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                    continue;

                if (items[i].item != item)
                    continue;

                if (items[i].amount >= item.maxStack)
                    continue;

                int availableSpace = item.maxStack - items[i].amount;
                int amountToAdd = Mathf.Min(amount, availableSpace);

                items[i].amount += amountToAdd;
                amount -= amountToAdd;

                if (amount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }

            // -----------------------------------------------------
            // Crear nuevo stack
            // -----------------------------------------------------

            if (amount > 0)
            {
                items.Add(new InventoryItem(item, amount));
            }

            NotifyInventoryChanged();

            return true;
        }

        // =========================================================
        // GET ITEM
        // =========================================================

        public InventoryItem GetItem(int index)
        {
            if (index < 0 || index >= items.Count)
                return null;

            return items[index];
        }

        // =========================================================
        // SET ITEM
        // =========================================================

        public void SetItem(int index, InventoryItem item)
        {
            if (index < 0)
                return;

            while (items.Count <= index)
                items.Add(null);

            items[index] = item;

            NotifyInventoryChanged();
        }

        // =========================================================
        // GET AMOUNT
        // =========================================================

        public int GetItemAmount(ItemData item)
        {
            if (item == null)
                return 0;

            int amount = 0;

            foreach (InventoryItem inventoryItem in items)
            {
                if (inventoryItem == null)
                    continue;

                if (inventoryItem.item == item)
                    amount += inventoryItem.amount;
            }

            return amount;
        }

        // =========================================================
        // REMOVE ITEM
        // =========================================================

        public int RemoveItem(ItemData itemToRemove, int amountToRemove)
        {
            if (itemToRemove == null || amountToRemove <= 0)
                return 0;

            int remaining = amountToRemove;
            int removed = 0;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                InventoryItem inventoryItem = items[i];

                if (inventoryItem == null)
                    continue;

                if (inventoryItem.item != itemToRemove)
                    continue;

                int amount = Mathf.Min(
                    inventoryItem.amount,
                    remaining
                );

                inventoryItem.amount -= amount;

                remaining -= amount;
                removed += amount;

                if (inventoryItem.amount <= 0)
                {
                    items.RemoveAt(i);
                }

                if (remaining <= 0)
                    break;
            }

            if (removed > 0)
                NotifyInventoryChanged();

            return removed;
        }

        // =========================================================
        // REMOVE MINERALS
        // =========================================================

        public int RemoveMinerals(int amountToRemove)
        {
            if (amountToRemove <= 0)
                return 0;

            int remaining = amountToRemove;
            int removed = 0;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                InventoryItem inventoryItem = items[i];

                if (inventoryItem == null)
                    continue;

                if (inventoryItem.item == null)
                    continue;

                int amount = Mathf.Min(
                    inventoryItem.amount,
                    remaining
                );

                inventoryItem.amount -= amount;

                remaining -= amount;
                removed += amount;

                if (inventoryItem.amount <= 0)
                {
                    items.RemoveAt(i);
                }

                if (remaining <= 0)
                    break;
            }

            if (removed > 0)
                NotifyInventoryChanged();

            return removed;
        }

        // =========================================================
        // NOTIFY UI
        // =========================================================

        private void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }
    }
}
