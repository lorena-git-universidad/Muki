using UnityEngine;

namespace StarterAssets
{
    public class CaveAmbience : MonoBehaviour
    {
        private void Start()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("Ambiente");
            }
        }
    }
}