using UnityEngine;

namespace Muki.UI
{
    /// <summary>
    /// Sonido de clic de los botones. El audio vive en Assets/Resources/UI/Boton y se reproduce
    /// desde un objeto que sobrevive a los cambios de escena, así no se corta al cargar otra escena
    /// y suena también con el juego en pausa.
    ///
    /// Unity no reproduce NADA si la escena no tiene un AudioListener (pasa en MenuPrincipal y
    /// SeleccionarNivel). Por eso este objeto lleva su propio AudioListener, que solo se activa
    /// cuando la escena no tiene otro, para no tener dos a la vez.
    /// </summary>
    public static class UISound
    {
        private const string ClickResource = "UI/Boton";

        private static AudioClip clickClip;
        private static AudioSource source;
        private static AudioListener fallbackListener;

        public static void PlayClick(float volume = 1f)
        {
            if (clickClip == null) clickClip = Resources.Load<AudioClip>(ClickResource);
            if (clickClip == null)
            {
                Debug.LogWarning("UISound: no se encontró Assets/Resources/UI/Boton.");
                return;
            }

            if (source == null)
            {
                GameObject go = new GameObject("UISound");
                Object.DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.ignoreListenerPause = true;
                fallbackListener = go.AddComponent<AudioListener>();
            }

            UpdateFallbackListener();
            source.PlayOneShot(clickClip, volume);
        }

        private static void UpdateFallbackListener()
        {
            bool sceneHasListener = false;
            foreach (AudioListener listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            {
                if (listener == fallbackListener) continue;
                if (listener.isActiveAndEnabled) { sceneHasListener = true; break; }
            }

            fallbackListener.enabled = !sceneHasListener;
        }
    }
}
