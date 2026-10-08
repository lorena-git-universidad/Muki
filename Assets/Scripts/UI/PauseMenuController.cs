using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Muki.UI
{
    /// <summary>
    /// Controls the in-game pause overlay. The panel is intentionally kept in
    /// the scene hierarchy so it can be edited visually and starts hidden.
    /// </summary>
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private string mainMenuScene = "MenuPrincipal";

        private bool isPaused;

        private void Awake()
        {
            isPaused = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            SetCursorLocked(true);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();
        }

        public void TogglePause()
        {
            if (isPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            if (isPaused) return;

            isPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
            SetCursorLocked(false);
            SelectFirstButton();
        }

        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            SetCursorLocked(true);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetCursorLocked(false);

            if (Application.CanStreamedLevelBeLoaded(mainMenuScene))
                SceneManager.LoadScene(mainMenuScene);
            else
                Debug.LogError($"No se encontró la escena '{mainMenuScene}'. Añádela a Build Settings.");
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
            Debug.Log("Saliendo de Bajo la Leyenda...");
            Application.Quit();
        }

        private void SelectFirstButton()
        {
            if (pausePanel == null) return;

            Transform firstButton = pausePanel.transform.Find("Panel_Pausa/Botones/Reanudar_Button");
            if (firstButton == null) firstButton = pausePanel.transform.Find("Panel_Pausa/Botones");
            if (firstButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.visible = !locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
