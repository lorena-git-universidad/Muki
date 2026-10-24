
using System.Collections;
using UnityEngine;

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
        [Tooltip("Prefab que contiene un SpriteRenderer.")]
        public GameObject screamerSpritePrefab;

        [Header("Duración")]
        [Min(0.1f)]
        public float defaultDuration = 1.2f;

        public bool IsPlaying { get; private set; }

        private GameObject currentScreamer;
        private Coroutine activeRoutine;

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

            if (mainMuki != null)
            {
                float distance = Vector3.Distance(
                    playerTransform.position,
                    mainMuki.position
                );

                if (distance < mukiSafeDistance)
                {
                    Debug.Log(
                        "Susto omitido: el Muki está demasiado cerca."
                    );

                    return false;
                }
            }

            return true;
        }

        public bool TryPlayEvent(
            Transform playerTransform,
            Transform spawnPoint,
            Sprite eventSprite,
            GameObject apparition,
            AudioClip sound,
            float duration)
        {
            if (!CanPlayEvent(playerTransform))
                return false;

            if (spawnPoint == null)
            {
                Debug.LogError(
                    "Este evento no tiene un punto de aparición asignado."
                );

                return false;
            }

            if (duration <= 0f)
                duration = defaultDuration;

            IsPlaying = true;

            activeRoutine = StartCoroutine(
                PlayEventRoutine(
                    spawnPoint,
                    eventSprite,
                    apparition,
                    sound,
                    duration
                )
            );

            return true;
        }

        private IEnumerator PlayEventRoutine(
            Transform spawnPoint,
            Sprite eventSprite,
            GameObject apparition,
            AudioClip sound,
            float duration)
        {
            // Aparición provisional mediante un SpriteRenderer.
            if (eventSprite != null)
            {
                if (screamerSpritePrefab != null)
                {
                    currentScreamer = Instantiate(
                        screamerSpritePrefab,
                        spawnPoint.position,
                        spawnPoint.rotation
                    );

                    SpriteRenderer renderer =
                        currentScreamer.GetComponentInChildren<SpriteRenderer>();

                    if (renderer != null)
                    {
                        renderer.sprite = eventSprite;
                        renderer.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning(
                            "El prefab del susto no contiene SpriteRenderer."
                        );
                    }
                }
                else
                {
                    currentScreamer =
                        new GameObject("ActiveWorldScreamer");

                    currentScreamer.transform.SetPositionAndRotation(
                        spawnPoint.position,
                        spawnPoint.rotation
                    );

                    SpriteRenderer renderer =
                        currentScreamer.AddComponent<SpriteRenderer>();

                    renderer.sprite = eventSprite;
                    renderer.sortingOrder = 100;
                }
            }

            // Aparición opcional preparada en la escena.
            if (apparition != null)
            {
                apparition.transform.SetPositionAndRotation(
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                apparition.SetActive(true);
            }

            // Audio opcional.
            if (sound != null)
            {
                AudioSource source = GetComponent<AudioSource>();

                if (source != null)
                    source.PlayOneShot(sound);
            }

            Debug.Log(
                "Susto activado en el punto: " + spawnPoint.name
            );

            yield return new WaitForSeconds(duration);

            if (currentScreamer != null)
                Destroy(currentScreamer);

            currentScreamer = null;

            if (apparition != null)
                apparition.SetActive(false);

            IsPlaying = false;
            activeRoutine = null;
        }

        private void OnDestroy()
        {
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            if (currentScreamer != null)
                Destroy(currentScreamer);

            if (Instance == this)
                Instance = null;
        }
    }
}
