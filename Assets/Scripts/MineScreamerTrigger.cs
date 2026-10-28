
using StarterAssets;
using UnityEngine;

public class MineScreamerTrigger : MonoBehaviour
{
    [Header("Probabilidad del evento")]
    [Range(0f, 1f)]
    public float activationChance = 0.15f;

    [Header("Punto de aparición")]
    public Transform spawnPoint;

    [Header("Animaciones del Muki")]
    public TipoAnimacionMuki tipoAnimacion;

    public AnimationClip asomada;
    public AnimationClip correteada;
    public AnimationClip mukisusto;

    [Header("Objeto que aparecerá")]
    public GameObject apparition;

    [Header("Sonido del evento")]
    public AudioClip eventSound;

    [Header("Duración del evento")]
    [Min(0.1f)]
    public float eventDuration = 1.2f;

    private bool triggerConsumed;

    public enum TipoAnimacionMuki
    {
        Asomada,
        Correteada,
        MukiSusto
    }

    private void Awake()
    {
        // La aparición debe permanecer oculta hasta que ocurra el evento.
        if (apparition != null)
        {
            apparition.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerConsumed)
            return;

        // Identifica al jugador mediante su componente PlayerRespawn.
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();

        if (player == null)
            return;

        MineScreamerManager manager = MineScreamerManager.Instance;

        if (manager == null)
        {
            Debug.LogWarning(
                "MineScreamerTrigger: no se encontró MineScreamerManager en la escena.",
                this
            );
            return;
        }

        Transform playerTransform = player.transform;

        // Primero verifica que el Muki esté suficientemente lejos
        // y que no haya otro evento activo.
        // Si no se cumple, no consume el trigger ni calcula la probabilidad.
        if (!manager.CanPlayEvent(playerTransform))
            return;

        // Solo se calcula la probabilidad cuando las condiciones son válidas.
        if (Random.value > activationChance)
        {
            // Este trigger ya tuvo su oportunidad y no activó el evento.
            triggerConsumed = true;
            Destroy(gameObject);
            return;
        }

        AnimationClip selectedClip = GetSelectedAnimation();

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "MineScreamerTrigger: falta asignar el Spawn Point.",
                this
            );
            return;
        }

        if (apparition == null)
        {
            Debug.LogWarning(
                "MineScreamerTrigger: falta asignar el objeto Apparition.",
                this
            );
            return;
        }

        if (selectedClip == null)
        {
            Debug.LogWarning(
                "MineScreamerTrigger: la animación seleccionada no está asignada.",
                this
            );
            return;
        }

        // El manager vuelve a comprobar las condiciones antes de iniciar.
        bool eventStarted = manager.TryPlayEvent(
            playerTransform,
            spawnPoint,
            selectedClip,
            apparition,
            null,
            eventSound,
            eventDuration
        );

        if (eventStarted)
        {
            triggerConsumed = true;
            Destroy(gameObject);
        }
    }

    private AnimationClip GetSelectedAnimation()
    {
        switch (tipoAnimacion)
        {
            case TipoAnimacionMuki.Asomada:
                return asomada;

            case TipoAnimacionMuki.Correteada:
                return correteada;

            case TipoAnimacionMuki.MukiSusto:
                return mukisusto;

            default:
                return null;
        }
    }
}
