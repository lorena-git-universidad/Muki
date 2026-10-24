using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Muki.UI
{
    /// <summary>
    /// Pantalla "Cómo se juega". Se muestra al empezar el nivel (con el juego detenido hasta que el
    /// jugador pulse Enter o el botón) y también se puede abrir desde el menú de pausa.
    /// </summary>
    public sealed class ControlsPanel : MonoBehaviour
    {
        public static ControlsPanel Instance { get; private set; }

        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text closeLabel;
        [SerializeField] private TMP_Text hintLabel;
        [SerializeField] private bool showOnLevelStart = true;

        private bool openedAsIntro;
        private StarterAssets.FirstPersonController playerController;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            Instance = this;
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Start y no Awake: el menú de pausa bloquea el cursor en su Awake y esto debe ir después.
        private void Start()
        {
            if (showOnLevelStart) OpenAsIntro();
        }

        private void Update()
        {
            if (!IsOpen) return;

            // Otros scripts (inventario, controlador del jugador) vuelven a bloquear el cursor al
            // iniciar la escena; mientras esta pantalla esté abierta se mantiene libre.
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Keyboard.current == null) return;

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                Close();
        }

        public void OpenAsIntro()
        {
            openedAsIntro = true;
            Show("COMENZAR", "ENTER  ·  COMENZAR");

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Con el cursor libre, mover el mouse giraría la cámara por debajo de la pantalla.
            playerController = FindFirstObjectByType<StarterAssets.FirstPersonController>();
            if (playerController != null) playerController.enabled = false;
        }

        /// <summary>Se llama desde el botón "Cómo se juega" del menú de pausa (el juego ya está detenido).</summary>
        public void OpenFromPause()
        {
            openedAsIntro = false;
            Show("VOLVER", "ESC  ·  VOLVER");
        }

        public void Close()
        {
            if (!IsOpen) return;
            root.SetActive(false);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            // Al empezar el nivel se reanuda el juego; desde la pausa se vuelve al menú de pausa.
            if (openedAsIntro)
            {
                if (playerController != null) playerController.enabled = true;
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Show(string buttonText, string hint)
        {
            if (closeLabel != null) closeLabel.text = buttonText;
            if (hintLabel != null) hintLabel.text = hint;
            if (root != null) root.SetActive(true);
        }
    }
}
