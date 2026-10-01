
using UnityEngine;

namespace StarterAssets
{
    public class ExitWinTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (GameVictoryManager.Instance == null)
                return;

            if (GameVictoryManager.Instance.ExitUnlocked)
            {
                GameVictoryManager.Instance.WinGame();
            }
            else
            {
                Debug.Log(
                    "Necesitas recolectar 4 minerales de carbón."
                );
            }
        }
    }
}