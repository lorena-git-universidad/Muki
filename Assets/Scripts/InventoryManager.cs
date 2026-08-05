using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;

        [Header("UI")]
        public Transform slotParent;
        public GameObject slotPrefab;

        [Header("Inventory")]
        public int slotCount = 36;

        private readonly List<InventorySlot> slots = new();
        private readonly List<InventoryItem> items = new();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GenerateSlots();
        }

        void GenerateSlots()
        {
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotParent);

                InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();

                inventorySlot.Clear();

                slots.Add(inventorySlot);

                // cada slot empieza vacío
                items.Add(null);
            }
        }

        //--------------------------------------------------
        // AGREGA UN OBJETO AL INVENTARIO
        //--------------------------------------------------

        public bool AddItem(ItemData itemData)
        {
            if (itemData == null)
            {
                Debug.LogWarning("ItemData es NULL.");
                return false;
            }

            // Buscar stack existente
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                    continue;

                if (items[i].item == itemData &&
                    items[i].amount < itemData.maxStack)
                {
                    items[i].amount++;

                    UpdateUI();

                    return true;
                }
            }

            // Buscar slot vacío
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    items[i] = new InventoryItem(itemData, 1);

                    UpdateUI();

                    return true;
                }
            }

            Debug.Log("Inventario lleno.");

            return false;
        }

        //--------------------------------------------------
        // ACTUALIZA LA UI
        //--------------------------------------------------

        private void UpdateUI()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (items[i] == null)
                {
                    slots[i].Clear();
                }
                else
                {
                    slots[i].SetItem(
                        items[i].item.icon,
                        items[i].amount);
                }
            }
        }
    }
}