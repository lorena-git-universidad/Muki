
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarterAssets
{
    public class PlayerLives : MonoBehaviour
    {
        public static PlayerLives Instance;

        [Header("Vidas del jugador")]
        [Min(1)]
        public int startingLives = 3;

        [Header("Escena de derrota")]
        public string defeatSceneName = "Derrota";

        [Tooltip("Espera alternativa si no hay video final asignado.")]
        public float defeatDelay = 1.5f;

        public int CurrentLives { get; private set; }

        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            CurrentLives = Mathf.Max(1, startingLives);
            IsGameOver = false;

            Debug.Log(
                "Sistema de vidas iniciado: " +
                CurrentLives + " vidas."
            );
        }

        public bool RegisterCapture()
        {
            // No descontar vidas después de la derrota.
            if (IsGameOver)
                return true;

            CurrentLives = Mathf.Max(0, CurrentLives - 1);

            Debug.Log(
                "¡El Muki atrapó al jugador! Vidas restantes: " +
                CurrentLives + "/" + startingLives
            );

            if (CurrentLives <= 0)
            {
                IsGameOver = true;

                Debug.LogError(
                    "DERROTA: el Muki atrapó al jugador por última vez."
                );

                // No cambiar de escena todavía.
                // MukiEnemy reproducirá el video final primero.
                return true;
            }

            return false;
        }

        public void LoadDefeatSceneNow()
        {
            if (Application.CanStreamedLevelBeLoaded(defeatSceneName))
            {
                SceneManager.LoadScene(defeatSceneName);
            }
            else
            {
                Debug.LogError(
                    "No se encontró la escena " + defeatSceneName +
                    ". Agrégala a Build Profiles / Build Settings."
                );
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
