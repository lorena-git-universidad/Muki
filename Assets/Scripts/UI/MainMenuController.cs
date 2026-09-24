using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Muki.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string levelSelectionScene = "SeleccionarNivel";

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                Play();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Quit();
        }

        public void Play()
        {
            MainMenuIntroAnimation intro = FindFirstObjectByType<MainMenuIntroAnimation>();
            if (intro != null)
            {
                intro.BeginExit(LoadLevelSelection);
                return;
            }

            LoadLevelSelection();
        }

        private void LoadLevelSelection()
        {
            if (Application.CanStreamedLevelBeLoaded(levelSelectionScene))
                SceneManager.LoadScene(levelSelectionScene);
            else
                Debug.LogError($"No se encontró la escena '{levelSelectionScene}'. Añádela a Build Settings.");
        }

        public void Quit()
        {
            Debug.Log("Saliendo de Bajo la Leyenda...");
            Application.Quit();
        }
    }
}
