using TMPro;
using UnityEngine;

namespace StarterAssets
{
    public class MineralChestUI : MonoBehaviour
    {
        [Header("UI")]
        public TMP_Text mineralText;

        public void UpdateText(int current, int maximum)
        {
            if (mineralText == null)
                return;

            mineralText.text =
                current.ToString() +
                "/" +
                maximum.ToString();
        }
    }
}