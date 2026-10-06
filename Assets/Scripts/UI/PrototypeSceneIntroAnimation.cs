using System.Collections;
using UnityEngine;

namespace Muki.UI
{
    public sealed class PrototypeSceneIntroAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup title;
        [SerializeField] private CanvasGroup playButton;
        [SerializeField] private CanvasGroup upcomingButton;
        [SerializeField] private CanvasGroup tooltip;
        [SerializeField] private RectTransform titleRect;
        [SerializeField] private RectTransform playRect;
        [SerializeField] private RectTransform upcomingRect;

        private void Awake()
        {
            SetAlpha(title, 0f);
            SetAlpha(playButton, 0f);
            SetAlpha(upcomingButton, 0f);
            SetAlpha(tooltip, 0f);
        }

        private void Start() => StartCoroutine(PlayIntro());

        private IEnumerator PlayIntro()
        {
            Vector3 titleEnd = titleRect != null ? titleRect.localPosition : Vector3.zero;
            Vector3 playEnd = playRect != null ? playRect.localPosition : Vector3.zero;
            Vector3 upcomingEnd = upcomingRect != null ? upcomingRect.localPosition : Vector3.zero;

            if (titleRect != null) titleRect.localPosition = titleEnd + Vector3.left * 55f;
            if (playRect != null) playRect.localPosition = playEnd + Vector3.right * 35f;
            if (upcomingRect != null) upcomingRect.localPosition = upcomingEnd + Vector3.right * 35f;

            yield return Animate(title, titleRect, titleEnd, 0.55f);
            yield return new WaitForSecondsRealtime(0.12f);
            yield return Animate(playButton, playRect, playEnd, 0.38f);
            yield return new WaitForSecondsRealtime(0.1f);
            yield return Animate(upcomingButton, upcomingRect, upcomingEnd, 0.34f);
        }

        private static IEnumerator Animate(CanvasGroup group, RectTransform rect, Vector3 end, float duration)
        {
            if (group == null) yield break;
            Vector3 start = rect != null ? rect.localPosition : end;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
                group.alpha = Mathf.Lerp(0f, 1f, t);
                if (rect != null) rect.localPosition = Vector3.Lerp(start, end, t);
                yield return null;
            }
            group.alpha = 1f;
            if (rect != null) rect.localPosition = end;
        }

        private static void SetAlpha(CanvasGroup group, float value)
        {
            if (group != null) group.alpha = value;
        }
    }
}
