
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace StarterAssets
{
    public class MineScreamerManager : MonoBehaviour
    {
        public static MineScreamerManager Instance { get; private set; }

        [Header("Referencias")]
        public Transform mainMuki;
        public Camera playerCamera;

        [Header("Distancia de seguridad")]
        [Min(0f)]
        public float mukiSafeDistance = 12f;

        [Header("Prefab visual provisional")]
        public GameObject screamerSpritePrefab;

        [Header("Duración")]
        [Min(0.1f)]
        public float defaultDuration = 1.2f;

        public bool IsPlaying { get; private set; }

        private GameObject currentScreamer;
        private GameObject currentApparition;
        private Coroutine activeRoutine;
        private PlayableGraph activeGraph;

        private Renderer[] hiddenMukiRenderers;
        private bool[] previousRendererStates;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError(
                    "Hay más de un MineScreamerManager en la escena."
                );

                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (playerCamera == null)
                playerCamera = Camera.main;

            if (mainMuki == null)
            {
                MukiEnemy muki =
                    FindFirstObjectByType<MukiEnemy>();

                if (muki != null)
                    mainMuki = muki.transform;
            }
        }

        public bool CanPlayEvent(Transform playerTransform)
        {
            if (IsPlaying || playerTransform == null)
                return false;

            if (playerCamera == null)
                return false;

            // No activar eventos si no conocemos la posición del Muki.
            if (mainMuki == null)
                return false;

            float distance = Vector3.Distance(
                playerTransform.position,
                mainMuki.position
            );

            if (distance < mukiSafeDistance)
            {
                return false;
            }

            return true;
        }

        public bool TryPlayEvent(
            Transform playerTransform,
            Transform spawnPoint,
            AnimationClip animationClip,
            GameObject apparition,
            Sprite eventSprite,
            AudioClip sound,
            float duration)
        {
            // La distancia se comprueba antes de aceptar el evento.
            if (!CanPlayEvent(playerTransform))
                return false;

            if (spawnPoint == null ||
                animationClip == null ||
                apparition == null)
            {
                Debug.LogError(
                    "Falta Spawn Point, Animation Clip o Apparition."
                );

                return false;
            }

            Animator animator =
                apparition.GetComponentInChildren<Animator>(true);

            if (animator == null)
            {
                Debug.LogError(
                    "Apparition necesita un Animator en su jerarquía."
                );

                return false;
            }

            if (duration <= 0f)
                duration = defaultDuration;

            IsPlaying = true;
            currentApparition = apparition;

            // El objeto no se muestra hasta iniciar la rutina.
            apparition.SetActive(false);

            activeRoutine = StartCoroutine(
                PlayEventRoutine(
                    spawnPoint,
                    animationClip,
                    apparition,
                    animator,
                    sound,
                    duration
                )
            );

            return true;
        }

        private IEnumerator PlayEventRoutine(
            Transform spawnPoint,
            AnimationClip animationClip,
            GameObject apparition,
            Animator animator,
            AudioClip sound,
            float duration)
        {
            // Ocultar al Muki real sin desactivar su IA.
            SetMainMukiVisible(false, apparition);

            // Preparar y activar la aparición.
            apparition.transform.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation
            );

            apparition.SetActive(true);

            // Reproducir el AnimationClip mediante Playables.
            if (activeGraph.IsValid())
                activeGraph.Destroy();

            activeGraph = PlayableGraph.Create("MukiScreamerGraph");

            activeGraph.SetTimeUpdateMode(
                DirectorUpdateMode.GameTime
            );

            AnimationPlayableOutput output =
                AnimationPlayableOutput.Create(
                    activeGraph,
                    "MukiScreamerOutput",
                    animator
                );

            AnimationClipPlayable clipPlayable =
                AnimationClipPlayable.Create(
                    activeGraph,
                    animationClip
                );

            clipPlayable.SetApplyFootIK(false);
            output.SetSourcePlayable(clipPlayable);

            activeGraph.Play();

            // Sonido opcional.
            if (sound != null)
            {
                AudioSource source = GetComponent<AudioSource>();

                if (source != null)
                    source.PlayOneShot(sound);
                else
                    Debug.LogWarning(
                        "MineScreamerManager no tiene AudioSource."
                    );
            }

            Debug.Log(
                "Susto activado: " + animationClip.name +
                ". Muki principal oculto."
            );

            yield return new WaitForSeconds(duration);

            FinishEvent();
        }

        private void SetMainMukiVisible(
            bool visible,
            GameObject apparition)
        {
            if (mainMuki == null)
                return;

            if (!visible)
            {
                // Guardar el estado original de cada Renderer.
                hiddenMukiRenderers =
                    mainMuki.GetComponentsInChildren<Renderer>(true);

                previousRendererStates =
                    new bool[hiddenMukiRenderers.Length];

                for (int i = 0; i < hiddenMukiRenderers.Length; i++)
                {
                    Renderer rend = hiddenMukiRenderers[i];

                    if (rend == null)
                        continue;

                    previousRendererStates[i] = rend.enabled;

                    // No ocultar la aparición si está dentro
                    // de la jerarquía del Muki principal.
                    if (apparition != null &&
                        (rend.transform == apparition.transform ||
                         rend.transform.IsChildOf(apparition.transform)))
                    {
                        continue;
                    }

                    rend.enabled = false;
                }
            }
            else
            {
                RestoreMainMukiVisibility();
            }
        }

        private void RestoreMainMukiVisibility()
        {
            if (hiddenMukiRenderers != null &&
                previousRendererStates != null)
            {
                for (int i = 0;
                     i < hiddenMukiRenderers.Length;
                     i++)
                {
                    if (hiddenMukiRenderers[i] != null)
                    {
                        hiddenMukiRenderers[i].enabled =
                            previousRendererStates[i];
                    }
                }
            }

            hiddenMukiRenderers = null;
            previousRendererStates = null;
        }

        private void FinishEvent()
        {
            // Destruir el grafo de animación.
            if (activeGraph.IsValid())
                activeGraph.Destroy();

            // Ocultar la aparición.
            if (currentApparition != null)
                currentApparition.SetActive(false);

            if (currentScreamer != null)
                Destroy(currentScreamer);

            currentScreamer = null;
            currentApparition = null;

            // Recuperar la visibilidad original del Muki.
            RestoreMainMukiVisibility();

            IsPlaying = false;
            activeRoutine = null;

            Debug.Log("Evento finalizado. Muki vuelve a ser visible.");
        }

        private void OnDestroy()
        {
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            if (activeGraph.IsValid())
                activeGraph.Destroy();

            if (currentScreamer != null)
                Destroy(currentScreamer);

            if (currentApparition != null)
                currentApparition.SetActive(false);

            RestoreMainMukiVisibility();

            if (Instance == this)
                Instance = null;
        }
    }
}
