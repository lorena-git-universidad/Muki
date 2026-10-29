using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Muki.UI
{
    /// <summary>
    /// Añade el sonido de clic a TODOS los botones de cualquier escena al cargarla, incluidos los que
    /// empiezan desactivados (pausa, controles). No hace falta tocar cada botón ni cada escena.
    /// Button.onClick solo se dispara si el botón es interactuable, así que los bloqueados no suenan.
    /// </summary>
    public static class ButtonSoundInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // La primera escena ya se cargó antes de suscribirse.
            Install();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Install();

        private static void Install()
        {
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                // Se quita antes de añadir para no duplicar el sonido si el botón ya estaba instalado.
                button.onClick.RemoveListener(PlayClick);
                button.onClick.AddListener(PlayClick);
            }
        }

        private static void PlayClick() => UISound.PlayClick();
    }
}
