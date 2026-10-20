using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Muki.UI
{
    [Serializable]
    public class Thought
    {
        [TextArea(2, 5)] public string text;
        [Tooltip("Expresión del jugador (opcional). Si es null se mantiene la anterior.")]
        public Sprite expression;
    }

    /// <summary>
    /// Cuadro de pensamiento en la parte inferior de la pantalla: retrato a la izquierda
    /// y texto con efecto máquina de escribir. Los pensamientos se encolan.
    /// </summary>
    public sealed class ThoughtDialogUI : MonoBehaviour
    {
        public static ThoughtDialogUI Instance { get; private set; }

        [Header("Referencias")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text thoughtText;

        [Header("Animación")]
        [SerializeField] private float charsPerSecond = 40f;
        [SerializeField] private float fadeDuration = 0.35f;
        [SerializeField] private float slideDistance = 40f;
        [SerializeField] private float holdAfterTyping = 2.5f;

        private readonly Queue<Thought> queue = new Queue<Thought>();
        private Coroutine routine;
        private RectTransform groupRect;
        private Vector2 shownPosition;

        private void Awake()
        {
            Instance = this;

            if (group == null) group = GetComponent<CanvasGroup>();
            groupRect = group != null ? group.transform as RectTransform : null;
            if (groupRect != null) shownPosition = groupRect.anchoredPosition;

            SetVisible(0f);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(IEnumerable<Thought> thoughts)
        {
            if (thoughts == null) return;
            foreach (Thought t in thoughts)
                if (t != null && !string.IsNullOrWhiteSpace(t.text)) queue.Enqueue(t);

            if (routine == null && queue.Count > 0)
                routine = StartCoroutine(Run());
        }

        public void Show(Thought thought) => Show(new[] { thought });

        private IEnumerator Run()
        {
            yield return Fade(0f, 1f);

            while (queue.Count > 0)
            {
                Thought t = queue.Dequeue();

                if (portraitImage != null && t.expression != null)
                {
                    portraitImage.sprite = t.expression;
                    portraitImage.enabled = true;
                }

                thoughtText.text = t.text;
                thoughtText.maxVisibleCharacters = 0;
                thoughtText.ForceMeshUpdate();
                int total = thoughtText.textInfo.characterCount;

                float shown = 0f;
                while (shown < total)
                {
                    shown += charsPerSecond * Time.unscaledDeltaTime;
                    thoughtText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
                    yield return null;
                }

                thoughtText.maxVisibleCharacters = total;
                yield return new WaitForSecondsRealtime(holdAfterTyping);
            }

            yield return Fade(1f, 0f);
            routine = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                SetVisible(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / fadeDuration)));
                yield return null;
            }
            SetVisible(to);
        }

        private void SetVisible(float a)
        {
            if (group == null) return;
            group.alpha = a;
            if (groupRect != null)
                groupRect.anchoredPosition = shownPosition + Vector2.down * slideDistance * (1f - a);
        }
    }
}
