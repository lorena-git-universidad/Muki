
using TMPro;
using UnityEngine;

namespace StarterAssets
{
    public class MineralChestUI : MonoBehaviour
    {
        [Header("UI")]
        public TMP_Text mineralText;
        public ChestUIVisual visual;

        private void Awake()
        {
            if (visual == null)
                visual = GetComponentInChildren<ChestUIVisual>(true);
        }

        public void UpdateText(int current, int maximum)
        {
            if (mineralText == null)
                return;

            mineralText.text = current + "/" + maximum;
            if (visual != null)
                visual.UpdateValue(current, maximum);
        }

        public void UpdateContents(string contents)
        {
            if (mineralText == null)
                return;

            mineralText.text = contents;
            if (visual != null)
                visual.UpdateContents(contents);
        }
    }
}
