
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarterAssets
{
    [Serializable]
    public class ChestMineralStack
    {
        public ItemData item;
        public int amount;

        public ChestMineralStack(ItemData itemData, int quantity)
        {
            item = itemData;
            amount = quantity;
        }
    }

    public class MineralChest : MonoBehaviour
    {
        [Header("Chest Settings")]
        public int maxMinerals = 100;

        [Header("Interaction")]
        public float interactDistance = 3.5f;

        [Header("UI")]
        public MineralChestUI chestUI;

        [Header("Stored Minerals")]
        public List<ChestMineralStack> storedMinerals =
            new List<ChestMineralStack>();

        public int currentMinerals = 0;

        private Camera playerCamera;

        private void Start()
        {
            playerCamera = Camera.main;
            RecalculateTotal();
            UpdateUI();
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null)
                return;

            if (!UnityEngine.InputSystem.Keyboard.current.eKey
                    .wasPressedThisFrame)
                return;

            TryDepositMinerals();
        }

        private void TryDepositMinerals()
        {
            if (currentMinerals >= maxMinerals)
            {
                Debug.Log("El cofre está lleno.");
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
                interactDistance))
                return;

            MineralChest chest =
                hit.collider.GetComponentInParent<MineralChest>();

            if (chest != this)
                return;

            DepositAllFromInventory();
        }

        private void DepositAllFromInventory()
        {
            InventoryManager inventory = InventoryManager.Instance;

            if (inventory == null)
            {
                Debug.LogError("No existe InventoryManager.");
                return;
            }

            int depositedTotal = 0;

            // Recorremos una copia para poder modificar el inventario
            // sin alterar la lista que estamos recorriendo.
            List<InventoryItem> inventorySnapshot =
                new List<InventoryItem>(inventory.items);

            foreach (InventoryItem inventoryItem in inventorySnapshot)
            {
                if (currentMinerals >= maxMinerals)
                    break;

                if (inventoryItem == null || inventoryItem.item == null)
                    continue;

                ItemData mineral = inventoryItem.item;

                int spaceAvailable = maxMinerals - currentMinerals;
                int amountToDeposit = Mathf.Min(
                    inventoryItem.amount,
                    spaceAvailable
                );

                if (amountToDeposit <= 0)
                    continue;

                int actuallyRemoved = inventory.RemoveItem(
                    mineral,
                    amountToDeposit
                );

                if (actuallyRemoved <= 0)
                    continue;

                AddStoredMineral(mineral, actuallyRemoved);
                depositedTotal += actuallyRemoved;
            }

            RecalculateTotal();
            UpdateUI();

            if (depositedTotal <= 0)
            {
                Debug.Log("No tienes minerales para depositar.");
                return;
            }

            Debug.Log(
                "Total depositado: " + depositedTotal +
                " | Cofre: " + currentMinerals + "/" + maxMinerals
            );

            // Comprueba los requisitos usando las cantidades del cofre,
            // no las cantidades que el jugador recogió.
            if (GameVictoryManager.Instance != null)
                GameVictoryManager.Instance.CheckRequirements(this);
        }

        private void AddStoredMineral(ItemData item, int amount)
        {
            foreach (ChestMineralStack stack in storedMinerals)
            {
                if (stack.item == item)
                {
                    stack.amount += amount;
                    return;
                }
            }

            storedMinerals.Add(new ChestMineralStack(item, amount));
        }

        public int GetAmountOf(ItemData item)
        {
            if (item == null)
                return 0;

            foreach (ChestMineralStack stack in storedMinerals)
            {
                if (stack.item == item)
                    return stack.amount;
            }

            return 0;
        }

        private void RecalculateTotal()
        {
            currentMinerals = 0;

            foreach (ChestMineralStack stack in storedMinerals)
            {
                if (stack != null && stack.item != null)
                    currentMinerals += stack.amount;
            }
        }

        private void UpdateUI()
        {
            if (chestUI == null)
                return;

            StringBuilder text = new StringBuilder();

            text.AppendLine(
                "Total: " + currentMinerals + "/" + maxMinerals
            );

            foreach (ChestMineralStack stack in storedMinerals)
            {
                if (stack == null || stack.item == null)
                    continue;

                text.AppendLine(
                    stack.item.itemName + ": " + stack.amount
                );
            }

            chestUI.UpdateContents(text.ToString());
        }
    }
}