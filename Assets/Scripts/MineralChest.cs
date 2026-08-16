using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class MineralChest : MonoBehaviour
    {
        [Header("Chest Settings")]
        public int maxMinerals = 100;

        [Header("Interaction")]
        public float interactDistance = 3.5f;
        public LayerMask playerLayer;

        [Header("UI")]
        public MineralChestUI chestUI;

        [Header("State")]
        public int currentMinerals = 0;

        private Camera playerCamera;

        private void Start()
        {
            playerCamera = Camera.main;

            if (chestUI != null)
            {
                chestUI.UpdateText(currentMinerals, maxMinerals);
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (!Keyboard.current.eKey.wasPressedThisFrame)
                return;

            TryDepositMinerals();
        }

        private void TryDepositMinerals()
        {
            if (currentMinerals >= maxMinerals)
            {
                Debug.Log("El cofre ya está lleno.");
                return;
            }

            if (playerCamera == null)
                playerCamera = Camera.main;

            if (playerCamera == null)
                return;

            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactDistance,
                    playerLayer))
            {
                return;
            }

            MineralChest chest =
                hit.collider.GetComponentInParent<MineralChest>();

            if (chest != this)
                return;

            DepositFromInventory();
        }

        private void DepositFromInventory()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("No existe InventoryManager.");
                return;
            }

            int spaceAvailable =
                maxMinerals - currentMinerals;

            if (spaceAvailable <= 0)
                return;

            int deposited =
                InventoryManager.Instance.RemoveMinerals(spaceAvailable);

            if (deposited <= 0)
            {
                Debug.Log("No tienes minerales para depositar.");
                return;
            }

            currentMinerals += deposited;

            if (chestUI != null)
            {
                chestUI.UpdateText(
                    currentMinerals,
                    maxMinerals
                );
            }

            Debug.Log(
                "Minerales depositados: " +
                deposited +
                " | Cofre: " +
                currentMinerals +
                "/" +
                maxMinerals
            );
        }
    }
}