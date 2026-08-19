using UnityEngine;

namespace StarterAssets
{
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint")]
        public Transform respawnPoint;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (PlayerRespawn.Instance != null)
            {
                PlayerRespawn.Instance.SetCheckpoint(this);

                Debug.Log(
                    "Checkpoint activado: " +
                    gameObject.name
                );
            }
        }

        public Vector3 GetRespawnPosition()
        {
            if (respawnPoint != null)
                return respawnPoint.position;

            return transform.position;
        }

        public Quaternion GetRespawnRotation()
        {
            if (respawnPoint != null)
                return respawnPoint.rotation;

            return transform.rotation;
        }
    }
}