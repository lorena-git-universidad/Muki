using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Muki.UI
{
    public sealed class MainMenuIntroAnimation : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float overlayFadeDuration = 0.65f;
        [SerializeField] private float titleDelay = 0.12f;
        [SerializeField] private float menuDelay = 0.72f;
        [SerializeField] private float buttonGap = 0.13f;
        [SerializeField] private float exitDuration = 0.35f;

        private CanvasGroup overlay;
        private CanvasGroup warmField;
        private CanvasGroup rightShade;
        private CanvasGroup title;
        private CanvasGroup subtitle;
        private CanvasGroup navigation;
        private CanvasGroup playButton;
        private CanvasGroup quitButton;
        private CanvasGroup meta;
        private RectTransform titleRect;
        private RectTransform navigationRect;
        private RectTransform playRect;
        private RectTransform quitRect;
        private Vector3 playScale;
        private bool isReady;
        private bool isExiting;

        private void Awake()
        {
            overlay = GetOrAddCanvasGroup("UX_TransitionOverlay");
            warmField = GetOrAddCanvasGroup("WarmField");
            rightShade = GetOrAddCanvasGroup("RightShade");
            title = GetOrAddCanvasGroup("Title");
            subtitle = GetOrAddCanvasGroup("Subtitle");
            navigation = GetOrAddCanvasGroup("Navigation");
            playButton = GetOrAddCanvasGroup("PlayButton");
            quitButton = GetOrAddCanvasGroup("QuitButton");
            meta = GetOrAddCanvasGroup("TopMeta");

            titleRect = FindRect("Title");
            navigationRect = FindRect("Navigation");
            playRect = FindRect("PlayButton");
            quitRect = FindRect("QuitButton");
            playScale = playRect != null ? playRect.localScale : Vector3.one;

            overlay.alpha = 1f;
            warmField.alpha = 0f;
            rightShade.alpha = 0f;
            title.alpha = 0f;
            subtitle.alpha = 0f;
            navigation.alpha = 0f;
            playButton.alpha = 0f;
            quitButton.alpha = 0f;
            meta.alpha = 0f;
        }

        private void Start()
        {
            StartCoroutine(PlayIntro());
        }

        private IEnumerator PlayIntro()
        {
            Vector3 titleStart = titleRect != null ? titleRect.localPosition : Vector3.zero;
            Vector3 navigationStart = navigationRect != null ? navigationRect.localPosition : Vector3.zero;
            Vector3 playStart = playRect != null ? playRect.localPosition : Vector3.zero;
            Vector3 quitStart = quitRect != null ? quitRect.localPosition : Vector3.zero;

            if (titleRect != null) titleRect.localPosition = titleStart + Vector3.left * 70f;
            if (navigationRect != null) navigationRect.localPosition = navigationStart + Vector3.right * 80f;
            if (playRect != null) playRect.localPosition = playStart + Vector3.right * 32f;
            if (quitRect != null) quitRect.localPosition = quitStart + Vector3.right * 32f;

            StartCoroutine(FadeCanvas(warmField, 1f, 0.55f));
            StartCoroutine(FadeCanvas(rightShade, 1f, 0.75f));
            yield return FadeCanvas(overlay, 0f, overlayFadeDuration);

            yield return new WaitForSecondsRealtime(titleDelay);
            yield return AnimateCanvasAndPosition(title, titleRect, 1f, titleStart, 0.75f, 70f, Vector3.left);

            yield return AnimateCanvasAndPosition(subtitle, null, 1f, Vector3.zero, 0.42f, 0f, Vector3.zero);
            yield return new WaitForSecondsRealtime(menuDelay - titleDelay);
            yield return AnimateCanvasAndPosition(navigation, navigationRect, 1f, navigationStart, 0.6f, 80f, Vector3.right);
            yield return AnimateCanvasAndPosition(playButton, playRect, 1f, playStart, 0.38f, 32f, Vector3.right);
            yield return new WaitForSecondsRealtime(buttonGap);
            yield return AnimateCanvasAndPosition(quitButton, quitRect, 1f, quitStart, 0.34f, 32f, Vector3.right);
            yield return FadeCanvas(meta, 1f, 0.3f);

            isReady = true;
            StartCoroutine(PulsePlayButton());
        }

        public void BeginExit(Action completed)
        {
            if (isExiting) return;
            isExiting = true;
            StartCoroutine(ExitRoutine(completed));
        }

        private IEnumerator ExitRoutine(Action completed)
        {
            yield return FadeCanvas(overlay, 1f, exitDuration);
            completed?.Invoke();
        }

        private IEnumerator PulsePlayButton()
        {
            while (!isExiting && isReady && playRect != null)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.012f;
                playRect.localScale = playScale * pulse;
                yield return null;
            }
        }

        private IEnumerator AnimateCanvasAndPosition(CanvasGroup group, RectTransform rect, float targetAlpha, Vector3 targetPosition, float duration, float distance, Vector3 direction)
        {
            Vector3 startPosition = rect != null ? rect.localPosition : Vector3.zero;
            Vector3 fromPosition = targetPosition + direction * distance;
            if (rect != null) rect.localPosition = fromPosition;
            float startAlpha = group.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                if (rect != null) rect.localPosition = Vector3.Lerp(fromPosition, startPosition, t);
                yield return null;
            }
            group.alpha = targetAlpha;
            if (rect != null) rect.localPosition = startPosition;
        }

        private IEnumerator FadeCanvas(CanvasGroup group, float target, float duration)
        {
            float start = group.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(start, target, EaseOutCubic(Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            group.alpha = target;
        }

        private CanvasGroup GetOrAddCanvasGroup(string objectName)
        {
            Transform target = FindChild(objectName);
            if (target == null) return gameObject.AddComponent<CanvasGroup>();
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            return group != null ? group : target.gameObject.AddComponent<CanvasGroup>();
        }

        private RectTransform FindRect(string objectName) => FindChild(objectName) as RectTransform;

        private Transform FindChild(string objectName)
        {
            foreach (GameObject rootObject in gameObject.scene.GetRootGameObjects())
            {
                Transform found = FindRecursive(rootObject.transform, objectName);
                if (found != null) return found;
            }
            return null;
        }

        private static Transform FindRecursive(Transform current, string objectName)
        {
            if (current.name == objectName) return current;
            foreach (Transform child in current)
            {
                Transform found = FindRecursive(child, objectName);
                if (found != null) return found;
            }
            return null;
        }

        private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);
    }
}
