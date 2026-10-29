using System.Collections;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

namespace Muki.UI
{
    /// <summary>
    /// Muestra las vidas del jugador como corazones. Al perder una, el corazón tiembla un momento
    /// y cambia al sprite de "vida perdida". Lee PlayerLives, que ya lleva la cuenta de las capturas.
    /// </summary>
    public sealed class LivesHUD : MonoBehaviour
    {
        [SerializeField] private Image[] hearts;
        [SerializeField] private Sprite fullHeart;

        [Header("Al perder una vida: tiembla y luego desaparece")]
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float shakeDuration = 0.55f;
        [SerializeField] private float shakeDistance = 7f;
        [SerializeField] private float shakeAngle = 9f;
        [SerializeField] private float shakeSpeed = 48f;

        private int shownLives = -1;

        private void Update()
        {
            PlayerLives lives = PlayerLives.Instance;
            if (lives == null) return;

            int current = Mathf.Clamp(lives.CurrentLives, 0, hearts.Length);

            // Primera lectura: se dibuja el estado actual sin animar.
            if (shownLives < 0)
            {
                shownLives = current;
                for (int i = 0; i < hearts.Length; i++)
                    SetHeartVisible(hearts[i], i < current);
                return;
            }

            if (current == shownLives) return;

            // Se pierde de derecha a izquierda: el último corazón lleno es el primero en apagarse.
            for (int i = shownLives - 1; i >= current; i--)
                StartCoroutine(LoseHeart(i));

            for (int i = shownLives; i < current; i++) // por si algún día se recupera una vida
                SetHeartVisible(hearts[i], true);

            shownLives = current;
        }

        private IEnumerator LoseHeart(int index)
        {
            RectTransform rect = (RectTransform)hearts[index].transform;
            // La posición de reposo se toma aquí: el contenedor ya acomodó los corazones.
            Vector2 rest = rect.anchoredPosition;

            for (float t = 0f; t < shakeDuration; t += Time.unscaledDeltaTime)
            {
                float fade = 1f - t / shakeDuration;       // el temblor se va apagando
                float wave = Mathf.Sin(t * shakeSpeed);
                float wave2 = Mathf.Cos(t * shakeSpeed * 1.3f);

                rect.anchoredPosition = rest + new Vector2(wave, wave2 * 0.6f) * shakeDistance * fade;
                rect.localRotation = Quaternion.Euler(0f, 0f, wave2 * shakeAngle * fade);
                yield return null;
            }

            rect.anchoredPosition = rest;
            rect.localRotation = Quaternion.identity;

            // Después del temblor el corazón se desvanece y se encoge un poco hasta desaparecer.
            Image heart = hearts[index];
            for (float t = 0f; t < fadeOutDuration; t += Time.unscaledDeltaTime)
            {
                float k = t / fadeOutDuration;
                heart.color = new Color(1f, 1f, 1f, 1f - k);
                rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, k);
                yield return null;
            }

            SetHeartVisible(heart, false);
        }

        // Oculto con enabled=false: el hueco sigue en la fila y los demás corazones no se mueven.
        private void SetHeartVisible(Image heart, bool visible)
        {
            heart.sprite = fullHeart;
            heart.enabled = visible;
            heart.color = Color.white;
            heart.rectTransform.localScale = Vector3.one;
            heart.rectTransform.localRotation = Quaternion.identity;
        }
    }
}
