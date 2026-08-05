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

        private PlayerHide playerHide;
        private PlayerMining playerMining;

        private void Awake()
        {
            playerHide = GetComponent<PlayerHide>();
            playerMining = GetComponent<PlayerMining>();

            if (playerMining == null)
            {
                Debug.LogError("Falta el componente PlayerMining en el Player.");
            }

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();

                if (playerCamera == null)
                    playerCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (playerCamera == null)
                return;

            // NO TOCAR (escondite)
            if (playerHide != null && playerHide.IsHidden)
                return;

            if (Keyboard.current == null)
                return;

            if (!Keyboard.current.eKey.wasPressedThisFrame)
                return;

            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
                return;

            Debug.Log("Objeto detectado: " + hit.collider.name);

            //==========================
            // RECOGER PICO
            //==========================

            PickaxeItem pickaxe = hit.collider.GetComponent<PickaxeItem>();

            if (pickaxe == null)
                pickaxe = hit.collider.GetComponentInParent<PickaxeItem>();

            if (pickaxe != null)
            {
                if (!playerMining.hasPickaxe)
                {
                    playerMining.TryEquipPickaxe(pickaxe.gameObject);
                }

                return;
            }

            //==========================
            // ESCONDITE (NO MODIFICADO)
            //==========================

            HideSpot hideSpot = hit.collider.GetComponentInParent<HideSpot>();

            if (hideSpot == null)
            {
                hideSpot = hit.collider.GetComponentInChildren<HideSpot>();
            }

            if (hideSpot != null && playerHide != null)
            {
                Debug.Log("Interactuando con HideSpot");
                hideSpot.Interact(playerHide);
                return;
            }
        }
    }
}