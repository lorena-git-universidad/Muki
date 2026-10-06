using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public sealed class MiningProgressUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text percentText;

        private void Awake() => HideProgress();

        public void ShowProgress(float progress)
        {
            float value = Mathf.Clamp01(progress);
            if (group != null) group.alpha = 1f;
            if (progressFill != null) progressFill.fillAmount = value;
            if (percentText != null) percentText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        public void HideProgress()
        {
            if (group != null) group.alpha = 0f;
            if (progressFill != null) progressFill.fillAmount = 0f;
            if (percentText != null) percentText.text = "0%";
        }
    }
}
