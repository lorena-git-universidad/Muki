
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Interaction")]
        public float interactDistance = 3.5f;
        public LayerMask interactLayer;

        [Header("References")]
        public Camera playerCamera;

        [Header("Offering")]
        public Transform offeringHolder;

        private OfferingItem heldOffering;

        private PlayerHide playerHide;
        private PlayerMining playerMining;

        // ==========================================
        // MANO IZQUIERDA
        // ==========================================

        private Animator leftHandAnimator;

        private void Awake()
        {
            playerHide = GetComponent<PlayerHide>();
            playerMining = GetComponent<PlayerMining>();

            if (playerMining == null)
            {
                Debug.LogError(
                    "Falta el componente PlayerMining en el Player."
                );
            }

            if (playerCamera == null)
            {
                playerCamera =
                    GetComponentInChildren<Camera>();

                if (playerCamera == null)
                    playerCamera = Camera.main;
            }

            // ==========================================
            // BUSCAR OFFERING HOLDER
            // ==========================================

            if (offeringHolder == null &&
                playerCamera != null)
            {
                offeringHolder =
                    playerCamera.transform.Find(
                        "LeftHandPoint"
                    );
            }

            // ==========================================
            // BUSCAR MANO IZQUIERDA
            // ==========================================

            if (offeringHolder != null)
            {
                Transform leftHand =
                    offeringHolder.Find(
                        "ManoIzquierda"
                    );

                if (leftHand != null)
                {
                    leftHandAnimator =
                        leftHand.GetComponent<Animator>();
                }
            }

            if (leftHandAnimator == null)
            {
                Debug.LogWarning(
                    "No se encontró el Animator de ManoIzquierda."
                );
            }
        }

        private void Update()
        {
            if (playerCamera == null)
                return;

            // ==========================================
            // NO TOCAR - ESCONDITE
            // ==========================================

            if (playerHide != null &&
                playerHide.IsHidden)
                return;

            if (Keyboard.current == null)
                return;

            if (!Keyboard.current.eKey.wasPressedThisFrame)
                return;

            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactDistance,
                    interactLayer))
                return;

            Debug.Log(
                "Objeto detectado: " +
                hit.collider.name
            );

            // ==========================================
            // PICO
            // ==========================================

            PickaxeItem pickaxe =
                hit.collider.GetComponent<PickaxeItem>();

            if (pickaxe == null)
            {
                pickaxe =
                    hit.collider.GetComponentInParent<PickaxeItem>();
            }

            if (pickaxe != null)
            {
                if (!playerMining.hasPickaxe)
                {
                    bool equipped =
                        playerMining.TryEquipPickaxe(
                            pickaxe.gameObject
                        );

                    // ==========================================
                    // REPRODUCIR ANIMACIÓN DE AGARRAR
                    // ==========================================

                    if (equipped &&
                        playerMining.PickaxeAnimator != null)
                    {
                        playerMining.PickaxeAnimator.Play(
                            "agarre_manoDer"
                        );
                    }
                }

                return;
            }

            // ==========================================
            // OFRENDA EN EL MUNDO
            // ==========================================

            OfferingItem offering =
                hit.collider.GetComponent<OfferingItem>();

            if (offering == null)
            {
                offering =
                    hit.collider.GetComponentInParent<OfferingItem>();
            }

            if (offering != null &&
                heldOffering == null)
            {
                if (offeringHolder == null)
                {
                    Debug.LogError(
                        "No se encontró LeftHandPoint."
                    );

                    return;
                }

                heldOffering = offering;

                // Recoger físicamente la ofrenda
                offering.PickUp(offeringHolder);

                // Reproducir animación correspondiente
                PlayOfferingGrabAnimation(
                    offering.offeringName
                );

                return;
            }

            // ==========================================
            // ALTAR
            // ==========================================

            OfferingAltar altar =
                hit.collider.GetComponent<OfferingAltar>();

            if (altar == null)
            {
                altar =
                    hit.collider.GetComponentInParent<OfferingAltar>();
            }

            if (altar != null)
            {
                if (heldOffering != null)
                {
                    bool placed =
                        altar.PlaceOffering(
                            heldOffering
                        );

                    if (placed)
                    {
                        // Animación de soltar
                        PlayOfferingReleaseAnimation();

                        heldOffering = null;
                    }
                }

                return;
            }

            // ==========================================
            // ESCONDITE
            // NO MODIFICADO
            // ==========================================

            HideSpot hideSpot =
                hit.collider.GetComponentInParent<HideSpot>();

            if (hideSpot == null)
            {
                hideSpot =
                    hit.collider.GetComponentInChildren<HideSpot>();
            }

            if (hideSpot != null &&
                playerHide != null)
            {
                Debug.Log(
                    "Interactuando con HideSpot"
                );

                hideSpot.Interact(playerHide);

                return;
            }
        }

        // ==========================================
        // ANIMACIÓN DE AGARRE
        // ==========================================

        private void PlayOfferingGrabAnimation(
            string offeringName
        )
        {
            if (leftHandAnimator == null)
                return;

            switch (offeringName)
            {
                case "Alcohol":

                    leftHandAnimator.Play(
                        "agarreAlcohol_manoIzq"
                    );

                    break;

                case "Coca":

                    leftHandAnimator.Play(
                        "agarreCoca_manoIzq"
                    );

                    break;

                case "Tabaco":

                    leftHandAnimator.Play(
                        "agarreTabaco_manoIzq"
                    );

                    break;

                default:

                    Debug.LogWarning(
                        "No existe animación para la ofrenda: " +
                        offeringName
                    );

                    break;
            }
        }

        // ==========================================
        // ANIMACIÓN DE SOLTAR
        // ==========================================

        private void PlayOfferingReleaseAnimation()
        {
            if (leftHandAnimator == null)
                return;

            leftHandAnimator.Play(
                "soltar_manoIzq"
            );
        }
    }
}

