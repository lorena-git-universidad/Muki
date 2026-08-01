using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerInteractor : MonoBehaviour
    {
        public float interactDistance = 3f;
        public LayerMask interactLayer;

        public Camera playerCamera;

        void Update()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Ray ray = new Ray(
                    playerCamera.transform.position,
                    playerCamera.transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
                {
                    Debug.Log("Golpeó: " + hit.collider.name);

                    HideSpot hideSpot = hit.collider.GetComponentInParent<HideSpot>();

                    if (hideSpot == null)
                    {
                        hideSpot = hit.collider.GetComponentInChildren<HideSpot>();
                    }

                    if (hideSpot != null)
                    {
                        Debug.Log("Interactuando con HideSpot");
                        PlayerHide playerHide = GetComponent<PlayerHide>();

                        if (playerHide != null)
                        {
                            hideSpot.Interact(playerHide);
                        }
                    }
                }
            }
        }
    }
}