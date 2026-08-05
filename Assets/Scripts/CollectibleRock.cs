using UnityEngine;

namespace StarterAssets
{
    public class CollectibleRock : MonoBehaviour
    {
        public string rockName = "Roca";
        public float pickupDelay = 0.5f; // Wait half a second before pickup allowed so it drops naturally first
        private float spawnTime;

        private void Start()
        {
            spawnTime = Time.time;

            // Ensure there is a trigger collider for collection
            SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = 1.2f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time - spawnTime < pickupDelay) return;

            if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.GetComponentInParent<StarterAssetsInputs>() != null)
            {
                Debug.Log($"¡Recolectada {rockName}!");
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time - spawnTime < pickupDelay) return;

            Collider other = collision.collider;
            if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.GetComponentInParent<StarterAssetsInputs>() != null)
            {
                Debug.Log($"¡Recolectada {rockName}!");
                Destroy(gameObject);
            }
        }
    }
}
