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

        public List<InventoryItem> items = new();

        private readonly List<InventorySlot> slots = new();

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
                GameObject slotObj = Instantiate(slotPrefab, slotParent);

                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                slot.slotIndex = i;

                slot.Clear();

                slots.Add(slot);
                items.Add(null);
            }
        }

        public bool AddItem(ItemData item, int amount = 1)
        {
            // Buscar stack existente
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null &&
                    items[i].item == item &&
                    items[i].amount < item.maxStack)
                {
                    items[i].amount += amount;
                    RefreshSlot(i);
                    return true;
                }
            }

            // Buscar espacio vacío
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    items[i] = new InventoryItem(item, amount);
                    RefreshSlot(i);
                    return true;
                }
            }

            Debug.Log("Inventario lleno.");
            return false;
        }

        public InventoryItem GetItem(int index)
        {
            return items[index];
        }

        public void SetItem(int index, InventoryItem item)
        {
            items[index] = item;
            RefreshSlot(index);
        }

        public void MoveItem(int from, int to)
        {
            InventoryItem moving = items[from];

            items[from] = items[to];
            items[to] = moving;

            RefreshSlot(from);
            RefreshSlot(to);
        }

        public void RefreshSlot(int index)
        {
            if (items[index] == null)
                slots[index].Clear();
            else
                slots[index].SetItem(items[index]);
        }
    }
}