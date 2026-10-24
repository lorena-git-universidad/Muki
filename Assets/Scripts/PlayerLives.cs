
using UnityEngine;

namespace StarterAssets
{
    public class PlayerLives : MonoBehaviour
    {
        public static PlayerLives Instance;

        [Header("Vidas del jugador")]
        [Min(1)]
        public int startingLives = 3;

        public int CurrentLives { get; private set; }

        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
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
                    "DERROTA: el Muki atrapó al jugador tres veces."
                );

                return true;
            }

            return false;
        }
    }
}
