using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Muki.UI
{
    /// <summary>
    /// Pantalla de victoria: los bloques aparecen escalonados (fundido + ligero ascenso)
    /// y se ofrece reiniciar el nivel, volver al menú principal o salir del juego.
    /// </summary>
    public sealed class VictoryMenuController : MonoBehaviour
    {
        [Serializable]
        public class RevealItem
        {
            public CanvasGroup group;
            public float delay;
            public float duration = 0.9f;
            public float riseDistance = 28f;

            [NonSerialized] public Vector2 restPosition;
        }

        [SerializeField] private RevealItem[] reveals;
        [SerializeField] private RectTransform rail;
        [SerializeField] private float railDelay = 0.2f;
        [SerializeField] private float railDuration = 1.2f;
        [SerializeField] private Button firstButton;
        [SerializeField] private string levelScene = "EscenaPrueba";
        [SerializeField] private string mainMenuScene = "MenuPrincipal";

        private float elapsed;

        private void Awake()
        {
            // Viene de una partida que pudo dejar el tiempo detenido y el cursor bloqueado.
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (reveals != null)
            {
                foreach (RevealItem item in reveals)
                {
                    if (item.group == null) continue;
                    RectTransform rect = (RectTransform)item.group.transform;
                    item.restPosition = rect.anchoredPosition;
                    item.group.alpha = 0f;
                    item.group.interactable = false;
                }
            }

            if (rail != null) rail.localScale = new Vector3(1f, 0f, 1f);
        }

        private void Start()
        {
            if (firstButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            AnimateReveals();

            if (Keyboard.current == null) return;

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                RestartLevel();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                GoToMainMenu();
        }

        private void AnimateReveals()
        {
            if (rail != null)
            {
                float t = Mathf.Clamp01((elapsed - railDelay) / railDuration);
                rail.localScale = new Vector3(1f, Mathf.SmoothStep(0f, 1f, t), 1f);
            }

            if (reveals == null) return;

            foreach (RevealItem item in reveals)
            {
                if (item.group == null) continue;

                float t = Mathf.Clamp01((elapsed - item.delay) / Mathf.Max(0.01f, item.duration));
                float eased = Mathf.SmoothStep(0f, 1f, t);
                item.group.alpha = eased;
                ((RectTransform)item.group.transform).anchoredPosition = item.restPosition + Vector2.down * item.riseDistance * (1f - eased);
                item.group.interactable = t >= 1f;
            }
        }

        public void RestartLevel() => Load(levelScene);

        public void GoToMainMenu() => Load(mainMenuScene);

        public void QuitGame()
        {
            Debug.Log("Saliendo de Bajo la Leyenda...");
            Application.Quit();
        }

        private static void Load(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                Debug.LogError($"No se encontró la escena '{sceneName}'. Añádela a Build Settings.");
        }
    }
}
