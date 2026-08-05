using UnityEngine;

namespace StarterAssets
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData item;
        public int amount;

        public InventoryItem(ItemData item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }
}