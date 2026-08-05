using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class InventorySlot : MonoBehaviour
    {
        public Image itemImage;
        public TMP_Text amountText;

        public void Clear()
        {
            itemImage.enabled = false;
            amountText.text = "";
        }

        public void SetItem(Sprite icon, int amount)
        {
            itemImage.enabled = true;
            itemImage.sprite = icon;

            amountText.text = amount > 1 ? amount.ToString() : "";
        }
    }
}