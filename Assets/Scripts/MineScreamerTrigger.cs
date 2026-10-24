
using UnityEngine;

namespace StarterAssets
{
    public class MineScreamerTrigger : MonoBehaviour
    {
        [Header("Probabilidad")]
        [Range(0f, 1f)]
        public float activationChance = 0.15f;

        [Header("Punto de aparición")]
        [Tooltip("Transform que determina dónde y cómo aparece el susto.")]
        public Transform spawnPoint;

        [Header("Imagen provisional")]
        public Sprite eventImage;

        [Header("Aparición opcional")]
        [Tooltip("Modelo visual independiente, inicialmente desactivado.")]
        public GameObject apparition;

        [Header("Sonido")]
        public AudioClip eventSound;

        [Header("Duración")]
        [Min(0.1f)]
        public float eventDuration = 1.2f;

        private bool hasAttempted;

        private void Start()
        {
            if (apparition != null)
                apparition.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasAttempted)
                return;

            PlayerRespawn playerRespawn =
                other.GetComponentInParent<PlayerRespawn>();

            if (playerRespawn == null)
                return;

            Transform playerTransform = playerRespawn.transform;

            MineScreamerManager manager =
                MineScreamerManager.Instance;

            if (manager == null)
            {
                Debug.LogError(
                    "No existe MineScreamerManager en la escena."
                );

                return;
            }

            // Si el Muki está cerca o hay otro susto activo,
            // no consumimos todavía el punto.
            if (!manager.CanPlayEvent(playerTransform))
                return;

            if (spawnPoint == null)
            {
                Debug.LogError(
                    "Asigna un Spawn Point a " + gameObject.name
                );

                return;
            }

            // Un solo intento de probabilidad por punto.
            if (Random.value > activationChance)
            {
                hasAttempted = true;

                Debug.Log(
                    "El evento no ocurrió. Punto consumido: " +
                    gameObject.name
                );

                Destroy(gameObject);
                return;
            }

            bool started = manager.TryPlayEvent(
                playerTransform,
                spawnPoint,
                eventImage,
                apparition,
                eventSound,
                eventDuration
            );

            if (started)
            {
                hasAttempted = true;

                Debug.Log(
                    "Evento activado: " + gameObject.name
                );

                Destroy(gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (spawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(
                    transform.position,
                    spawnPoint.position
                );

                Gizmos.DrawWireSphere(
                    spawnPoint.position,
                    0.25f
                );

                Gizmos.DrawRay(
                    spawnPoint.position,
                    spawnPoint.forward * 1.5f
                );
            }
        }
    }
}
