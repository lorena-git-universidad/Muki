using UnityEngine;

namespace StarterAssets
{
    public class PlayerRespawn : MonoBehaviour
    {
        public static PlayerRespawn Instance;

        [Header("Default Respawn")]
        public Transform defaultSpawnPoint;

        [Header("References")]
        public CharacterController controller;
        public FirstPersonController movement;
        public PlayerHide playerHide;

        private Checkpoint currentCheckpoint;

        private void Awake()
        {
            Instance = this;

            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (movement == null)
                movement = GetComponent<FirstPersonController>();

            if (playerHide == null)
                playerHide = GetComponent<PlayerHide>();
        }

        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null)
                return;

            currentCheckpoint = checkpoint;

            Debug.Log(
                "Nuevo checkpoint guardado: " +
                checkpoint.gameObject.name
            );
        }

        public void Respawn()
        {
            Vector3 respawnPosition;
            Quaternion respawnRotation;

            // =====================================
            // ELEGIR PUNTO DE RESPAWN
            // =====================================

            if (currentCheckpoint != null)
            {
                respawnPosition =
                    currentCheckpoint.GetRespawnPosition();

                respawnRotation =
                    currentCheckpoint.GetRespawnRotation();
            }
            else if (defaultSpawnPoint != null)
            {
                respawnPosition =
                    defaultSpawnPoint.position;

                respawnRotation =
                    defaultSpawnPoint.rotation;
            }
            else
            {
                Debug.LogWarning(
                    "No hay Checkpoint ni DefaultSpawnPoint."
                );

                return;
            }

            // =====================================
            // SALIR DEL ESCONDITE SI ESTABA OCULTO
            // =====================================

            if (playerHide != null &&
                playerHide.IsHidden)
            {
                // Como PlayerHide desactiva el CharacterController
                // necesitamos volver a activarlo directamente.
                playerHide.enabled = false;
                playerHide.enabled = true;
            }

            // =====================================
            // MOVER PLAYER
            // =====================================

            if (controller != null)
                controller.enabled = false;

            transform.SetPositionAndRotation(
                respawnPosition,
                respawnRotation
            );

            if (controller != null)
                controller.enabled = true;

            // =====================================
            // ASEGURAR MOVIMIENTO
            // =====================================

            if (movement != null)
                movement.enabled = true;

            Debug.Log(
                "Jugador reapareció en el último checkpoint."
            );
        }
    }
}