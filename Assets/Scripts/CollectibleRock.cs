
using UnityEngine;

namespace StarterAssets
{
    public class CollectibleRock : MonoBehaviour
    {
        public string rockName = "Roca";
        public ItemData itemData;

        public float pickupDelay = 0.5f;

        private float spawnTime;

        private void Start()
        {
            spawnTime = Time.time;

            SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = 1.2f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time - spawnTime < pickupDelay)
                return;

            if (IsPlayer(other))
            {
                TryCollectRock();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time - spawnTime < pickupDelay)
                return;

            if (IsPlayer(collision.collider))
            {
                TryCollectRock();
            }
        }

        private bool IsPlayer(Collider other)
        {
            return other.CompareTag("Player") ||
                   other.GetComponent<CharacterController>() != null ||
                   other.GetComponentInParent<StarterAssetsInputs>() != null;
        }

        private void TryCollectRock()
        {
            if (InventoryManager.Instance == null)
                return;

            if (InventoryManager.Instance.AddItem(itemData))
            {
                // Reproducir el sonido cuando la roca se recoge correctamente.
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("Recoger_Objeto");
                }

                Destroy(gameObject);
            }
        }
    }
}
