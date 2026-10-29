
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

namespace StarterAssets
{
    [Serializable]
    public class MineralRequirement
    {
        public ItemData item;
        [Min(1)] public int requiredAmount = 1;
    }

    public class GameVictoryManager : MonoBehaviour
    {
        public static GameVictoryManager Instance;

        [Header("Victory Requirements")]
        public List<MineralRequirement> requirements =
            new List<MineralRequirement>();

        [Header("Exit")]
        public GameObject blockingRock;

        [Header("UI")]
        public TMP_Text coalCounterText;
        public GameObject victoryPanel;

        [Header("Victory Scene")]
        public string victorySceneName = "Victoria";
        public float loadDelay = 1.5f;

        public bool ExitUnlocked { get; private set; }
        public bool HasWon { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ExitUnlocked = false;
            HasWon = false;

            if (victoryPanel != null)
                victoryPanel.SetActive(false);

            if (blockingRock != null)
                blockingRock.SetActive(true);

            UpdateRequirementsUI();
        }

        public void CheckRequirements(MineralChest chest)
        {
            if (chest == null || HasWon || ExitUnlocked)
                return;

            if (PlayerLives.Instance != null &&
                PlayerLives.Instance.IsGameOver)
            {
                return;
            }

            bool allRequirementsMet = true;

            foreach (MineralRequirement requirement in requirements)
            {
                if (requirement == null || requirement.item == null)
                {
                    Debug.LogWarning(
                        "Hay un requisito sin ItemData asignado."
                    );

                    allRequirementsMet = false;
                    continue;
                }

                int storedAmount = chest.GetAmountOf(requirement.item);

                Debug.Log(
                    requirement.item.itemName + ": " +
                    storedAmount + "/" + requirement.requiredAmount
                );

                if (storedAmount < requirement.requiredAmount)
                    allRequirementsMet = false;
            }

            UpdateRequirementsUI();

            if (requirements.Count == 0)
            {
                Debug.LogWarning(
                    "Añade al menos un requisito en GameVictoryManager."
                );
                return;
            }

            if (allRequirementsMet)
                UnlockExit();
        }

        private void UpdateRequirementsUI()
        {
            if (coalCounterText == null)
                return;

            StringBuilder text = new StringBuilder();

            foreach (MineralRequirement requirement in requirements)
            {
                if (requirement == null || requirement.item == null)
                    continue;

                text.AppendLine(
                    requirement.item.itemName + ": " +
                    requirement.requiredAmount
                );
            }

            coalCounterText.text = text.ToString();
        }

        private void UnlockExit()
        {
            ExitUnlocked = true;

            if (blockingRock != null)
                blockingRock.SetActive(false);

            Debug.Log("¡Se cumplieron los requisitos! La salida está desbloqueada.");

            // Al abrirse la salida (la roca se destruye) la victoria es inmediata: no hay que
            // cruzar ningun trigger. loadDelay da un instante para ver desaparecer la roca.
            WinGame();
        }

        public void WinGame()
        {
            if (HasWon || !ExitUnlocked)
                return;

            if (PlayerLives.Instance != null &&
                PlayerLives.Instance.IsGameOver)
            {
                Debug.LogWarning(
                    "No se puede ganar: la partida ya terminó en derrota."
                );

                return;
            }

            HasWon = true;

            Debug.Log("¡Felicidades! ¡Escapaste de la mina!");

            if (victoryPanel != null)
                victoryPanel.SetActive(true);

            // Deja ver un instante el escape y pasa a la escena de victoria.
            Invoke(nameof(LoadVictoryScene), loadDelay);
        }

        private void LoadVictoryScene()
        {
            if (Application.CanStreamedLevelBeLoaded(victorySceneName))
                UnityEngine.SceneManagement.SceneManager.LoadScene(victorySceneName);
            else
                Debug.LogError("No se encontro la escena " + victorySceneName + ". Agregala a Build Settings.");
        }
    }
}