using UnityEngine;

namespace StarterAssets
{
    public class MineableRock : MonoBehaviour
    {
        [Header("Mining")]
        public float requiredMiningTime = 5f;

        [Header("Drops")]
        public ItemData droppedItem;
        public GameObject miniRockPrefab;
        public int miniRocksToSpawn = 4;
        public float spawnRadius = 0.4f;
        public float spawnForce = 3f;

        private float currentMiningTime;

        public bool Mine(float deltaTime)
        {
            currentMiningTime += deltaTime;

            if (currentMiningTime >= requiredMiningTime)
            {
                BreakRock();
                return true;
            }

            return false;
        }

        public void ResetMiningProgress()
        {
            currentMiningTime = 0f;
        }

        private void BreakRock()
        {
            if (miniRockPrefab == null)
            {
                Debug.LogWarning("No hay MiniRockPrefab asignado.");
                return;
            }

            for (int i = 0; i < miniRocksToSpawn; i++)
            {
                Vector3 spawnPos =
                    transform.position +
                    Vector3.up * 0.4f +
                    Random.insideUnitSphere * spawnRadius;

                GameObject rock = Instantiate(
                    miniRockPrefab,
                    spawnPos,
                    Random.rotation);

                //--------------------------------------------------
                // Asignar el ItemData al recolectable
                //--------------------------------------------------

                CollectibleRock collectible = rock.GetComponent<CollectibleRock>();

                if (collectible != null)
                {
                    collectible.itemData = droppedItem;
                }

                //--------------------------------------------------
                // Física de caída
                //--------------------------------------------------

                Rigidbody rb = rock.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 force = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.8f, 1.4f),
                        Random.Range(-1f, 1f));

                    rb.AddForce(force * spawnForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
                }
            }

            Destroy(gameObject);
        }
    }
}