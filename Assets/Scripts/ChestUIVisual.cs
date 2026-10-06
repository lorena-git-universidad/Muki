using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public sealed class ChestUIVisual : MonoBehaviour
    {
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private Image progressFill;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float visibleAlpha = 0.96f;

        public void UpdateValue(int current, int maximum)
        {
            if (countText != null) countText.text = $"{current}/{maximum}";
            if (progressFill != null) progressFill.fillAmount = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0f;
            if (detailText != null) detailText.text = current > 0 ? "MINERALES ALMACENADOS" : "COFRE VACÍO";
            if (canvasGroup != null) canvasGroup.alpha = visibleAlpha;
        }

        public void UpdateContents(string contents)
        {
            if (string.IsNullOrWhiteSpace(contents))
            {
                UpdateValue(0, 0);
                return;
            }

            string[] lines = contents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int current = 0;
            int maximum = 0;
            if (lines.Length > 0)
            {
                string[] values = lines[0].Split('/');
                if (values.Length == 2)
                {
                    int.TryParse(values[0].Substring(values[0].LastIndexOf(':') + 1).Trim(), out current);
                    int.TryParse(values[1].Trim(), out maximum);
                }
            }

            UpdateValue(current, maximum);
            if (detailText != null && lines.Length > 1)
            {
                string details = string.Join("  ·  ", lines, 1, lines.Length - 1);
                detailText.text = details;
            }
        }
    }
}
