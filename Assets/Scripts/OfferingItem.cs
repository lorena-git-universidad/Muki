using UnityEngine;

namespace StarterAssets
{
    public class OfferingItem : MonoBehaviour
    {
        [Header("Offering")]
        public string offeringName = "Ofrenda";

        [Header("Hand Position")]
        public Vector3 handPosition = Vector3.zero;
        public Vector3 handRotation = Vector3.zero;

        [HideInInspector]
        public bool isBeingHeld = false;

        private Collider[] colliders;
        private Rigidbody rb;

        private Vector3 originalScale;

        private SpriteRenderer[] spriteRenderers;

        private void Awake()
        {
            colliders = GetComponentsInChildren<Collider>();
            rb = GetComponent<Rigidbody>();

            originalScale = transform.localScale;

            spriteRenderers =
                GetComponentsInChildren<SpriteRenderer>();
        }

        public void PickUp(Transform holder)
        {
            if (isBeingHeld)
                return;

            isBeingHeld = true;

            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Ocultar la ofrenda física.
            SetVisible(false);

            transform.SetParent(holder, false);

            // Mantener escala original del prefab
            transform.localScale = originalScale;

            transform.localPosition = handPosition;
            transform.localRotation =
                Quaternion.Euler(handRotation);

            Debug.Log(
                "Ofrenda recogida: " +
                offeringName
            );
        }

        public void PlaceOnAltar(Transform altarPoint)
        {
            if (altarPoint == null)
            {
                Debug.LogError(
                    "No hay OfferingPoint en el altar."
                );

                return;
            }

            isBeingHeld = false;

            transform.SetParent(
                altarPoint,
                false
            );

            // Mantener escala original
            transform.localScale = originalScale;

            transform.localPosition = Vector3.zero;
            transform.localRotation =
                Quaternion.identity;

            // Volver a mostrar la ofrenda física.
            SetVisible(true);

            Debug.Log(
                "Ofrenda colocada en el altar."
            );
        }

        public void SetVisible(bool visible)
        {
            if (spriteRenderers == null)
                return;

            foreach (SpriteRenderer sprite in spriteRenderers)
            {
                if (sprite != null)
                    sprite.enabled = visible;
            }
        }
    }
}

